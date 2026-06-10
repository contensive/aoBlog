#Requires -Version 5.1
[CmdletBinding()]
param(
    [string]   $LocalDeployTarget  = '',
    [hashtable]$RemoteDeployTarget = $null
)

$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot '..\..\Contensive5\scripts\build-addon-collection.psm1') -Force

$projectRoot = (Resolve-Path "$PSScriptRoot\..").Path

Invoke-ContensiveBuild `
    -CollectionName    'Blog' `
    -CollectionPath    "$projectRoot\collections\Blog" `
    -SolutionPath      "$projectRoot\source\aoBlogs2.sln" `
    -BinPath           "$projectRoot\source\aoBlogs2\bin\Release\netstandard2.0" `
    -DeploymentRoot    'C:\Deployments\aoBlog' `
    -CleanFolders      @(
                           "$projectRoot\source\aoBlogs2\bin"
                           "$projectRoot\source\aoBlogs2\obj"
                       ) `
    -UiPath            "$projectRoot\ui" `
    -LocalDeployTarget  $LocalDeployTarget `
    -RemoteDeployTarget $RemoteDeployTarget
