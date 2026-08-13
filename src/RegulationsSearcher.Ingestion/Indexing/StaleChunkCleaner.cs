using Azure.Search.Documents;

namespace RegulationsSearcher.Ingestion.Indexing;

public sealed class StaleChunkCleaner
{
    private readonly SearchClient _searchClient;

    public StaleChunkCleaner(SearchClient searchClient)
    {
        _searchClient = searchClient;
    }

    public async Task CleanupAsync(
        string documentName,
        int previousChunkCount,
        int currentChunkCount,
        CancellationToken cancellationToken = default)
    {
        if (previousChunkCount <= currentChunkCount)
        {
            return;
        }

        var staleChunkIds = Enumerable
            .Range(currentChunkCount, previousChunkCount - currentChunkCount)
            .Select(chunkIndex => ChunkIdGenerator.GenerateId(documentName, chunkIndex))
            .ToList();

        await _searchClient.DeleteDocumentsAsync(SearchIndexSchema.KeyFieldName, staleChunkIds, cancellationToken: cancellationToken);
    }
}
