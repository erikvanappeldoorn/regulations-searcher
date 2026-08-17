using RegulationsSearcher.Ingestion.Extraction;

namespace RegulationsSearcher.Ingestion.Orchestration;

public sealed record ExtractedDocument(string DocumentName, IReadOnlyList<PdfPageText> Pages);
