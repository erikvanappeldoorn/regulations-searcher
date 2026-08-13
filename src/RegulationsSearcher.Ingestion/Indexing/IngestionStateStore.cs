using System.Text.Json;

namespace RegulationsSearcher.Ingestion.Indexing;

public sealed class IngestionStateStore
{
    public async Task<Dictionary<string, int>> LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(filePath);
        var chunkCountsByDocument = await JsonSerializer.DeserializeAsync<Dictionary<string, int>>(stream, cancellationToken: cancellationToken);
        return chunkCountsByDocument ?? [];
    }

    public async Task SaveAsync(string filePath, IReadOnlyDictionary<string, int> chunkCountsByDocument, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, chunkCountsByDocument, cancellationToken: cancellationToken);
    }
}
