using Microsoft.Agents.AI.Workflows;
using RegulationsSearcher.Ingestion.Extraction;

namespace RegulationsSearcher.Ingestion.Orchestration;

public sealed class ExtractTextExecutor : Executor<LoadedPdf, ExtractedDocument>
{
    private readonly PdfTextExtractor _pdfTextExtractor;
    private readonly IPipelineLogger _logger;

    public ExtractTextExecutor(PdfTextExtractor pdfTextExtractor, IPipelineLogger logger)
        : base(nameof(ExtractTextExecutor))
    {
        _pdfTextExtractor = pdfTextExtractor;
        _logger = logger;
    }

    public override ValueTask<ExtractedDocument> HandleAsync(LoadedPdf message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        try
        {
            var pages = _pdfTextExtractor.ExtractPages(message.FilePath);
            var extractedDocument = new ExtractedDocument(message.DocumentName, pages);
            _logger.LogStepSucceeded(message.DocumentName, Id);
            return new ValueTask<ExtractedDocument>(extractedDocument);
        }
        catch (Exception exception)
        {
            _logger.LogStepFailed(message.DocumentName, Id, exception);
            throw;
        }
    }
}
