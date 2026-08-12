using Microsoft.Extensions.AI;
using RegulationsSearcher.Ingestion.Chunking;

namespace RegulationsSearcher.Ingestion.Embeddings;

public sealed record EmbeddedChunk(TextChunk Chunk, ReadOnlyMemory<float> Vector);

public sealed class ChunkEmbedder
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

    public ChunkEmbedder(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    {
        _embeddingGenerator = embeddingGenerator;
    }

    public async Task<IReadOnlyList<EmbeddedChunk>> EmbedAsync(IReadOnlyList<TextChunk> chunks, CancellationToken cancellationToken = default)
    {
        if (chunks.Count == 0)
        {
            return [];
        }

        var embeddings = await _embeddingGenerator.GenerateAsync(
            chunks.Select(chunk => chunk.Content),
            cancellationToken: cancellationToken);

        var embeddedChunks = new List<EmbeddedChunk>(chunks.Count);
        for (var i = 0; i < chunks.Count; i++)
        {
            embeddedChunks.Add(new EmbeddedChunk(chunks[i], embeddings[i].Vector));
        }

        return embeddedChunks;
    }
}
