using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace RegulationsSearcher.Ingestion.Tests.Validation;

internal sealed class FakeSearchIndexClient : SearchIndexClient
{
    private readonly SearchIndex? _index;

    public FakeSearchIndexClient(SearchIndex? index)
    {
        _index = index;
    }

    public override Task<Response<SearchIndex>> GetIndexAsync(string indexName, CancellationToken cancellationToken = default)
    {
        if (_index is null)
        {
            throw new RequestFailedException(status: 404, message: "Index not found.");
        }

        return Task.FromResult(Response.FromValue(_index, response: null!));
    }
}
