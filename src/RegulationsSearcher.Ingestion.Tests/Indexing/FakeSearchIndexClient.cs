using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace RegulationsSearcher.Ingestion.Tests.Indexing;

internal sealed class FakeSearchIndexClient : SearchIndexClient
{
    private readonly SearchIndex? _index;

    public FakeSearchIndexClient(SearchIndex? index)
    {
        _index = index;
    }

    public SearchIndex? CreatedIndex { get; private set; }

    public override Task<Response<SearchIndex>> GetIndexAsync(string indexName, CancellationToken cancellationToken = default)
    {
        if (_index is null)
        {
            throw new RequestFailedException(status: 404, message: "Index not found.");
        }

        return Task.FromResult(Response.FromValue(_index, response: null!));
    }

    public override Task<Response<SearchIndex>> CreateIndexAsync(SearchIndex index, CancellationToken cancellationToken = default)
    {
        CreatedIndex = index;
        return Task.FromResult(Response.FromValue(index, response: null!));
    }
}
