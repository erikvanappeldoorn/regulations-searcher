#!/usr/bin/env bash
set -euo pipefail

eval "$(azd env get-values)"

PROJECT="src/RegulationsSearcher.Ingestion"

dotnet user-secrets set "Foundry:Endpoint" "$FOUNDRY_ENDPOINT" --project "$PROJECT"
dotnet user-secrets set "Foundry:EmbeddingDeploymentName" "text-embedding-3-small" --project "$PROJECT"
dotnet user-secrets set "AzureSearch:Endpoint" "$SEARCH_ENDPOINT" --project "$PROJECT"
dotnet user-secrets set "AzureSearch:IndexName" "regulation-chunks" --project "$PROJECT"
