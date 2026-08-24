using RegulationsSearcher.Ingestion.Orchestration;

namespace RegulationsSearcher.Ingestion.Tests.Orchestration;

public class LoadPdfExecutorTests
{
    [Fact]
    public async Task HandleAsync_ValidPath_LogsStepSucceeded()
    {
        var logger = new SpyPipelineLogger();
        var executor = new LoadPdfExecutor(logger);

        var result = await executor.HandleAsync("Docs/regulation.pdf", context: null!, CancellationToken.None);

        Assert.Equal("regulation.pdf", result.DocumentName);
        var succeededStep = Assert.Single(logger.SucceededSteps);
        Assert.Equal("regulation.pdf", succeededStep.DocumentName);
        Assert.Equal(nameof(LoadPdfExecutor), succeededStep.StepName);
        Assert.Empty(logger.FailedSteps);
    }
}
