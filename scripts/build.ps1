#Requires -Version 5.1
<#
.SYNOPSIS
    aoBlog collection build — configuration only.
    All build steps are defined in the shared Contensive build library.
    Entry point: build.cmd
.PARAMETER Configuration
    Build configuration (Debug or Release). Defaults to Debug.
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot '..\..\Contensive5\scripts\contensive-build.psm1') -Force

$projectRoot = (Resolve-Path "$PSScriptRoot\..").Path

# -- copy helpfiles into ui/helpFiles before the shared build zips them
$helpSrc = Join-Path $projectRoot 'helpfiles'
$helpDst = Join-Path $projectRoot 'ui\helpFiles'
if (-not (Test-Path $helpDst)) { New-Item -ItemType Directory -Path $helpDst | Out-Null }
Copy-Item (Join-Path $helpSrc '*') -Destination $helpDst -Force

Invoke-ContensiveBuild `
    -CollectionName    'Blog' `
    -CollectionPath    "$projectRoot\collections\Blog" `
    -SolutionPath      "$projectRoot\source\aoBlogs2.sln" `
    -BinPath           "$projectRoot\source\aoBlogs2\bin\$Configuration\net472" `
    -DeploymentRoot    'C:\deployments\aoBlog' `
    -Configuration     $Configuration `
    -CleanFolders      @(
                           "$projectRoot\source\aoBlogs2\bin"
                           "$projectRoot\source\aoBlogs2\obj"
                       ) `
    -UiPath            "$projectRoot\ui"
