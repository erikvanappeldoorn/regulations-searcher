## Context

Regulations Searcher is a greenfield .NET project. There is no existing ingestion, chunking, embedding, or search code in this repo yet. This change builds the first vertical slice: turning all regulation PDFs found in a configured documents folder into a queryable vector index, using only Microsoft-stack components (Azure AI Foundry for hosted models, Microsoft Agent Framework for orchestration, Azure AI Search for the vector store), all reachable from a .NET console app run on-demand by the developer.

Constraints:
- The user's own Azure subscription hosts the Foundry project and Azure AI Search service; this change assumes those resources exist (or documents the minimum required deployment) but does not automate Azure resource provisioning (e.g., via Bicep/Terraform).
- The set of PDFs is discovered by scanning a configured documents folder at each run; the count is not fixed, but there is no folder-watching or streaming ingestion — each run does a one-time scan.
- No query/answer capability is in scope — the deliverable is a populated, correct vector index.

## Goals / Non-Goals

**Goals:**
- Deterministically turn each PDF found in the configured documents folder into text chunks suitable for embedding and later semantic retrieval.
- Generate embeddings for every chunk using a model deployed on the user's Azure AI Foundry project.
- Store chunks + vectors + metadata in an Azure AI Search index with a vector field configured for similarity search.
- Orchestrate extract → chunk → embed → index as a Microsoft Agent Framework workflow, not ad hoc procedural code, so steps are observable and independently retryable.
- Re-running the pipeline against the same set of PDFs updates the index in place (no duplicate chunks).

**Non-Goals:**
- Query/answer, retrieval-augmented generation, or any chat experience.
- Support for arbitrary PDF uploads (outside the configured folder), live folder watching, or a document management UI.
- Automated provisioning of Azure resources (Foundry project, model deployment, Search service) — configuration only assumes they exist.
- OCR for scanned/image-only PDFs (assume the source PDFs have extractable text).

## Decisions

### Document discovery: scan a configured folder for PDFs
Rather than configuring individual PDF file paths, the pipeline discovers all `*.pdf` files in a single configured documents folder (e.g. `Docs/`) at the start of each run. This makes the set of ingested documents flexible — adding, removing, or renaming a PDF in the folder changes what gets ingested on the next run, with no code or config changes required. Each discovered file is still processed independently through the same extract → chunk → embed → index steps; there is no assumption about how many PDFs exist.
- Alternative considered: keep an explicit list of file paths in config — rejected because it requires editing config for every document addition/removal, which is exactly the flexibility this decision is meant to provide.

### PDF text extraction: PdfPig
Use the `PdfPig` library (pure managed .NET, no native dependencies; NuGet package ID `PdfPig`, not the stale/squatted `UglyToad.PdfPig` ID) to extract text per page. Alternative considered: iText7 (AGPL/commercial licensing friction) and Docnet.Core (native dependency, heavier for a two-document use case). PdfPig's page-by-page extraction also gives us page numbers for chunk metadata "for free."

### Chunking strategy: token-aware sliding window with overlap
Concatenate page text, then split into chunks of ~800 tokens with ~100-token overlap, using a tokenizer (`Microsoft.ML.Tokenizers`, cl100k-compatible) so chunk size is measured in the same units the embedding model bills/limits on — not characters. Overlap preserves context across chunk boundaries for regulation text where a clause can span a boundary.
- Alternative considered: fixed character-count chunking — rejected because it under- or over-fills the model's effective context depending on token density.
- Alternative considered: structure-aware chunking (split on article/section headers) — desirable for regulation text, but the source PDFs' heading structure must be inspected before assuming a reliable pattern. Recorded as an open question below rather than gold-plating the pipeline now.
- Each chunk carries metadata: source document name, page range, chunk index, and a stable `chunkId` derived from `{documentName}-{chunkIndex}`.

### Embedding generation: Microsoft.Extensions.AI `IEmbeddingGenerator` over an Azure AI Foundry deployment
Use the `Microsoft.Extensions.AI` abstraction (the same abstraction the Microsoft Agent Framework builds on) backed by the Azure AI Foundry/Azure OpenAI connector, pointed at an embedding model (e.g. `text-embedding-3-small` or `text-embedding-3-large`) deployed in the user's Foundry project. Using the `Microsoft.Extensions.AI` interface (rather than calling the Azure SDK directly) keeps the embedding step swappable and consistent with how the Agent Framework composes model clients elsewhere in the pipeline.

### Orchestration: Microsoft Agent Framework workflow
Model the pipeline as an Agent Framework workflow with explicit steps/edges: `LoadPdf → ExtractText → ChunkText → GenerateEmbeddings → UpsertToIndex`, one workflow run per document. This gives step-level logging and the ability to retry a failed step (e.g., a transient embedding-call failure) without re-running PDF extraction. Alternative considered: a plain top-to-bottom console script — simpler to write, but the user explicitly asked for the Agent Framework, and step boundaries also make unit testing each stage easier.

### Vector store: Azure AI Search index with a vector field
Create one Azure AI Search index (e.g. `regulation-chunks`) with fields: `id` (key, = chunkId), `content` (searchable string), `contentVector` (`Collection(Edm.Single)`, dimensions matching the embedding model, HNSW vector search profile), `documentName`, `pageStart`, `pageEnd`, `chunkIndex`. The index is created idempotently at pipeline start if it doesn't exist (schema defined in code, not the portal), so the whole pipeline is runnable from a clean subscription.

### Idempotent re-runs via deterministic IDs + mergeOrUpload
Each chunk's `id` is deterministically derived from document name + chunk index. Indexing uses Azure AI Search's `mergeOrUpload` action, so re-running the pipeline on an unchanged document overwrites the same documents instead of creating duplicates. If a document's chunk count shrinks between runs (e.g., after a chunking parameter change), stale trailing chunk IDs from the previous run are deleted by comparing against a per-document chunk-count record written alongside the data.

### Auth: Azure Identity (`DefaultAzureCredential`) by default, API key as local fallback
Prefer Entra ID auth (`DefaultAzureCredential`) for both the Foundry endpoint and Azure AI Search, matching Microsoft's recommended posture and avoiding long-lived keys in config. Fall back to API-key-based config (via .NET user-secrets, never committed) if the user's Foundry/Search resources aren't set up for RBAC yet — documented as a config option, not the default.

## Risks / Trade-offs

- **[Risk]** Chunk token size doesn't match the deployed embedding model's practical limits or the (not-yet-built) retrieval consumer's expectations → **Mitigation**: token count and overlap are configuration values, not hardcoded, so they can be tuned without code changes once a retrieval capability is designed.
- **[Risk]** Embedding vector dimensions must exactly match the Azure AI Search vector field's configured dimensions, and this is only discovered at index-create or upload time → **Mitigation**: read the embedding model's dimension count from config, validate it against the index schema before running the workflow, and fail fast with a clear error rather than a cryptic Search SDK error.
- **[Risk]** Regulation PDFs often have multi-column layouts or tables that PdfPig's simple text extraction may reorder or mangle → **Mitigation**: manually spot-check extracted text for each document discovered in the documents folder during implementation; if extraction quality is unacceptable for any of them, revisit extraction approach before building further on top of it.
- **[Risk]** Rate limiting or transient failures from the Foundry embedding endpoint mid-run → **Mitigation**: retry with exponential backoff at the `GenerateEmbeddings` workflow step; because chunk IDs are deterministic, a partial re-run is safe.
- **[Trade-off]** Using Azure AI Search for a small documents folder is heavier infrastructure than the data volume strictly requires → accepted per explicit decision to keep parity with the eventual production shape and avoid a throwaway in-memory store that would need replacing soon.

## Migration Plan

Net-new capability — no existing data or consumers to migrate. First run against a freshly created (or empty) Azure AI Search index. If the index schema needs to change later, the design supports recreating the index under a new name and re-running the pipeline, since all state is derived from the source PDFs.

## Open Questions

- Do the regulation PDFs in the documents folder have a heading/section structure regular enough to justify structure-aware chunking instead of pure token-window chunking? (Needs inspection of the actual files during implementation.)
- Which specific embedding model deployment (name, dimensions) and Azure AI Search service/tier will be used? Needed to pin exact config values and validate vector-field dimensions.
- Should the per-document "expected chunk count" record (used to prune stale chunks on re-run) live inside the Azure AI Search index itself (e.g., a manifest document) or in local pipeline state? Leaning toward a manifest document in the same index to avoid a second storage dependency, but not yet finalized.
