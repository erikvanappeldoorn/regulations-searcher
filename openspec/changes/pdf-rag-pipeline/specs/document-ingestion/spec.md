## ADDED Requirements

### Requirement: Source Document Discovery
The system SHALL discover all PDF documents to ingest by scanning a configured documents folder, rather than relying on a fixed or hardcoded list of file paths.

#### Scenario: Folder contains multiple PDFs
- **WHEN** the pipeline runs and the configured documents folder contains one or more PDF files
- **THEN** the system ingests every PDF file found in that folder, regardless of count

#### Scenario: Folder contains no PDFs
- **WHEN** the pipeline runs and the configured documents folder contains no PDF files
- **THEN** the system completes without error, indexes no chunks, and reports that no source documents were found

### Requirement: PDF Text Extraction
The system SHALL extract text content, per page, from each PDF document discovered in the configured source documents folder.

#### Scenario: Text extracted from a readable PDF
- **WHEN** the pipeline processes a configured PDF that contains extractable text
- **THEN** the system produces the page-ordered text content of that document along with each page's page number

#### Scenario: Extraction fails for an unreadable PDF
- **WHEN** the pipeline processes a discovered PDF file that cannot be parsed as a PDF
- **THEN** the system reports a clear error identifying the failing document and does not proceed to chunk or index that document

### Requirement: Token-Aware Text Chunking
The system SHALL split each document's extracted text into overlapping chunks sized by token count, using configurable target chunk size and overlap size.

#### Scenario: Document text is split into overlapping chunks
- **WHEN** a document's extracted text is chunked with a configured target size of N tokens and overlap of M tokens
- **THEN** the system produces a sequence of chunks where each chunk (except possibly the last) contains approximately N tokens, and consecutive chunks share approximately M tokens of overlapping content

#### Scenario: Each chunk retains traceable metadata
- **WHEN** a chunk is produced from a document
- **THEN** the chunk carries the source document name, the page range it was drawn from, and a zero-based chunk index within that document

### Requirement: Chunk Embedding Generation
The system SHALL generate a vector embedding for every chunk using an embedding model deployed on the user's Azure AI Foundry project.

#### Scenario: Embedding generated for a chunk
- **WHEN** a text chunk is submitted to the configured Foundry-hosted embedding model
- **THEN** the system receives a numeric vector for that chunk whose dimension count matches the configured embedding model's output dimensions

#### Scenario: Embedding call fails transiently
- **WHEN** a call to the embedding model fails with a transient error (e.g., throttling or a timeout)
- **THEN** the system retries the call with backoff before giving up, and only fails the pipeline run for that document after retries are exhausted

### Requirement: Embedding Dimension Validation
The system SHALL validate that the configured embedding model's output dimension matches the Azure AI Search index's vector field dimension before any chunks are indexed.

#### Scenario: Dimension mismatch is caught before indexing
- **WHEN** the pipeline starts and the configured embedding dimension does not match the target Azure AI Search vector field's configured dimension
- **THEN** the system fails fast with an error identifying the mismatch and does not attempt to upload any chunks

### Requirement: Azure AI Search Index Provisioning
The system SHALL ensure the target Azure AI Search index exists, creating it with the required schema (key field, searchable content field, vector field, and document metadata fields) if it does not already exist.

#### Scenario: Index does not yet exist
- **WHEN** the pipeline runs and the configured Azure AI Search index is not present in the Search service
- **THEN** the system creates the index with the required fields before attempting to upload any chunks

#### Scenario: Index already exists
- **WHEN** the pipeline runs and the configured index already exists with a compatible schema
- **THEN** the system does not attempt to recreate or drop the index, and proceeds directly to indexing chunks

### Requirement: Idempotent Chunk Indexing
The system SHALL upsert each chunk into the Azure AI Search index using a deterministic document ID derived from the source document name and chunk index, so that re-running ingestion on an unchanged document does not create duplicate entries.

#### Scenario: Re-running ingestion on the same document
- **WHEN** the pipeline ingests a document that was already fully indexed in a previous run, with no change to chunking configuration
- **THEN** the resulting Azure AI Search index contains the same set of chunk documents as before, with existing entries overwritten in place rather than duplicated

#### Scenario: Chunk count shrinks after a re-run
- **WHEN** a document produces fewer chunks on a re-run than it did previously (e.g., after a chunking configuration change)
- **THEN** the system removes the now-stale chunk documents for that document from the index so no orphaned chunks remain

### Requirement: Pipeline Orchestration via Microsoft Agent Framework
The system SHALL orchestrate the extract, chunk, embed, and index steps for each document as a Microsoft Agent Framework workflow, with each step individually identifiable in logs and independently retryable on failure.

#### Scenario: A single step fails
- **WHEN** the embedding-generation step fails for a document after exhausting retries
- **THEN** the workflow reports which step and which document failed, and the extraction and chunking results already produced for that run are not silently discarded before the error is surfaced
