## 1. azd project scaffolding

- [x] 1.1 Add `azure.yaml` at the repo root: project name, no (or empty) `services:` block, wired to `infra/` as the infrastructure path
- [x] 1.2 Add `.gitignore` entries for azd-generated local state (`.azure/`) if not already covered

## 2. Bicep: resource group and parameters

- [x] 2.1 Add `infra/main.bicep` at subscription scope, taking standard azd parameters (`environmentName`, `location`, `principalId`)
- [x] 2.2 Create the resource group, tagged with `azd-env-name`
- [x] 2.3 Add `infra/main.parameters.json` wiring azd environment values (`AZURE_ENV_NAME`, `AZURE_LOCATION`, `AZURE_PRINCIPAL_ID`) to the bicep parameters

## 3. Bicep: Azure AI Foundry / OpenAI account

- [x] 3.1 Add a module provisioning a `Microsoft.CognitiveServices/accounts` resource (Foundry/OpenAI kind) with a unique, azd-conventional name
- [x] 3.2 Add a `text-embedding-3-small` model deployment on the Foundry account, matching `FoundryOptions.EmbeddingDeploymentName`'s current default
- [x] 3.3 Output the Foundry account endpoint from the module for use by the postprovision hook

## 4. Bicep: Azure AI Search

- [x] 4.1 Add a module provisioning a `Microsoft.Search/searchServices` resource on the Basic SKU, with a unique, azd-conventional name
- [x] 4.2 Output the Search service endpoint from the module for use by the postprovision hook
- [x] 4.3 Confirm no index/schema resources are defined here — index creation stays owned by `SearchIndexProvisioner` at runtime

## 5. Bicep: RBAC

- [x] 5.1 Assign the `Cognitive Services OpenAI User` role on the Foundry account to the `principalId` parameter
- [x] 5.2 Assign the `Search Service Contributor` role on the Search service to the `principalId` parameter
- [ ] 5.3 Assign the `Search Index Data Contributor` role on the Search service to the `principalId` parameter

## 6. Local dev wiring

- [ ] 6.1 Add a postprovision hook script (referenced from `azure.yaml`) that reads `azd env get-values` and runs `dotnet user-secrets set` for `Foundry:Endpoint`, `Foundry:EmbeddingDeploymentName`, `AzureSearch:Endpoint`, `AzureSearch:IndexName` against `RegulationsSearcher.Ingestion`'s existing `UserSecretsId`
- [ ] 6.2 Verify the hook is idempotent (safe to run on every `azd up`, not just the first)

## 7. Validation

- [ ] 7.1 Run `azd up` end-to-end against a real subscription; confirm resource group, Foundry deployment, and Search service are created and the postprovision hook populates user-secrets correctly
- [ ] 7.2 Run `dotnet run` immediately after `azd up` with no manual config edits; confirm the ingestion pipeline authenticates and runs successfully against the provisioned resources
- [ ] 7.3 Run `azd down --purge`; confirm the resource group and the Foundry/Cognitive Services account (including its soft-deleted state) are fully removed
- [ ] 7.4 Run `azd up` again in the same environment after teardown; confirm no "name already exists (soft-deleted)" conflict occurs

## 8. Documentation

- [ ] 8.1 Document the `azd up` / `azd down --purge` workflow (prerequisites: `azd` CLI, `azd auth login`/`az login`) in a README section
- [ ] 8.2 Note the soft-delete/purge gotcha and the expected transient RBAC-propagation delay after first provisioning
