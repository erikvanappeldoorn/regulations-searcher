using Microsoft.Agents.AI.Workflows.Checkpointing;
using RegulationsSearcher.Ingestion.Chunking;
using RegulationsSearcher.Ingestion.Embeddings;
using RegulationsSearcher.Ingestion.Extraction;
using RegulationsSearcher.Ingestion.Indexing;
using RegulationsSearcher.Ingestion.Orchestration;

namespace RegulationsSearcher.Ingestion.Tests.Orchestration;

public class PdfIngestionWorkflowFactoryTests
{
    private static PdfIngestionWorkflowFactory CreateFactory() =>
        new(
            new PdfTextExtractor(),
            new TextChunker(chunkSizeInTokens: 800, overlapSizeInTokens: 100),
            new ChunkEmbedder(new FakeEmbeddingGenerator()),
            new ChunkUploader(new FakeSearchClient()),
            new NoOpPipelineLogger());

    private static string SourceIdOf(EdgeInfo edgeInfo) => Assert.Single(edgeInfo.Connection.SourceIds);

    private static string SinkIdOf(EdgeInfo edgeInfo) => Assert.Single(edgeInfo.Connection.SinkIds);

    [Fact]
    public void Build_StartsAtLoadPdfExecutor()
    {
        var workflow = CreateFactory().Build();

        Assert.Equal(nameof(LoadPdfExecutor), workflow.StartExecutorId);
    }

    [Fact]
    public void Build_RegistersAllFiveSteps()
    {
        var workflow = CreateFactory().Build();

        var expectedIds = new[]
        {
            nameof(LoadPdfExecutor), nameof(ExtractTextExecutor), nameof(ChunkTextExecutor), nameof(GenerateEmbeddingsExecutor), nameof(UpsertToIndexExecutor),
        };

        Assert.Equal(
            expectedIds.OrderBy(id => id, StringComparer.Ordinal),
            workflow.ReflectExecutors().Keys.OrderBy(id => id, StringComparer.Ordinal));
    }

    [Fact]
    public void Build_ConnectsStepsInPipelineOrder()
    {
        var workflow = CreateFactory().Build();
        var edges = workflow.ReflectEdges();

        var expectedChain = new[]
        {
            (Source: nameof(LoadPdfExecutor), Sink: nameof(ExtractTextExecutor)),
            (Source: nameof(ExtractTextExecutor), Sink: nameof(ChunkTextExecutor)),
            (Source: nameof(ChunkTextExecutor), Sink: nameof(GenerateEmbeddingsExecutor)),
            (Source: nameof(GenerateEmbeddingsExecutor), Sink: nameof(UpsertToIndexExecutor)),
        };

        foreach (var (source, sink) in expectedChain)
        {
            var edge = Assert.Single(edges[source]);
            Assert.Equal(source, SourceIdOf(edge));
            Assert.Equal(sink, SinkIdOf(edge));
        }

        Assert.False(edges.ContainsKey(nameof(UpsertToIndexExecutor)));
    }
}
