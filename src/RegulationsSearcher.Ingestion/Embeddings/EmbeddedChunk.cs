using RegulationsSearcher.Ingestion.Chunking;

namespace RegulationsSearcher.Ingestion.Embeddings;

public sealed record EmbeddedChunk(TextChunk Chunk, ReadOnlyMemory<float> Vector);
