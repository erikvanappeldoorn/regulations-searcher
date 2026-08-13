using Azure;
using Azure.Search.Documents.Indexes;

namespace RegulationsSearcher.Ingestion.Indexing;

public sealed class SearchIndexProvisioner
{
    private readonly SearchIndexClient _searchIndexClient;

    public SearchIndexProvisioner(SearchIndexClient searchIndexClient)
    {
        _searchIndexClient = searchIndexClient;
    }

    public async Task EnsureIndexExistsAsync(
        string indexName,
        int vectorSearchDimensions,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _searchIndexClient.GetIndexAsync(indexName, cancellationToken);
            return;
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
        }

        await _searchIndexClient.CreateIndexAsync(
            SearchIndexSchema.BuildIndex(indexName, vectorSearchDimensions),
            cancellationToken);
    }
}
