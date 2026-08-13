using Microsoft.Extensions.AI;
using RegulationsSearcher.Ingestion.Chunking;
using RegulationsSearcher.Ingestion.Embeddings;

namespace RegulationsSearcher.Ingestion.Tests.Embeddings;

public class ChunkEmbedderTests
{
    private sealed class FakeEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
    {
        public List<string> ReceivedValues { get; } = [];
        public int CallCount { get; private set; }

        public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
            IEnumerable<string> values,
            EmbeddingGenerationOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            var inputs = values.ToList();
            ReceivedValues.AddRange(inputs);

            var embeddings = new GeneratedEmbeddings<Embedding<float>>(
                inputs.Select(value => new Embedding<float>(new ReadOnlyMemory<float>([value.Length]))));

            return Task.FromResult(embeddings);
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose()
        {
        }
    }

    private static TextChunk BuildChunk(string content, int chunkIndex) =>
        new("doc.pdf", content, PageStart: 1, PageEnd: 1, chunkIndex);

    [Fact]
    public async Task EmbedAsync_NoChunks_ReturnsEmptyListWithoutCallingGenerator()
    {
        var generator = new FakeEmbeddingGenerator();
        var embedder = new ChunkEmbedder(generator);

        var result = await embedder.EmbedAsync([]);

        Assert.Empty(result);
        Assert.Empty(generator.ReceivedValues);
    }

    [Fact]
    public async Task EmbedAsync_PairsEachChunkWithItsGeneratedVectorInOrder()
    {
        var generator = new FakeEmbeddingGenerator();
        var embedder = new ChunkEmbedder(generator);
        var chunks = new[] { BuildChunk("ab", 0), BuildChunk("abcd", 1) };

        var result = await embedder.EmbedAsync(chunks);

        Assert.Equal(["ab", "abcd"], generator.ReceivedValues);
        Assert.Equal(2, result.Count);
        Assert.Same(chunks[0], result[0].Chunk);
        Assert.Equal(2f, result[0].Vector.Span[0]);
        Assert.Same(chunks[1], result[1].Chunk);
        Assert.Equal(4f, result[1].Vector.Span[0]);
    }

    [Fact]
    public async Task EmbedAsync_MoreChunksThanBatchSize_CallsGeneratorOncePerBatch()
    {
        var generator = new FakeEmbeddingGenerator();
        var embedder = new ChunkEmbedder(generator, batchSize: 2);
        var chunks = new[] { BuildChunk("a", 0), BuildChunk("bb", 1), BuildChunk("ccc", 2) };

        var result = await embedder.EmbedAsync(chunks);

        Assert.Equal(2, generator.CallCount);
        Assert.Equal(["a", "bb", "ccc"], generator.ReceivedValues);
        Assert.Equal(3, result.Count);
        Assert.Same(chunks[2], result[2].Chunk);
        Assert.Equal(3f, result[2].Vector.Span[0]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_NonPositiveBatchSize_Throws(int batchSize)
    {
        var generator = new FakeEmbeddingGenerator();

        Assert.Throws<ArgumentOutOfRangeException>(() => new ChunkEmbedder(generator, batchSize));
    }
}
