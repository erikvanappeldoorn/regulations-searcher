using Microsoft.Agents.AI.Workflows;

namespace RegulationsSearcher.Ingestion.Orchestration;

public sealed class LoadPdfExecutor : Executor<string, LoadedPdf>
{
    private readonly IPipelineLogger _logger;

    public LoadPdfExecutor(IPipelineLogger logger)
        : base(nameof(LoadPdfExecutor))
    {
        _logger = logger;
    }

    public override ValueTask<LoadedPdf> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        try
        {
            var loadedPdf = new LoadedPdf(Path.GetFileName(message), message);
            _logger.LogStepSucceeded(Id);
            return new ValueTask<LoadedPdf>(loadedPdf);
        }
        catch (Exception exception)
        {
            _logger.LogStepFailed(Id, exception);
            throw;
        }
    }
}
