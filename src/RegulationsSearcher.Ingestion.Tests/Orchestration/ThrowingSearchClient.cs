using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;

namespace RegulationsSearcher.Ingestion.Tests.Orchestration;

internal sealed class ThrowingSearchClient : SearchClient
{
    public override Task<Response<IndexDocumentsResult>> MergeOrUploadDocumentsAsync<T>(
        IEnumerable<T> documents,
        IndexDocumentsOptions? options = null,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("search service unavailable");
}
