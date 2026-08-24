using Microsoft.Extensions.Configuration;
using RegulationsSearcher.Ingestion.Chunking;
using RegulationsSearcher.Ingestion.Clients;
using RegulationsSearcher.Ingestion.Configuration;
using RegulationsSearcher.Ingestion.Discovery;
using RegulationsSearcher.Ingestion.Embeddings;
using RegulationsSearcher.Ingestion.Extraction;
using RegulationsSearcher.Ingestion.Indexing;
using RegulationsSearcher.Ingestion.Orchestration;
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

var searchIndexProvisioner = new SearchIndexProvisioner(searchIndexClient);
await searchIndexProvisioner.EnsureIndexExistsAsync(searchOptions.IndexName, foundryOptions.EmbeddingDimension);
Console.WriteLine($"Ensured Azure AI Search index '{searchOptions.IndexName}' exists.");

var embeddingDimensionValidator = new EmbeddingDimensionValidator(searchIndexClient);
await embeddingDimensionValidator.ValidateAsync(
    searchOptions.IndexName,
    SearchIndexSchema.ContentVectorFieldName,
    foundryOptions.EmbeddingDimension);
Console.WriteLine($"Validated embedding dimension ({foundryOptions.EmbeddingDimension}) against index '{searchOptions.IndexName}'.");

var pdfTextExtractor = new PdfTextExtractor();
var textChunker = new TextChunker(ingestionOptions.ChunkSizeInTokens, ingestionOptions.OverlapSizeInTokens);
var embeddingGenerator = AzureClientFactory.CreateEmbeddingGenerator(foundryClient, foundryOptions);
var chunkEmbedder = new ChunkEmbedder(embeddingGenerator);
var searchClient = AzureClientFactory.CreateSearchClient(searchOptions);
var chunkUploader = new ChunkUploader(searchClient);
var pipelineLogger = new ConsolePipelineLogger();

var workflowFactory = new PdfIngestionWorkflowFactory(pdfTextExtractor, textChunker, chunkEmbedder, chunkUploader, pipelineLogger);
var workflow = workflowFactory.Build();
var ingestionRunner = new PdfIngestionRunner(workflow, pipelineLogger);

var failedDocuments = await ingestionRunner.RunAsync(sourceDocumentPaths);

if (failedDocuments.Count > 0)
{
    Console.WriteLine($"Completed with {failedDocuments.Count} of {sourceDocumentPaths.Count} document(s) failing: {string.Join(", ", failedDocuments)}");
    Environment.ExitCode = 1;
}
else
{
    Console.WriteLine($"Completed ingestion of {sourceDocumentPaths.Count} document(s).");
}
