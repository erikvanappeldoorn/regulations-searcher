using RegulationsSearcher.Ingestion.Indexing;

namespace RegulationsSearcher.Ingestion.Tests.Indexing;

public class IngestionStateStoreTests : IDisposable
{
    private readonly string _tempDirectory = Directory.CreateTempSubdirectory("ingestion-state-tests").FullName;

    [Fact]
    public async Task LoadAsync_FileDoesNotExist_ReturnsEmptyState()
    {
        var store = new IngestionStateStore();
        var filePath = Path.Combine(_tempDirectory, "missing.json");

        var state = await store.LoadAsync(filePath);

        Assert.Empty(state);
    }

    [Fact]
    public async Task SaveAsync_ThenLoadAsync_RoundTripsState()
    {
        var store = new IngestionStateStore();
        var filePath = Path.Combine(_tempDirectory, "state.json");
        var state = new Dictionary<string, int> { ["doc.pdf"] = 5, ["other.pdf"] = 2 };

        await store.SaveAsync(filePath, state);
        var loaded = await store.LoadAsync(filePath);

        Assert.Equal(state, loaded);
    }

    [Fact]
    public async Task SaveAsync_ParentDirectoryDoesNotExist_CreatesIt()
    {
        var store = new IngestionStateStore();
        var filePath = Path.Combine(_tempDirectory, "nested", "state.json");

        await store.SaveAsync(filePath, new Dictionary<string, int> { ["doc.pdf"] = 1 });

        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public async Task SaveAsync_OverwritesPreviouslySavedState()
    {
        var store = new IngestionStateStore();
        var filePath = Path.Combine(_tempDirectory, "state.json");

        await store.SaveAsync(filePath, new Dictionary<string, int> { ["doc.pdf"] = 5 });
        await store.SaveAsync(filePath, new Dictionary<string, int> { ["doc.pdf"] = 2 });
        var loaded = await store.LoadAsync(filePath);

        Assert.Equal(2, loaded["doc.pdf"]);
    }

    public void Dispose()
    {
        Directory.Delete(_tempDirectory, recursive: true);
    }
}
