using Microsoft.Agents.AI.Workflows;
using RegulationsSearcher.Ingestion.Chunking;
using RegulationsSearcher.Ingestion.Embeddings;
using RegulationsSearcher.Ingestion.Extraction;
using RegulationsSearcher.Ingestion.Indexing;

namespace RegulationsSearcher.Ingestion.Orchestration;

public sealed class PdfIngestionWorkflowFactory
{
    private readonly PdfTextExtractor _pdfTextExtractor;
    private readonly TextChunker _textChunker;
    private readonly ChunkEmbedder _chunkEmbedder;
    private readonly ChunkUploader _chunkUploader;
    private readonly IPipelineLogger _pipelineLogger;

    public PdfIngestionWorkflowFactory(
        PdfTextExtractor pdfTextExtractor,
        TextChunker textChunker,
        ChunkEmbedder chunkEmbedder,
        ChunkUploader chunkUploader,
        IPipelineLogger pipelineLogger)
    {
        _pdfTextExtractor = pdfTextExtractor;
        _textChunker = textChunker;
        _chunkEmbedder = chunkEmbedder;
        _chunkUploader = chunkUploader;
        _pipelineLogger = pipelineLogger;
    }

    public Workflow Build()
    {
        var loadPdf = new LoadPdfExecutor(_pipelineLogger);
        var extractText = new ExtractTextExecutor(_pdfTextExtractor, _pipelineLogger);
        var chunkText = new ChunkTextExecutor(_textChunker, _pipelineLogger);
        var generateEmbeddings = new GenerateEmbeddingsExecutor(_chunkEmbedder, _pipelineLogger);
        var upsertToIndex = new UpsertToIndexExecutor(_chunkUploader, _pipelineLogger);

        return new WorkflowBuilder(loadPdf)
            .AddEdge(loadPdf, extractText)
            .AddEdge(extractText, chunkText)
            .AddEdge(chunkText, generateEmbeddings)
            .AddEdge(generateEmbeddings, upsertToIndex)
            .WithOutputFrom(upsertToIndex)
            .WithName("PdfIngestionWorkflow")
            .Build();
    }
}
