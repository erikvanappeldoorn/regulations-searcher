using Microsoft.Extensions.AI;
using RegulationsSearcher.Ingestion.Chunking;

namespace RegulationsSearcher.Ingestion.Embeddings;

public sealed class ChunkEmbedder
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly int _batchSize;

    public ChunkEmbedder(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, int batchSize = 16)
    {
        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(batchSize), batchSize, "Batch size must be greater than zero.");
        }

        _embeddingGenerator = embeddingGenerator;
        _batchSize = batchSize;
    }

    public async Task<IReadOnlyList<EmbeddedChunk>> EmbedAsync(IReadOnlyList<TextChunk> chunks, CancellationToken cancellationToken = default)
    {
        if (chunks.Count == 0)
        {
            return [];
        }

        var embeddedChunks = new List<EmbeddedChunk>(chunks.Count);

        foreach (var batch in chunks.Chunk(_batchSize))
        {
            var embeddings = await _embeddingGenerator.GenerateAsync(
                batch.Select(chunk => chunk.Content),
                cancellationToken: cancellationToken);

            for (var i = 0; i < batch.Length; i++)
            {
                embeddedChunks.Add(new EmbeddedChunk(batch[i], embeddings[i].Vector));
            }
        }

        return embeddedChunks;
    }
}
