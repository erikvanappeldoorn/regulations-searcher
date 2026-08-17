namespace RegulationsSearcher.Ingestion.Chunking;

public sealed record TextChunk(string DocumentName, string Content, int PageStart, int PageEnd, int ChunkIndex);
