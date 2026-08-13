using Azure.Search.Documents;
using RegulationsSearcher.Ingestion.Embeddings;

namespace RegulationsSearcher.Ingestion.Indexing;

public sealed class ChunkUploader
{
    private readonly SearchClient _searchClient;

    public ChunkUploader(SearchClient searchClient)
    {
        _searchClient = searchClient;
    }

    public async Task UploadAsync(IReadOnlyList<EmbeddedChunk> embeddedChunks, CancellationToken cancellationToken = default)
    {
        if (embeddedChunks.Count == 0)
        {
            return;
        }

        var documents = embeddedChunks.Select(ToIndexedChunk);
        await _searchClient.MergeOrUploadDocumentsAsync(documents, cancellationToken: cancellationToken);
    }

    private static IndexedChunk ToIndexedChunk(EmbeddedChunk embeddedChunk)
    {
        var chunk = embeddedChunk.Chunk;
        var id = ChunkIdGenerator.GenerateId(chunk.DocumentName, chunk.ChunkIndex);
        return new IndexedChunk(id, chunk.Content, embeddedChunk.Vector, chunk.DocumentName, chunk.PageStart, chunk.PageEnd, chunk.ChunkIndex);
    }
}
