#!/usr/bin/env pwsh
# Regenerate Kiota client from an OpenAPI document.
# Run from csharp_location_sdk repo root (or clients/csharp_location_sdk in the monorepo).

param(
    [string] $SpecPath = 'openapi/openapi.v1.json'
)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

if (-not (Test-Path $SpecPath)) {
    Write-Error "Missing $SpecPath. Fetch from ExploraProp/OpenAPI-contracts (newest apis/location/openapi.*.json)."
    exit 1
}

if (Test-Path '.config/dotnet-tools.json') {
    dotnet tool restore
}
elseif (Test-Path '../../.config/dotnet-tools.json') {
    dotnet tool restore --tool-manifest ../../.config/dotnet-tools.json
}
else {
    Write-Error 'No .config/dotnet-tools.json with microsoft.openapi.kiota found.'
    exit 1
}

dotnet kiota generate `
    -l CSharp `
    -d $SpecPath `
    -c LocationApiClient `
    -n ExploraProp.Location.ApiClient `
    -o src/Generated `
    --clean-output

Write-Host 'Kiota generation complete.'
