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

    public PdfIngestionWorkflowFactory(
        PdfTextExtractor pdfTextExtractor,
        TextChunker textChunker,
        ChunkEmbedder chunkEmbedder,
        ChunkUploader chunkUploader)
    {
        _pdfTextExtractor = pdfTextExtractor;
        _textChunker = textChunker;
        _chunkEmbedder = chunkEmbedder;
        _chunkUploader = chunkUploader;
    }

    public Workflow Build()
    {
        var loadPdf = new LoadPdfExecutor();
        var extractText = new ExtractTextExecutor(_pdfTextExtractor);
        var chunkText = new ChunkTextExecutor(_textChunker);
        var generateEmbeddings = new GenerateEmbeddingsExecutor(_chunkEmbedder);
        var upsertToIndex = new UpsertToIndexExecutor(_chunkUploader);

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
