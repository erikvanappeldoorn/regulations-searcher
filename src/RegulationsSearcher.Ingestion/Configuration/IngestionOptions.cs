namespace RegulationsSearcher.Ingestion.Configuration;

public sealed class IngestionOptions
{
    public const string SectionName = "Ingestion";

    /// <summary>Folder scanned for PDF files to ingest; the number of documents is not fixed.</summary>
    public required string SourceDocumentsFolder { get; init; }
}
