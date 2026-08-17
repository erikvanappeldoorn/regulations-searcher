using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;

namespace RegulationsSearcher.Ingestion.Tests.Indexing;

internal sealed class UploadCapturingSearchClient : SearchClient
{
    public List<object> UploadedDocuments { get; } = [];

    public override Task<Response<IndexDocumentsResult>> MergeOrUploadDocumentsAsync<T>(
        IEnumerable<T> documents,
        IndexDocumentsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        UploadedDocuments.AddRange(documents!.Cast<object>());
        return Task.FromResult(Response.FromValue(
            SearchModelFactory.IndexDocumentsResult([]),
            response: null!));
    }
}
