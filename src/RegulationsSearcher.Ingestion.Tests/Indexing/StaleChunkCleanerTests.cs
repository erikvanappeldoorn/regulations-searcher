using RegulationsSearcher.Ingestion.Indexing;

namespace RegulationsSearcher.Ingestion.Tests.Indexing;

public class StaleChunkCleanerTests
{
    [Fact]
    public async Task CleanupAsync_PreviousCountGreaterThanCurrent_DeletesTrailingChunkIds()
    {
        var client = new DeleteCapturingSearchClient();
        var cleaner = new StaleChunkCleaner(client);

        await cleaner.CleanupAsync("doc.pdf", previousChunkCount: 5, currentChunkCount: 2);

        Assert.Equal(SearchIndexSchema.KeyFieldName, client.DeletedKeyName);
        Assert.Equal(["doc_pdf-2", "doc_pdf-3", "doc_pdf-4"], client.DeletedKeyValues);
    }

    [Fact]
    public async Task CleanupAsync_PreviousCountEqualToCurrent_DoesNotDelete()
    {
        var client = new DeleteCapturingSearchClient();
        var cleaner = new StaleChunkCleaner(client);

        await cleaner.CleanupAsync("doc.pdf", previousChunkCount: 3, currentChunkCount: 3);

        Assert.Null(client.DeletedKeyValues);
    }

    [Fact]
    public async Task CleanupAsync_PreviousCountLessThanCurrent_DoesNotDelete()
    {
        var client = new DeleteCapturingSearchClient();
        var cleaner = new StaleChunkCleaner(client);

        await cleaner.CleanupAsync("doc.pdf", previousChunkCount: 2, currentChunkCount: 5);

        Assert.Null(client.DeletedKeyValues);
    }
}
