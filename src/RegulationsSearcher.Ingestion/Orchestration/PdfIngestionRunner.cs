using Microsoft.Agents.AI.Workflows;

namespace RegulationsSearcher.Ingestion.Orchestration;

public sealed class PdfIngestionRunner
{
    private readonly Workflow _workflow;
    private readonly IPipelineLogger _logger;

    public PdfIngestionRunner(Workflow workflow, IPipelineLogger logger)
    {
        _workflow = workflow;
        _logger = logger;
    }

    public async Task<IReadOnlyList<string>> RunAsync(IReadOnlyList<string> sourceDocumentPaths, CancellationToken cancellationToken = default)
    {
        var failedDocuments = new List<string>();

        foreach (var sourceDocumentPath in sourceDocumentPaths)
        {
            var documentName = Path.GetFileName(sourceDocumentPath);
            Console.WriteLine($"Running pipeline for {documentName}...");

            try
            {
                await using var run = await InProcessExecution.RunAsync(_workflow, sourceDocumentPath, cancellationToken: cancellationToken);

                if (run.NewEvents.OfType<WorkflowErrorEvent>().Any())
                {
                    failedDocuments.Add(documentName);
                }
            }
            catch (Exception exception)
            {
                _logger.LogStepFailed(documentName, nameof(PdfIngestionRunner), exception);
                failedDocuments.Add(documentName);
            }
        }

        return failedDocuments;
    }
}
