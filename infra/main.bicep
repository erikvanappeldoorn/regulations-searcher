targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the azd environment that can be used as part of naming resource convention')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

@description('Id of the developer principal to assign local-dev RBAC roles to')
param principalId string

var tags = {
  'azd-env-name': environmentName
}

resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}

module foundry 'foundry.bicep' = {
  name: 'foundry'
  scope: rg
  params: {
    environmentName: environmentName
    location: location
  }
}

output FOUNDRY_ENDPOINT string = foundry.outputs.endpoint
