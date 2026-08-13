using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using RegulationsSearcher.Ingestion.Indexing;

namespace RegulationsSearcher.Ingestion.Tests.Indexing;

public class StaleChunkCleanerTests
{
    private sealed class FakeSearchClient : SearchClient
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

    [Fact]
    public async Task CleanupAsync_PreviousCountGreaterThanCurrent_DeletesTrailingChunkIds()
    {
        var client = new FakeSearchClient();
        var cleaner = new StaleChunkCleaner(client);

        await cleaner.CleanupAsync("doc.pdf", previousChunkCount: 5, currentChunkCount: 2);

        Assert.Equal(SearchIndexSchema.KeyFieldName, client.DeletedKeyName);
        Assert.Equal(["doc_pdf-2", "doc_pdf-3", "doc_pdf-4"], client.DeletedKeyValues);
    }

    [Fact]
    public async Task CleanupAsync_PreviousCountEqualToCurrent_DoesNotDelete()
    {
        var client = new FakeSearchClient();
        var cleaner = new StaleChunkCleaner(client);

        await cleaner.CleanupAsync("doc.pdf", previousChunkCount: 3, currentChunkCount: 3);

        Assert.Null(client.DeletedKeyValues);
    }

    [Fact]
    public async Task CleanupAsync_PreviousCountLessThanCurrent_DoesNotDelete()
    {
        var client = new FakeSearchClient();
        var cleaner = new StaleChunkCleaner(client);

        await cleaner.CleanupAsync("doc.pdf", previousChunkCount: 2, currentChunkCount: 5);

        Assert.Null(client.DeletedKeyValues);
    }
}
