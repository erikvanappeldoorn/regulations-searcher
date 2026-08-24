using RegulationsSearcher.Ingestion.Chunking;
using RegulationsSearcher.Ingestion.Extraction;
using RegulationsSearcher.Ingestion.Orchestration;

namespace RegulationsSearcher.Ingestion.Tests.Orchestration;

public class ChunkTextExecutorTests
{
    [Fact]
    public async Task HandleAsync_ValidDocument_LogsStepSucceededWithDocumentName()
    {
        var logger = new SpyPipelineLogger();
        var executor = new ChunkTextExecutor(new TextChunker(chunkSizeInTokens: 800, overlapSizeInTokens: 100), logger);
        var extractedDocument = new ExtractedDocument("regulation.pdf", [new PdfPageText(1, "some regulatory text")]);

        await executor.HandleAsync(extractedDocument, context: null!, CancellationToken.None);

        var succeededStep = Assert.Single(logger.SucceededSteps);
        Assert.Equal("regulation.pdf", succeededStep.DocumentName);
        Assert.Equal(nameof(ChunkTextExecutor), succeededStep.StepName);
        Assert.Empty(logger.FailedSteps);
    }
}
