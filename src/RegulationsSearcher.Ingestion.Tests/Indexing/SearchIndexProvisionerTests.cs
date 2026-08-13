using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using RegulationsSearcher.Ingestion.Indexing;

namespace RegulationsSearcher.Ingestion.Tests.Indexing;

public class SearchIndexProvisionerTests
{
    private sealed class FakeSearchIndexClient : SearchIndexClient
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

    [Fact]
    public async Task EnsureIndexExistsAsync_IndexDoesNotExist_CreatesIndex()
    {
        var client = new FakeSearchIndexClient(index: null);
        var provisioner = new SearchIndexProvisioner(client);

        await provisioner.EnsureIndexExistsAsync("regulations", 1536);

        Assert.NotNull(client.CreatedIndex);
        Assert.Equal("regulations", client.CreatedIndex!.Name);
    }

    [Fact]
    public async Task EnsureIndexExistsAsync_IndexAlreadyExists_DoesNotCreateIndex()
    {
        var index = new SearchIndex("regulations");
        var client = new FakeSearchIndexClient(index);
        var provisioner = new SearchIndexProvisioner(client);

        await provisioner.EnsureIndexExistsAsync("regulations", 1536);

        Assert.Null(client.CreatedIndex);
    }
}
