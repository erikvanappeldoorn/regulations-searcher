# azure-infrastructure

## Purpose

TBD

## Requirements

### Requirement: Provision ingestion dependencies via azd
The system SHALL provide an azd project (`azure.yaml` and `infra/*.bicep`) that, when run via `azd up`, provisions an Azure AI Foundry account with a `text-embedding-3-small` model deployment and an Azure AI Search service on the Basic SKU, inside a resource group tagged with the azd environment name.

#### Scenario: First-time provisioning
- **WHEN** a developer with no existing resources runs `azd up` and selects a subscription/region/environment name
- **THEN** azd creates a resource group tagged with the azd environment name, an Azure AI Foundry account with a `text-embedding-3-small` deployment, and an Azure AI Search service on the Basic SKU, and completes without requiring any manual portal steps

#### Scenario: Re-provisioning an existing environment
- **WHEN** a developer runs `azd up` again against an azd environment that was already provisioned
- **THEN** azd updates the deployment idempotently without creating duplicate resources

### Requirement: Tear down provisioned resources via azd
The system SHALL support full removal of the provisioned resources via `azd down --purge`, including permanent removal of the soft-deleted Azure AI Foundry / Cognitive Services account.

#### Scenario: Full teardown
- **WHEN** a developer runs `azd down --purge` against a provisioned azd environment
- **THEN** the resource group and all resources within it are deleted, and the Foundry/Cognitive Services account is purged rather than left in a soft-deleted state

#### Scenario: Re-provisioning after teardown
- **WHEN** a developer runs `azd up` again after a prior `azd down --purge` in the same environment
- **THEN** provisioning succeeds without a "name already exists (soft-deleted)" conflict on the Foundry/Cognitive Services account

### Requirement: RBAC scoped to the developer's principal
The system SHALL grant the deploying developer's signed-in principal exactly the roles needed to run the ingestion pipeline locally against the provisioned resources, without relying on API keys or a managed identity.

#### Scenario: Foundry access
- **WHEN** infrastructure provisioning completes
- **THEN** the deploying principal holds the `Cognitive Services OpenAI User` role on the provisioned Foundry account

#### Scenario: Search access
- **WHEN** infrastructure provisioning completes
- **THEN** the deploying principal holds both the `Search Service Contributor` role (for index management) and the `Search Index Data Contributor` role (for document upload) on the provisioned Search service

### Requirement: Local app config wired automatically after provisioning
The system SHALL populate the ingestion app's local user-secrets with the provisioned Foundry and Search endpoint values automatically after `azd up`, so the app can run locally without manual configuration edits.

#### Scenario: Postprovision hook populates user-secrets
- **WHEN** `azd up` completes provisioning
- **THEN** a postprovision hook sets `Foundry:Endpoint`, `Foundry:EmbeddingDeploymentName`, `AzureSearch:Endpoint`, and `AzureSearch:IndexName` in the ingestion project's user-secrets store, using the project's existing `UserSecretsId`

#### Scenario: Running the app immediately after provisioning
- **WHEN** a developer runs `dotnet run` in the ingestion project right after `azd up` completes, without editing any config file
- **THEN** the app resolves the Foundry and Search endpoints from user-secrets and authenticates successfully using `DefaultAzureCredential`
