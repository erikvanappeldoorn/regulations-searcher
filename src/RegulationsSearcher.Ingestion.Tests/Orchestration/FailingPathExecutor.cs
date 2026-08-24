using Microsoft.Agents.AI.Workflows;

namespace RegulationsSearcher.Ingestion.Tests.Orchestration;

internal sealed class FailingPathExecutor : Executor<string, string>
{
    public FailingPathExecutor()
        : base(nameof(FailingPathExecutor))
    {
    }

    public override ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken) =>
        message.Contains("fail", StringComparison.OrdinalIgnoreCase)
            ? throw new InvalidOperationException($"Simulated failure for {message}")
            : new ValueTask<string>(message);
}
