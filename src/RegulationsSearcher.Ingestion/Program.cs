using Microsoft.Extensions.Configuration;
using RegulationsSearcher.Ingestion.Clients;
using RegulationsSearcher.Ingestion.Configuration;
using RegulationsSearcher.Ingestion.Discovery;
using RegulationsSearcher.Ingestion.Extraction;
using RegulationsSearcher.Ingestion.Indexing;
using RegulationsSearcher.Ingestion.Validation;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .Build();

var foundryOptions = configuration.GetSection(FoundryOptions.SectionName).Get<FoundryOptions>()
    ?? throw new InvalidOperationException($"Missing '{FoundryOptions.SectionName}' configuration section.");
var searchOptions = configuration.GetSection(AzureSearchOptions.SectionName).Get<AzureSearchOptions>()
    ?? throw new InvalidOperationException($"Missing '{AzureSearchOptions.SectionName}' configuration section.");
var ingestionOptions = configuration.GetSection(IngestionOptions.SectionName).Get<IngestionOptions>()
    ?? throw new InvalidOperationException($"Missing '{IngestionOptions.SectionName}' configuration section.");

var sourceDocumentPaths = DocumentDiscovery.DiscoverPdfFiles(ingestionOptions.SourceDocumentsFolder);

Console.WriteLine($"Foundry endpoint: {foundryOptions.Endpoint}");
Console.WriteLine($"Azure AI Search endpoint: {searchOptions.Endpoint} (index: {searchOptions.IndexName})");
Console.WriteLine(sourceDocumentPaths.Count == 0
    ? $"No PDF files found in source documents folder: {ingestionOptions.SourceDocumentsFolder}"
    : $"Discovered {sourceDocumentPaths.Count} source document(s): {string.Join(", ", sourceDocumentPaths)}");

var searchIndexClient = AzureClientFactory.CreateSearchIndexClient(searchOptions);
var foundryClient = AzureClientFactory.CreateFoundryClient(foundryOptions);

Console.WriteLine($"Search client auth: {(string.IsNullOrEmpty(searchOptions.ApiKey) ? "DefaultAzureCredential" : "API key")}");
Console.WriteLine($"Foundry client auth: {(string.IsNullOrEmpty(foundryOptions.ApiKey) ? "DefaultAzureCredential" : "API key")}");

var embeddingDimensionValidator = new EmbeddingDimensionValidator(searchIndexClient);
await embeddingDimensionValidator.ValidateAsync(
    searchOptions.IndexName,
    SearchIndexSchema.ContentVectorFieldName,
    foundryOptions.EmbeddingDimension);
Console.WriteLine($"Validated embedding dimension ({foundryOptions.EmbeddingDimension}) against index '{searchOptions.IndexName}'.");

var pdfTextExtractor = new PdfTextExtractor();

foreach (var sourceDocumentPath in sourceDocumentPaths)
{
    var pages = pdfTextExtractor.ExtractPages(sourceDocumentPath);
    Console.WriteLine($"Extracted {pages.Count} page(s) from {Path.GetFileName(sourceDocumentPath)}");
}
