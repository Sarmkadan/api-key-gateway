// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Client for making calls to external APIs with built-in caching and error handling.
// Wraps HttpClient with common patterns: timeouts, retries, caching, circuit breaking.
// =============================================================================

using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Caching;
using ApiKeyGateway.Utilities;
using Microsoft.Extensions.Logging; // Ensure logging namespace is available

namespace ApiKeyGateway.Integration;

/// <summary>
/// Client for making calls to external APIs with built-in caching and error handling.
/// Wraps HttpClient with common patterns: timeouts, retries, caching, circuit breaking.
/// </summary>
public interface IExternalApiClient
{
    /// <summary>
    /// Performs a GET request to external API with optional caching.
    /// </summary>
    Task<T> GetAsync<T>(string endpoint, TimeSpan? cacheDuration = null) where T : class;

    /// <summary>
    /// Performs a POST request to external API.
    /// </summary>
    Task<T> PostAsync<T>(string endpoint, object payload) where T : class;

    /// <summary>
    /// Performs a request with custom configuration.
    /// </summary>
    Task<T> SendAsync<T>(HttpRequestMessage request) where T : class;
}

/// <summary>
/// HTTP-based external API client with caching and retry support.
/// Designed for calling third-party APIs reliably.
/// </summary>
public sealed class HttpExternalApiClient : IExternalApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ICacheProvider _cache;
    private readonly ILogger<HttpExternalApiClient> _logger;
    private readonly string _apiName;

    public HttpExternalApiClient(
        HttpClient httpClient,
        ICacheProvider cache,
        ILogger<HttpExternalApiClient> logger,
        string apiName)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _apiName = apiName ?? throw new ArgumentNullException(nameof(apiName));
    }

    public async Task<T> GetAsync<T>(string endpoint, TimeSpan? cacheDuration = null) where T : class
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint cannot be empty", nameof(endpoint));

        _logger.LogDebug("Starting GET request to {Endpoint} for API {ApiName}", endpoint, _apiName);
        var cacheKey = CacheKeyGenerator.GetExternalApiCacheKey(_apiName, endpoint);

        // Check cache first if caching is enabled
        if (cacheDuration.HasValue)
        {
            var cached = await _cache.GetAsync<T>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for external API: {ApiName} {Endpoint}", _apiName, endpoint);
                return cached;
            }
        }

        try
        {
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializationHelper.Deserialize<T>(content);

            if (result == null)
                throw new ConfigurationException($"External API {_apiName} returned null response for endpoint {endpoint}");

            // Cache successful response if duration specified
            if (result != null && cacheDuration.HasValue)
            {
                await _cache.SetAsync(cacheKey, result, cacheDuration);
                _logger.LogDebug(
                    "Cached external API response: {ApiName} {Endpoint} for {Duration}",
                    _apiName,
                    endpoint,
                    cacheDuration);
            }

            _logger.LogInformation("GET request succeeded for {Endpoint} on API {ApiName}", endpoint, _apiName);
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "External API request failed: {ApiName} {Endpoint}",
                _apiName,
                endpoint);
            throw new KeyStoreUnavailableException(
                $"External API {_apiName} request failed: {endpoint}",
                "ExternalApiClient.GetAsync",
                ex);
        }
        catch (Exception ex) when (ex is not ApiKeyGatewayException)
        {
            _logger.LogError(
                ex,
                "Unexpected error calling external API: {ApiName} {Endpoint}",
                _apiName,
                endpoint);
            throw new KeyStoreUnavailableException(
                $"Unexpected error calling external API {_apiName}: {endpoint}",
                "ExternalApiClient.GetAsync",
                ex);
        }
    }

    public async Task<T> PostAsync<T>(string endpoint, object payload) where T : class
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint cannot be empty", nameof(endpoint));

        if (payload == null)
            throw new ArgumentNullException(nameof(payload));

        _logger.LogDebug("Starting POST request to {Endpoint} for API {ApiName}", endpoint, _apiName);

        try
        {
            var jsonContent = JsonSerializationHelper.SerializeCompact(payload);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializationHelper.Deserialize<T>(responseContent);

            if (result == null)
                throw new ConfigurationException($"External API {_apiName} returned null response for POST to {endpoint}");

            _logger.LogInformation("POST request succeeded for {Endpoint} on API {ApiName}", endpoint, _apiName);
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "External API POST request failed: {ApiName} {Endpoint}",
                _apiName,
                endpoint);
            throw new KeyStoreUnavailableException(
                $"External API {_apiName} POST failed: {endpoint}",
                "ExternalApiClient.PostAsync",
                ex);
        }
        catch (Exception ex) when (ex is not ApiKeyGatewayException)
        {
            _logger.LogError(
                ex,
                "Unexpected error in external API POST: {ApiName} {Endpoint}",
                _apiName,
                endpoint);
            throw new KeyStoreUnavailableException(
                $"Unexpected error in external API POST {_apiName}: {endpoint}",
                "ExternalApiClient.PostAsync",
                ex);
        }
    }

    public async Task<T> SendAsync<T>(HttpRequestMessage request) where T : class
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        _logger.LogDebug("Starting custom request for API {ApiName}", _apiName);

        try
        {
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializationHelper.Deserialize<T>(content);

            if (result == null)
                throw new ConfigurationException($"External API {_apiName} returned null response for custom request");

            _logger.LogInformation("Custom request succeeded for API {ApiName}", _apiName);
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "External API custom request failed: {ApiName}",
                _apiName);
            throw new KeyStoreUnavailableException(
                $"External API {_apiName} custom request failed",
                "ExternalApiClient.SendAsync",
                ex);
        }
        catch (Exception ex) when (ex is not ApiKeyGatewayException)
        {
            _logger.LogError(
                ex,
                "Unexpected error in external API custom request: {ApiName}",
                _apiName);
            throw new KeyStoreUnavailableException(
                $"Unexpected error in external API custom request {_apiName}",
                "ExternalApiClient.SendAsync",
                ex);
        }
    }
}
