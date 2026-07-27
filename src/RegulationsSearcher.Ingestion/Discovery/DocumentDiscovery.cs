namespace RegulationsSearcher.Ingestion.Discovery;

public static class DocumentDiscovery
{
    /// <summary>Scans <paramref name="folderPath"/> for PDF files. Returns an empty list if none are found.</summary>
    public static IReadOnlyList<string> DiscoverPdfFiles(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            throw new DirectoryNotFoundException($"Source documents folder not found: {folderPath}");
        }

        return Directory.GetFiles(folderPath, "*.pdf", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
