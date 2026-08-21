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
