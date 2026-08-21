# regulations-searcher

## Infrastructure (azd)

The ingestion pipeline depends on two Azure PaaS backends — Azure AI Foundry (embeddings) and Azure AI Search (vector index). These are provisioned with the Azure Developer CLI (`azd`) and Bicep; the app itself always runs locally.

### Prerequisites

- [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd) (`azd`)
- An authenticated session with rights to create resources and role assignments in the target subscription: `azd auth login` or `az login`

### Provisioning

```sh
azd up
```

This creates a resource group, an Azure AI Foundry account with a `text-embedding-3-small` deployment, and an Azure AI Search service, then assigns the signed-in principal the RBAC roles the app needs. A `postprovision` hook writes the resulting endpoints into `RegulationsSearcher.Ingestion`'s `dotnet user-secrets`, so `dotnet run` works immediately afterward with no manual config edits.

> **RBAC propagation delay:** role assignments can take a few minutes to propagate after `azd up` finishes. If the first `dotnet run` fails with an authentication/authorization error, wait a minute or two and retry.

### Tearing down

```sh
azd down --purge
```

> **Soft-delete gotcha:** always use `--purge`, not plain `azd down`. Azure soft-deletes the Foundry/Cognitive Services account on removal, and a plain `azd down` leaves it in that soft-deleted state — a subsequent `azd up` in the same environment will then fail with a "name already exists (soft-deleted)" conflict. `--purge` removes it permanently.
