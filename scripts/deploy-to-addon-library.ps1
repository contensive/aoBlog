#Requires -Version 5.1
<#
.SYNOPSIS
    Uploads a built collection to the Addon Collection Library on contensive.com.

.DESCRIPTION
    Thin wrapper that imports the shared deploy-to-addon-library module from
    Contensive5\scripts\ and calls Invoke-AddonLibraryDeploy with the
    parameters for this repo.

    This script is called by deploy-to-addon-library.cmd with repo-specific
    arguments already filled in.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$CollectionName,
    [Parameter(Mandatory)][string]$CollectionPath,
    [Parameter(Mandatory)][string]$DeploymentPath,
    [string]$UiPath = '',
    [string]$TargetDomain = 'www.contensive.com'
)

$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot '..\..\Contensive5\scripts\deploy-to-addon-library.psm1') -Force

Invoke-AddonLibraryDeploy `
    -CollectionName $CollectionName `
    -CollectionPath $CollectionPath `
    -DeploymentPath $DeploymentPath `
    -UiPath         $UiPath `
    -TargetDomain   $TargetDomain
