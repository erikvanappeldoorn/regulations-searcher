## 1. Project Setup

- [x] 1.1 Create a new .NET console project (e.g. `src/RegulationsSearcher.Ingestion`) targeting a current LTS .NET SDK
- [x] 1.2 Add package references: `PdfPig`, `Microsoft.ML.Tokenizers`, `Azure.Search.Documents`, `Microsoft.Extensions.AI` (+ Azure AI Foundry/OpenAI connector), Microsoft Agent Framework packages, `Azure.Identity`
- [x] 1.3 Add configuration (`appsettings.json` + user-secrets) for: Foundry endpoint, embedding deployment name, embedding dimension, Azure AI Search endpoint, index name, and the source documents folder path (all PDFs in this folder are discovered and ingested; count is not fixed)
- [x] 1.4 Wire up `DefaultAzureCredential`-based auth for both the Foundry and Search clients, with an API-key fallback read from user-secrets

## 2. PDF Text Extraction

- [x] 2.1 Implement a `PdfTextExtractor` that opens a PDF via PdfPig and returns page-ordered text with page numbers
- [x] 2.2 Handle and surface a clear error when a configured PDF path is missing or fails to parse
- [x] 2.3 Run extraction against all discovered source PDFs and manually spot-check the extracted text for layout/ordering issues (tables, multi-column sections)

## 3. Text Chunking

- [x] 3.1 Implement a token-aware chunker using `Microsoft.ML.Tokenizers`, with configurable target chunk size and overlap size
- [x] 3.2 Attach metadata to each chunk: source document name, page range, and zero-based chunk index
- [x] 3.3 Unit test chunk boundaries and overlap behavior against sample text, including edge cases (text shorter than one chunk, exact multiples of chunk size)

## 4. Embedding Generation

- [x] 4.1 Implement an embedding client using `Microsoft.Extensions.AI`'s `IEmbeddingGenerator`, configured against the Azure AI Foundry embedding deployment
- [x] 4.2 Add retry-with-backoff around embedding calls for transient failures (throttling, timeouts)
- [ ] 4.3 Add a startup validation step that compares the configured embedding dimension against the Azure AI Search vector field's dimension and fails fast on mismatch

## 5. Azure AI Search Index Management

- [ ] 5.1 Define the target index schema in code: key field (`id`), searchable `content` field, `contentVector` field (HNSW vector search profile), `documentName`, `pageStart`, `pageEnd`, `chunkIndex`
- [ ] 5.2 Implement index provisioning that creates the index if it doesn't exist and is a no-op if a compatible index already exists
- [ ] 5.3 Implement deterministic chunk ID generation (`{documentName}-{chunkIndex}`) used for both indexing and stale-chunk cleanup

## 6. Chunk Indexing and Idempotency

- [ ] 6.1 Implement chunk upload using `mergeOrUpload` so re-running ingestion overwrites existing chunk documents instead of duplicating them
- [ ] 6.2 Implement stale-chunk cleanup: track expected chunk count per document and delete any previously-indexed chunk IDs beyond the current count
- [ ] 6.3 Verify idempotency manually: run ingestion twice against the same PDFs and confirm the index document count is unchanged after the second run

## 7. Pipeline Orchestration (Microsoft Agent Framework)

- [ ] 7.1 Define an Agent Framework workflow with steps `LoadPdf → ExtractText → ChunkText → GenerateEmbeddings → UpsertToIndex`, run once per document
- [ ] 7.2 Add step-level logging so each step's success/failure is individually visible in console output
- [ ] 7.3 Ensure a failure in a later step (e.g., `GenerateEmbeddings`) surfaces which document and step failed without silently discarding earlier step results
- [ ] 7.4 Wire the console app's `Main` entry point to run the workflow for every PDF discovered in the documents folder, sequentially

## 8. End-to-End Verification

- [ ] 8.1 Run the full pipeline against all real PDFs in the documents folder end-to-end and confirm the Azure AI Search index is populated with the expected chunk documents
- [ ] 8.2 Spot-check a handful of indexed chunks in the Azure portal (or via a search query) for correct content, metadata, and vector presence
- [ ] 8.3 Document how to configure and run the pipeline (required settings, how to provision the Azure AI Search index/Foundry deployment) in a README for the new project
