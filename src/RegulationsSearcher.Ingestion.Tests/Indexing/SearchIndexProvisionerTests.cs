using Azure.Search.Documents.Indexes.Models;
using RegulationsSearcher.Ingestion.Indexing;

namespace RegulationsSearcher.Ingestion.Tests.Indexing;

public class SearchIndexProvisionerTests
{
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
