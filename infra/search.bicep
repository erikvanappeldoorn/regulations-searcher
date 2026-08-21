@description('Name of the azd environment, used for resource naming and tagging')
param environmentName string

@description('Location for the Azure AI Search service')
param location string

@description('SKU for the Azure AI Search service')
param searchSku string = 'basic'

@description('Id of the principal to assign local-dev RBAC roles to')
param principalId string

var resourceToken = uniqueString(subscription().id, resourceGroup().id, environmentName)
var searchServiceName = 'srch-${resourceToken}'

resource searchService 'Microsoft.Search/searchServices@2025-05-01' = {
  name: searchServiceName
  location: location
  sku: {
    name: searchSku
  }
  properties: {
    replicaCount: 1
    partitionCount: 1
    hostingMode: 'default'
  }
  tags: {
    'azd-env-name': environmentName
  }
}

// Built-in role: Search Service Contributor
var searchServiceContributorRoleId = '7ca78c08-252a-4471-8644-bb5ff32d4ba0'

resource searchServiceContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(searchService.id, principalId, searchServiceContributorRoleId)
  scope: searchService
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', searchServiceContributorRoleId)
    principalId: principalId
  }
}

output endpoint string = 'https://${searchService.name}.search.windows.net'
