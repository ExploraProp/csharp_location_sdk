# csharp_location_sdk

Kiota-generated C# client for the **Location** OpenAPI contract.

| Item | Value |
|------|--------|
| Package | `ExploraProp.Location.ApiClient` |
| Repo | [`ExploraProp/csharp_location_sdk`](https://github.com/ExploraProp/csharp_location_sdk) |
| Contracts | `ExploraProp/OpenAPI-contracts` → newest `apis/location/openapi.<yyyyMMddHHmmss>Z.json` |
| Notify | `openapi-spec-updated` (`spec_path` in payload) |

## Local regenerate

```powershell
./generate.ps1 -SpecPath openapi/openapi.v1.json
dotnet pack src/ExploraProp.Location.ApiClient.csproj -c Release
```

## CI

[`.github/workflows/openapi-spec-updated.yml`](.github/workflows/openapi-spec-updated.yml) on `repository_dispatch`:

1. Fetch spec from contracts (`CROSS_REPO_TOKEN`)
2. Kiota generate
3. Pack as `0.0.0-openapi.<timestamp>Z`
4. Push to GitHub Packages (`ExploraProp`)
5. Commit regenerated sources

## Consumers (functional tests)

```xml
<PackageReference Include="ExploraProp.Location.ApiClient" Version="0.0.0-openapi.*" />
```

Map the package in `NuGet.Config` to `https://nuget.pkg.github.com/ExploraProp/index.json`. Prefer a pinned prerelease version in lockfiles.
