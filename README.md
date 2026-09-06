# csharp_location_sdk

Kiota-generated C# client for the **Location** OpenAPI contract.

| Item | Value |
|------|--------|
| Package | `ExploraProp.Location.ApiClient` |
| Repo | [`ExploraProp/csharp_location_sdk`](https://github.com/ExploraProp/csharp_location_sdk) |
| Contracts | `ExploraProp/OpenAPI-contracts` → newest `apis/location/openapi.<YYYY.MM.DD.HH.MM>.json` |
| Notify | `openapi-spec-updated` (`spec_path` in payload) |
| Generate | **API.Scripts** only (`generate-client`) — no `generate.ps1` |

## Local regenerate

Requires `API.Scripts` + `microsoft.openapi.kiota` from [`.config/dotnet-tools.json`](.config/dotnet-tools.json) (GitHub Packages auth for ExploraProp).

```powershell
dotnet tool restore
dotnet tool run api-scripts -- generate-client --spec openapi/openapi.v1.json --bc location --out src/Generated
dotnet pack src/ExploraProp.Location.ApiClient.csproj -c Release -p:PackageVersion=2026.9.2.1403
```

## CI

[`.github/workflows/openapi-spec-updated.yml`](.github/workflows/openapi-spec-updated.yml) on `repository_dispatch`:

1. Fetch spec from contracts (`CROSS_REPO_TOKEN`)
2. `api-scripts generate-client --spec … --bc location`
3. Pack as NuGet 4-part `YYYY.M.D.HHMM` from snapshot `openapi.YYYY.MM.DD.HH.MM.json` (e.g. `2026.9.2.1403`)
4. Push to GitHub Packages (`ExploraProp`)
5. Commit regenerated sources (`openapi/`, `src/Generated`)

## Consumers

External apps use NuGet:

```xml
<PackageReference Include="ExploraProp.Location.ApiClient" Version="2026.*" />
```

Map the package in `NuGet.Config` to `https://nuget.pkg.github.com/ExploraProp/index.json`. Prefer pinning the exact resolved version in lockfiles.

**location-api functional tests** use an ephemeral `ProjectReference` to `clients/ExploraProp.Location.ApiClient` generated via `api-scripts generate-client --service Location` (not this package).
