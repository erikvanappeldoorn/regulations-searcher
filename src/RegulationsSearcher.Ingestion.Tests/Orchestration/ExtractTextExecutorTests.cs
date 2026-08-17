using RegulationsSearcher.Ingestion.Extraction;
using RegulationsSearcher.Ingestion.Orchestration;

namespace RegulationsSearcher.Ingestion.Tests.Orchestration;

public class ExtractTextExecutorTests
{
    [Fact]
    public async Task HandleAsync_ExtractionFails_LogsStepFailedAndRethrows()
    {
        var logger = new SpyPipelineLogger();
        var executor = new ExtractTextExecutor(new PdfTextExtractor(), logger);
        var missingPdf = new LoadedPdf("missing.pdf", Path.Combine(AppContext.BaseDirectory, "missing.pdf"));

        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            executor.HandleAsync(missingPdf, context: null!, CancellationToken.None).AsTask());

        var failedStep = Assert.Single(logger.FailedSteps);
        Assert.Equal(nameof(ExtractTextExecutor), failedStep.StepName);
        Assert.IsType<FileNotFoundException>(failedStep.Exception);
        Assert.Empty(logger.SucceededSteps);
    }
}
