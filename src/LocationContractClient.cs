using System.Net;
using System.Text.Json;
using ExploraProp.Location.ApiClient.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace ExploraProp.Location.ApiClient;

/// <summary>
/// Factory for the Kiota <see cref="LocationApiClient"/> bound to an existing <see cref="HttpClient"/>
/// (e.g. hybrid functional-test mesh client).
/// </summary>
public static class LocationApiClientFactory
{
    public static LocationApiClient Create(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        var adapter = new HttpClientRequestAdapter(
            new AnonymousAuthenticationProvider(),
            httpClient: httpClient)
        {
            BaseUrl = httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "http://localhost"
        };

        return new LocationApiClient(adapter);
    }
}

/// <summary>
/// Location HTTP surface built from OpenAPI (Kiota). Asserts the published wire contract
/// without referencing Location.Application DTOs.
/// </summary>
public sealed class LocationContractClient(HttpClient httpClient)
{
    private readonly LocationApiClient _api = LocationApiClientFactory.Create(httpClient);

    public Task<ContractResponse<ListCountriesResponseDto>> ListCountriesAsync(
        CancellationToken cancellationToken = default) =>
        InvokeAsync(() => _api.Api.V1.Country.GetAsync(cancellationToken: cancellationToken));

    public Task<ContractResponse<CountryDto>> GetCountryByIsoCodeAsync(
        string isoCode,
        CancellationToken cancellationToken = default) =>
        InvokeAsync(() => _api.Api.V1.Country[isoCode].GetAsync(cancellationToken: cancellationToken));

    public Task<ContractResponse<PaginatedResultOfAdministrativeDivisionDto>> ListAdministrativeDivisionsAsync(
        string? country = null,
        string? parentId = null,
        string? levelId = null,
        string? sortBy = null,
        string? sortDirection = null,
        int? pageNumber = null,
        int? pageSize = null,
        string? cursor = null,
        CancellationToken cancellationToken = default) =>
        InvokeAsync(() => _api.Api.V1.AdministrativeDivisions.GetAsync(config =>
        {
            config.QueryParameters.Country = country;
            config.QueryParameters.ParentId = parentId;
            config.QueryParameters.LevelId = levelId;
            config.QueryParameters.SortBy = sortBy;
            config.QueryParameters.SortDirection = sortDirection;
            config.QueryParameters.PageNumber = pageNumber;
            config.QueryParameters.PageSize = pageSize;
            config.QueryParameters.Cursor = cursor;
        }, cancellationToken));

    public Task<ContractResponse<AdministrativeDivisionDto>> GetAdministrativeDivisionByIdAsync(
        string divisionId,
        CancellationToken cancellationToken = default) =>
        InvokeAsync(() => _api.Api.V1.AdministrativeDivisions[divisionId].GetAsync(cancellationToken: cancellationToken));

    public Task<ContractResponse<SearchAdministrativeDivisionsByNameResultDto>> SearchAdministrativeDivisionsAsync(
        string? country = null,
        string? name = null,
        CancellationToken cancellationToken = default) =>
        InvokeAsync(() => _api.Api.V1.AdministrativeDivisions.Search.GetAsync(config =>
        {
            config.QueryParameters.Country = country;
            config.QueryParameters.Name = name;
        }, cancellationToken));

    public Task<ContractResponse<ListAdministrativeLevelsResponseDto>> ListAdministrativeLevelsAsync(
        string? country = null,
        CancellationToken cancellationToken = default) =>
        InvokeAsync(() => _api.Api.V1.AdministrativeLevels.GetAsync(config =>
        {
            config.QueryParameters.Country = country;
        }, cancellationToken));

    private static async Task<ContractResponse<T>> InvokeAsync<T>(Func<Task<T?>> call)
    {
        try
        {
            var data = await call();
            return new ContractResponse<T>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Data = data
            };
        }
        catch (ApiException ex)
        {
            ContractProblemDetails? error = null;
            if (!string.IsNullOrWhiteSpace(ex.Message))
            {
                try
                {
                    error = JsonSerializer.Deserialize<ContractProblemDetails>(ex.ResponseHeaders is null
                        ? ex.Message
                        : ex.Message, ContractJson.Options);
                }
                catch (JsonException)
                {
                    error = new ContractProblemDetails { Detail = ex.Message, Title = ex.GetType().Name };
                }
            }

            var status = ex.ResponseStatusCode is > 0
                ? (HttpStatusCode)ex.ResponseStatusCode
                : HttpStatusCode.InternalServerError;

            return new ContractResponse<T>
            {
                StatusCode = status,
                IsSuccess = false,
                Error = error ?? new ContractProblemDetails { Detail = ex.Message },
                RawContent = ex.Message
            };
        }
    }
}

public sealed class ContractResponse<T>
{
    public HttpStatusCode StatusCode { get; init; }
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public ContractProblemDetails? Error { get; init; }
    public string? RawContent { get; init; }
}

public sealed class ContractProblemDetails
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int? Status { get; set; }
    public string? Detail { get; set; }
    public string? Instance { get; set; }
}

file static class ContractJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
