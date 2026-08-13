using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace RegulationsSearcher.Ingestion.Indexing;

public static class SearchIndexSchema
{
    public const string KeyFieldName = "id";
    public const string ContentFieldName = "content";
    public const string ContentVectorFieldName = "contentVector";
    public const string DocumentNameFieldName = "documentName";
    public const string PageStartFieldName = "pageStart";
    public const string PageEndFieldName = "pageEnd";
    public const string ChunkIndexFieldName = "chunkIndex";

    private const string VectorSearchAlgorithmName = "regulation-chunks-hnsw";
    private const string VectorSearchProfileName = "regulation-chunks-vector-profile";

    public static SearchIndex BuildIndex(string indexName, int vectorSearchDimensions)
    {
        return new SearchIndex(indexName)
        {
            Fields =
            {
                new SimpleField(KeyFieldName, SearchFieldDataType.String) { IsKey = true },
                new SearchableField(ContentFieldName, collection: false),
                new VectorSearchField(ContentVectorFieldName, vectorSearchDimensions, VectorSearchProfileName),
                new SimpleField(DocumentNameFieldName, SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                new SimpleField(PageStartFieldName, SearchFieldDataType.Int32) { IsFilterable = true },
                new SimpleField(PageEndFieldName, SearchFieldDataType.Int32) { IsFilterable = true },
                new SimpleField(ChunkIndexFieldName, SearchFieldDataType.Int32) { IsFilterable = true },
            },
            VectorSearch = new VectorSearch
            {
                Algorithms = { new HnswAlgorithmConfiguration(VectorSearchAlgorithmName) },
                Profiles = { new VectorSearchProfile(VectorSearchProfileName, VectorSearchAlgorithmName) },
            },
        };
    }
}
