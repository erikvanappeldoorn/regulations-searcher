using Microsoft.Agents.AI.Workflows;

namespace RegulationsSearcher.Ingestion.Orchestration;

public sealed class LoadPdfExecutor : Executor<string, LoadedPdf>
{
    public LoadPdfExecutor()
        : base(nameof(LoadPdfExecutor))
    {
    }

    public override ValueTask<LoadedPdf> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken) =>
        new(new LoadedPdf(Path.GetFileName(message), message));
}
