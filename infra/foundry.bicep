@description('Name of the azd environment, used for resource naming and tagging')
param environmentName string

@description('Location for the Foundry / Azure OpenAI account')
param location string

var resourceToken = uniqueString(subscription().id, resourceGroup().id, environmentName)
var accountName = 'cog-${resourceToken}'

resource account 'Microsoft.CognitiveServices/accounts@2024-10-01' = {
  name: accountName
  location: location
  kind: 'OpenAI'
  sku: {
    name: 'S0'
  }
  properties: {
    customSubDomainName: accountName
    publicNetworkAccess: 'Enabled'
  }
  tags: {
    'azd-env-name': environmentName
  }
}

resource embeddingDeployment 'Microsoft.CognitiveServices/accounts/deployments@2024-10-01' = {
  parent: account
  name: 'text-embedding-3-small'
  sku: {
    name: 'Standard'
    capacity: 30
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'text-embedding-3-small'
      version: '1'
    }
  }
}

output endpoint string = account.properties.endpoint
