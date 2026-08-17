using Microsoft.Agents.AI.Workflows;
using RegulationsSearcher.Ingestion.Extraction;

namespace RegulationsSearcher.Ingestion.Orchestration;

public sealed class ExtractTextExecutor : Executor<LoadedPdf, ExtractedDocument>
{
    private readonly PdfTextExtractor _pdfTextExtractor;

    public ExtractTextExecutor(PdfTextExtractor pdfTextExtractor)
        : base(nameof(ExtractTextExecutor))
    {
        _pdfTextExtractor = pdfTextExtractor;
    }

    public override ValueTask<ExtractedDocument> HandleAsync(LoadedPdf message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        var pages = _pdfTextExtractor.ExtractPages(message.FilePath);
        return new ValueTask<ExtractedDocument>(new ExtractedDocument(message.DocumentName, pages));
    }
}
