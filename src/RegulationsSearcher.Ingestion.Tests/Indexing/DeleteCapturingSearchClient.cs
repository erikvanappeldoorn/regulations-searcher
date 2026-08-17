using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;

namespace RegulationsSearcher.Ingestion.Tests.Indexing;

internal sealed class DeleteCapturingSearchClient : SearchClient
{
    public string? DeletedKeyName { get; private set; }
    public List<string>? DeletedKeyValues { get; private set; }

    public override Task<Response<IndexDocumentsResult>> DeleteDocumentsAsync(
        string keyName,
        IEnumerable<string> keyValues,
        IndexDocumentsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        DeletedKeyName = keyName;
        DeletedKeyValues = keyValues.ToList();
        return Task.FromResult(Response.FromValue(
            SearchModelFactory.IndexDocumentsResult([]),
            response: null!));
    }
}
