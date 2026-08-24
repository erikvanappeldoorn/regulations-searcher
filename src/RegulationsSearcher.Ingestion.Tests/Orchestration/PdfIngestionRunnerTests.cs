using Microsoft.Agents.AI.Workflows;
using RegulationsSearcher.Ingestion.Orchestration;

namespace RegulationsSearcher.Ingestion.Tests.Orchestration;

public class PdfIngestionRunnerTests
{
    private static Workflow BuildWorkflow()
    {
        var executor = new FailingPathExecutor();
        return new WorkflowBuilder(executor)
            .WithOutputFrom(executor)
            .WithName("FailingPathWorkflow")
            .Build();
    }

    [Fact]
    public async Task RunAsync_AllDocumentsSucceed_ReturnsEmptyList()
    {
        var runner = new PdfIngestionRunner(BuildWorkflow(), new NoOpPipelineLogger());

        var failedDocuments = await runner.RunAsync(["Docs/one.pdf", "Docs/two.pdf"]);

        Assert.Empty(failedDocuments);
    }

    [Fact]
    public async Task RunAsync_OneDocumentFails_ReportsItAndStillProcessesTheRest()
    {
        var runner = new PdfIngestionRunner(BuildWorkflow(), new NoOpPipelineLogger());

        var failedDocuments = await runner.RunAsync(["Docs/one.pdf", "Docs/fail.pdf", "Docs/two.pdf"]);

        Assert.Equal(["fail.pdf"], failedDocuments);
    }
}
