using RegulationsSearcher.Ingestion.Chunking;
using RegulationsSearcher.Ingestion.Embeddings;
using RegulationsSearcher.Ingestion.Indexing;
using RegulationsSearcher.Ingestion.Orchestration;

namespace RegulationsSearcher.Ingestion.Tests.Orchestration;

public class UpsertToIndexExecutorTests
{
    [Fact]
    public async Task HandleAsync_UploadFails_LogsStepFailedWithDocumentNameAndRethrows()
    {
        var logger = new SpyPipelineLogger();
        var executor = new UpsertToIndexExecutor(new ChunkUploader(new ThrowingSearchClient()), logger);
        var chunk = new TextChunk("regulation.pdf", "content", PageStart: 1, PageEnd: 1, ChunkIndex: 0);
        var embeddedChunks = new[] { new EmbeddedChunk(chunk, new ReadOnlyMemory<float>([0f])) };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            executor.HandleAsync(embeddedChunks, context: null!, CancellationToken.None).AsTask());

        var failedStep = Assert.Single(logger.FailedSteps);
        Assert.Equal("regulation.pdf", failedStep.DocumentName);
        Assert.Equal(nameof(UpsertToIndexExecutor), failedStep.StepName);
        Assert.Empty(logger.SucceededSteps);
    }
}
