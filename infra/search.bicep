@description('Name of the azd environment, used for resource naming and tagging')
param environmentName string

@description('Location for the Azure AI Search service')
param location string

@description('SKU for the Azure AI Search service')
param searchSku string = 'basic'

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

output endpoint string = 'https://${searchService.name}.search.windows.net'
