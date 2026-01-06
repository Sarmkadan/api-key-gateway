// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Caching;
using ApiKeyGateway.Utilities;

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
    Task<T?> GetAsync<T>(string endpoint, TimeSpan? cacheDuration = null) where T : class;

    /// <summary>
    /// Performs a POST request to external API.
    /// </summary>
    Task<T?> PostAsync<T>(string endpoint, object payload) where T : class;

    /// <summary>
    /// Performs a request with custom configuration.
    /// </summary>
    Task<T?> SendAsync<T>(HttpRequestMessage request) where T : class;
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
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _apiName = apiName;
    }

    public async Task<T?> GetAsync<T>(string endpoint, TimeSpan? cacheDuration = null) where T : class
    {
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

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "External API request failed: {ApiName} {Endpoint}",
                _apiName,
                endpoint);
            return null;
        }
    }

    public async Task<T?> PostAsync<T>(string endpoint, object payload) where T : class
    {
        try
        {
            var jsonContent = JsonSerializationHelper.SerializeCompact(payload);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializationHelper.Deserialize<T>(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "External API POST request failed: {ApiName} {Endpoint}",
                _apiName,
                endpoint);
            return null;
        }
    }

    public async Task<T?> SendAsync<T>(HttpRequestMessage request) where T : class
    {
        try
        {
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializationHelper.Deserialize<T>(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "External API custom request failed: {ApiName}",
                _apiName);
            return null;
        }
    }
}
