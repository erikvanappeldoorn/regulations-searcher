namespace RegulationsSearcher.Ingestion.Configuration;

public sealed class IngestionOptions
{
    public const string SectionName = "Ingestion";

    /// <summary>Absolute or repo-relative paths to the source PDF documents to ingest.</summary>
    public required string[] SourceDocumentPaths { get; init; }
}
