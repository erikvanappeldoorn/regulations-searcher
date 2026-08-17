using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;

namespace RegulationsSearcher.Ingestion.Tests.Orchestration;

internal sealed class FakeSearchClient : SearchClient
{
    public override Task<Response<IndexDocumentsResult>> MergeOrUploadDocumentsAsync<T>(
        IEnumerable<T> documents,
        IndexDocumentsOptions? options = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Response.FromValue(SearchModelFactory.IndexDocumentsResult([]), response: null!));
}
