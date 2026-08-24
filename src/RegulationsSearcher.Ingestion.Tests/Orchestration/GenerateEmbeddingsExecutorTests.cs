using RegulationsSearcher.Ingestion.Chunking;
using RegulationsSearcher.Ingestion.Embeddings;
using RegulationsSearcher.Ingestion.Orchestration;

namespace RegulationsSearcher.Ingestion.Tests.Orchestration;

public class GenerateEmbeddingsExecutorTests
{
    [Fact]
    public async Task HandleAsync_EmbeddingFails_LogsStepFailedWithDocumentNameAndRethrows()
    {
        var logger = new SpyPipelineLogger();
        var executor = new GenerateEmbeddingsExecutor(new ChunkEmbedder(new ThrowingEmbeddingGenerator()), logger);
        var chunks = new[] { new TextChunk("regulation.pdf", "content", PageStart: 1, PageEnd: 1, ChunkIndex: 0) };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            executor.HandleAsync(chunks, context: null!, CancellationToken.None).AsTask());

        var failedStep = Assert.Single(logger.FailedSteps);
        Assert.Equal("regulation.pdf", failedStep.DocumentName);
        Assert.Equal(nameof(GenerateEmbeddingsExecutor), failedStep.StepName);
        Assert.Empty(logger.SucceededSteps);
    }
}
