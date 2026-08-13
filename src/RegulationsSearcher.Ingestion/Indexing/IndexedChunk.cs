using System.Text.Json.Serialization;

namespace RegulationsSearcher.Ingestion.Indexing;

public sealed record IndexedChunk(
    [property: JsonPropertyName(SearchIndexSchema.KeyFieldName)] string Id,
    [property: JsonPropertyName(SearchIndexSchema.ContentFieldName)] string Content,
    [property: JsonPropertyName(SearchIndexSchema.ContentVectorFieldName)] ReadOnlyMemory<float> ContentVector,
    [property: JsonPropertyName(SearchIndexSchema.DocumentNameFieldName)] string DocumentName,
    [property: JsonPropertyName(SearchIndexSchema.PageStartFieldName)] int PageStart,
    [property: JsonPropertyName(SearchIndexSchema.PageEndFieldName)] int PageEnd,
    [property: JsonPropertyName(SearchIndexSchema.ChunkIndexFieldName)] int ChunkIndex);
