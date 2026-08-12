using Azure;
using Azure.Search.Documents.Indexes;

namespace RegulationsSearcher.Ingestion.Validation;

public sealed class EmbeddingDimensionValidator
{
    private readonly SearchIndexClient _searchIndexClient;

    public EmbeddingDimensionValidator(SearchIndexClient searchIndexClient)
    {
        _searchIndexClient = searchIndexClient;
    }

    public async Task ValidateAsync(
        string indexName,
        string vectorFieldName,
        int configuredDimension,
        CancellationToken cancellationToken = default)
    {
        Response<Azure.Search.Documents.Indexes.Models.SearchIndex> response;
        try
        {
            response = await _searchIndexClient.GetIndexAsync(indexName, cancellationToken);
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            return;
        }

        var vectorField = response.Value.Fields.FirstOrDefault(field => field.Name == vectorFieldName)
            ?? throw new InvalidOperationException(
                $"Index '{indexName}' does not contain a field named '{vectorFieldName}'.");

        var indexDimension = vectorField.VectorSearchDimensions
            ?? throw new InvalidOperationException(
                $"Field '{vectorFieldName}' on index '{indexName}' is not configured as a vector field.");

        if (indexDimension != configuredDimension)
        {
            throw new InvalidOperationException(
                $"Embedding dimension mismatch: configured embedding dimension is {configuredDimension}, " +
                $"but index '{indexName}' field '{vectorFieldName}' is configured for {indexDimension}.");
        }
    }
}
