## Context

`RegulationsSearcher.Ingestion` is a locally-run .NET 10 console app that reads PDFs from a local `Docs/` folder and writes chunks + embeddings to two Azure PaaS backends:

- **Azure AI Foundry / Azure OpenAI** — embedding generation (`Foundry:*` config, `AzureClientFactory.CreateFoundryClient`)
- **Azure AI Search** — vector index storage (`AzureSearch:*` config, `AzureClientFactory.CreateSearchIndexClient` / `CreateSearchClient`)

`AzureClientFactory` already authenticates via `DefaultAzureCredential`, with an API-key fallback intended only for local user-secrets use, not for this flow. The Search index schema is created and maintained by the app itself at runtime (`Indexing/SearchIndexProvisioner.cs`), not by infrastructure code. There is currently no `azure.yaml`, no `infra/` folder, and no scripted way to provision or remove these resources — everything so far has been created manually.

## Goals / Non-Goals

**Goals:**
- `azd up` provisions an Azure AI Foundry account with a `text-embedding-3-small` deployment, and an Azure AI Search service (Basic SKU), in a fresh resource group.
- `azd down` (with `--purge`) removes everything cleanly, including the soft-deleted Foundry/Cognitive Services account.
- The developer's own signed-in principal gets exactly the RBAC roles needed to run the ingestion app locally against the provisioned resources — no API keys involved in this flow.
- After `azd up`, `dotnet run` works immediately with no manual config edits: a postprovision hook pushes the provisioned endpoints into .NET user-secrets.

**Non-Goals:**
- Hosting the ingestion app in Azure (no Dockerfile, no Container Apps Job/Environment, no ACR). It stays a locally-run console exe.
- Provisioning Blob Storage for source PDFs — they remain on local disk in `Docs/`.
- Provisioning Log Analytics or Application Insights — the recently added step-level pipeline logging stays console/local-only.
- Creating or managing the Search index schema via bicep — that stays owned by `SearchIndexProvisioner` at runtime.
- Pinning a default Azure region or subscription — left as azd's standard interactive prompts.

## Decisions

**Infra-only azd project, not infra+hosted job.**
Considered containerizing the app into an Azure Container Apps Job so `azd up` could also run ingestion in Azure. Rejected for this change: the app reads PDFs from local disk today, so hosting it would first require adding Blob Storage and a document-upload path — a separate concern from "let me provision my dependencies." Keeping this change infra-only means no Dockerfile, no ACR, no Container Apps Environment, and a much smaller `azure.yaml` (no `services:` block). Hosting can be a future change once/if a query-facing service exists to host alongside it.

**Search SKU: Basic over Free.**
Free tier caps at 3 indexes / 50MB storage and has no semantic ranker. Basic costs a small monthly amount but avoids hitting those limits as the document set grows beyond the current single F1-regs PDF. This is set as a bicep parameter so it can be changed per-environment later if needed.

**RBAC assigned to the developer's principal, not a managed identity.**
Because nothing runs in Azure, there is no service identity to assign roles to. Instead, bicep takes a `principalId` parameter (azd's standard pattern, sourced from `az ad signed-in-user show` / the azd environment) and assigns:
- `Cognitive Services OpenAI User` on the Foundry account (call embeddings)
- `Search Service Contributor` on the Search service (control-plane index create/update, needed by `SearchIndexProvisioner`)
- `Search Index Data Contributor` on the Search service (data-plane document upload, needed by `ChunkUploader`)

Two roles are needed on Search (not one) because the app does both control-plane (index management) and data-plane (document upload) operations.

**Local config wiring: postprovision hook → `dotnet user-secrets`, not env vars.**
Considered writing a `.env` file and relying on the `Microsoft.Extensions.Configuration.EnvironmentVariables` provider already wired into the app. Rejected in favor of user-secrets: the `.csproj` already declares a `UserSecretsId`, meaning user-secrets is the established local-secret mechanism for this project. A `postprovision` hook running `dotnet user-secrets set` means `dotnet run` works right after `azd up` with nothing to export or source in each new shell. The hook sets `Foundry:Endpoint`, `Foundry:EmbeddingDeploymentName`, `AzureSearch:Endpoint`, and `AzureSearch:IndexName`, reading bicep outputs via `azd env get-values`.

**Region/subscription left as azd's standard prompts.**
No default region is pinned in this change — embedding-model regional availability and quota vary, and there's no fixed subscription/region preference yet. Standard azd `location` parameter and first-run prompts apply.

## Risks / Trade-offs

- **[Risk]** Azure OpenAI / Foundry accounts soft-delete on removal, so repeated `azd up` / `azd down` cycles during iteration can hit "name already exists (soft-deleted)" errors → **Mitigation:** document that `azd down --purge` must be used, and call this out in tasks/README so it's not a surprise mid-iteration.
- **[Risk]** Embedding model deployment quota/availability differs by region; a developer could pick a region where `text-embedding-3-small` isn't available or quota is exhausted → **Mitigation:** this is inherent to leaving region unpinned; document known-good regions in the task notes as a hint, without hardcoding one.
- **[Risk]** RBAC role propagation in Azure AD can take a few minutes after `azd up` completes, causing the first `dotnet run` to fail with an auth error even though provisioning succeeded → **Mitigation:** note this in the README/tasks as an expected transient failure mode; retrying after a short wait resolves it.
- **[Trade-off]** Because the index schema stays app-managed rather than defined in bicep, `azd up` alone does not leave a fully "ready to query" index — the ingestion app must still be run once to create it. This matches current behavior and keeps infra and schema concerns separated, but means "provisioned" and "usable" aren't quite the same moment.

## Migration Plan

No existing infra to migrate from — this is additive. Rollout is: merge `azure.yaml` + `infra/`, developers run `azd up` once against their own subscription to validate, then rely on it going forward instead of manual resource creation. No rollback concerns beyond `azd down --purge` to clean up a bad provisioning attempt.

## Open Questions

None blocking — all scope, SKU, region, observability, and local-wiring decisions were resolved during exploration prior to this proposal.
