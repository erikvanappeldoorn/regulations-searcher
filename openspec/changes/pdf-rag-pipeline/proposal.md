## Why

Regulations Searcher needs a repeatable way to turn source regulation PDFs into searchable vector data before any retrieval or Q&A capability can be built. Today there is no ingestion path at all — this change establishes it as the foundation for the project, using the Microsoft stack (Azure AI Foundry-hosted models, Microsoft Agent Framework, Azure AI Search) the project has standardized on.

## What Changes

- Add a .NET console application that ingests a fixed set of two PDF regulation documents.
- Extract text from each PDF and split it into overlapping chunks sized for embedding and later retrieval.
- Generate vector embeddings for each chunk using an embedding model deployed on the user's own Azure AI Foundry resource.
- Create and populate an Azure AI Search index with the chunks, their vector embeddings, and source metadata (document name, page/section, chunk position).
- Orchestrate the ingestion steps (extract → chunk → embed → index) using the Microsoft Agent Framework in C#/.NET rather than hand-rolled control flow.
- Make the ingestion pipeline idempotent so re-running it against the same two PDFs updates/replaces existing index entries rather than duplicating them.

## Capabilities

### New Capabilities
- `document-ingestion`: PDF text extraction, chunking, embedding generation via an Azure AI Foundry-hosted model, and indexing of chunks + vectors into Azure AI Search, orchestrated via the Microsoft Agent Framework.

### Modified Capabilities
- None (greenfield change; no existing specs in this repo).

## Impact

- **New code**: a new .NET console project (ingestion pipeline), configuration for Azure AI Foundry and Azure AI Search connections.
- **Azure resources**: requires an Azure AI Foundry project with an embedding model deployment, and an Azure AI Search service/index (assumed to exist or be provisioned by the user; provisioning steps documented but not automated by this change).
- **Dependencies**: Microsoft Agent Framework (C#/.NET), a PDF text-extraction library, Azure.Search.Documents SDK, Azure AI Foundry/OpenAI SDK for embeddings.
- **Out of scope**: query/answer (retrieval + generation) capability, support for arbitrary/uploaded PDFs, and any UI or API surface — these are candidate follow-up changes.
