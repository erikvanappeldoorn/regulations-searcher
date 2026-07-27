using Microsoft.Extensions.Configuration;
using RegulationsSearcher.Ingestion.Clients;
using RegulationsSearcher.Ingestion.Configuration;

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

Console.WriteLine($"Foundry endpoint: {foundryOptions.Endpoint}");
Console.WriteLine($"Azure AI Search endpoint: {searchOptions.Endpoint} (index: {searchOptions.IndexName})");
Console.WriteLine($"Source documents: {string.Join(", ", ingestionOptions.SourceDocumentPaths)}");

var searchIndexClient = AzureClientFactory.CreateSearchIndexClient(searchOptions);
var foundryClient = AzureClientFactory.CreateFoundryClient(foundryOptions);

Console.WriteLine($"Search client auth: {(string.IsNullOrEmpty(searchOptions.ApiKey) ? "DefaultAzureCredential" : "API key")}");
Console.WriteLine($"Foundry client auth: {(string.IsNullOrEmpty(foundryOptions.ApiKey) ? "DefaultAzureCredential" : "API key")}");
