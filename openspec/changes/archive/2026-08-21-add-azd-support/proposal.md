## Why

The ingestion pipeline depends on two Azure PaaS backends (Azure AI Foundry for embeddings, Azure AI Search for the vector index), but there is no repeatable way to provision or tear them down. Today they must be created by hand, which is slow, undocumented, and error-prone to reproduce across machines or after cleanup. Adding Azure Developer CLI (azd) support with Bicep infra lets anyone run `azd up` to stand up exactly what the app needs and `azd down` to remove it, with no manual portal work.

## What Changes

- Add `azure.yaml` at the repo root with no (or an empty) `services:` block — this is an infra-only azd project. Nothing is hosted in Azure; the ingestion app keeps running locally against the provisioned backends.
- Add `infra/main.bicep` (subscription-scope) provisioning:
  - An Azure AI Foundry / Azure OpenAI account (`Microsoft.CognitiveServices/accounts`) with a `text-embedding-3-small` model deployment
  - An Azure AI Search service on the Basic SKU
  - Standard azd resource-group creation and `azd-env-name` tagging
- Add RBAC role assignments in bicep, scoped to the developer's own signed-in principal (no managed identity — nothing runs in Azure):
  - `Cognitive Services OpenAI User` on the Foundry account
  - `Search Service Contributor` and `Search Index Data Contributor` on the Search service
- Add a `postprovision` hook in `azure.yaml` that runs `dotnet user-secrets set` against the existing `UserSecretsId` in `RegulationsSearcher.Ingestion.csproj`, populating `Foundry:Endpoint`, `Foundry:EmbeddingDeploymentName`, `AzureSearch:Endpoint`, and `AzureSearch:IndexName` so `dotnet run` works immediately after `azd up` with no manual config editing.
- Document that `azd down --purge` is required to fully remove the Foundry/Cognitive Services account (Azure soft-deletes these), so re-provisioning during iteration doesn't hit a name conflict.

Out of scope for this change: hosting the ingestion app in Azure (no Dockerfile, no Container Apps, no ACR), Blob Storage for source PDFs, Log Analytics/Application Insights, and any change to the Search index schema or ingestion pipeline logic.

## Capabilities

### New Capabilities
- `azure-infrastructure`: Provisioning and teardown of the Azure resources the ingestion pipeline depends on (Azure AI Foundry with an embedding deployment, Azure AI Search) via azd and Bicep, including the RBAC needed for a locally-run app authenticating with `DefaultAzureCredential`, and the local dev-secrets wiring that connects provisioned resources to the app without manual steps.

### Modified Capabilities
(none — no existing specs are affected)

## Impact

- New files: `azure.yaml`, `infra/main.bicep` (plus any supporting bicep modules/params), a postprovision hook script.
- No changes to application code or the ingestion pipeline logic.
- `src/RegulationsSearcher.Ingestion/RegulationsSearcher.Ingestion.csproj`'s existing `UserSecretsId` becomes the target of the postprovision hook — no csproj changes needed, just a new consumer of the existing mechanism.
- Developers gain new prerequisites: the Azure Developer CLI (`azd`) and an `az login`/`azd auth login` session with rights to create resources and role assignments in the target subscription.
