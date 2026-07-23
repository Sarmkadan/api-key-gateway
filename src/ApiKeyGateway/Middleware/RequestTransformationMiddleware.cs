// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text;
using ApiKeyGateway.Configuration;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Transformation;

namespace ApiKeyGateway.Middleware;

/// <summary>
/// ASP.NET Core middleware that drives the request transformation pipeline.
/// Positioned immediately after <see cref="ApiKeyAuthenticationMiddleware"/> so it
/// has access to the authenticated <see cref="ApiKey"/> identity stored in
/// <c>context.Items</c>.
/// </summary>
/// <remarks>
/// <para>
/// The middleware builds a <see cref="TransformationContext"/> from the live
/// <see cref="HttpRequest"/>, delegates execution to <see cref="ITransformationPipeline"/>, and then writes any mutations (headers, query parameters, path, body) back onto
/// the request before the pipeline continues to the next handler.
/// </para>
/// <para>
/// When any rule marks <see cref="TransformationContext.IsBlocked"/> as
/// <see langword="true"/> the request is terminated with <c>HTTP 403 Forbidden</c>
/// and the pipeline is short-circuited — no downstream middleware executes.
/// </para>
/// <para>
/// Body capture (controlled by <see cref="TransformationPipelineOptions.EnableBodyCapture"/>)
/// enables scripts to read and rewrite the raw request body. Because body buffering
/// incurs a memory allocation per request, it is disabled by default.
/// </para>
/// </remarks>
public sealed class RequestTransformationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITransformationPipeline _pipeline;
    private readonly TransformationPipelineOptions _options;
    private readonly ILogger<RequestTransformationMiddleware> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="RequestTransformationMiddleware"/>.
    /// </summary>
    public RequestTransformationMiddleware(
        RequestDelegate next,
        ITransformationPipeline pipeline,
        TransformationPipelineOptions options,
        ILogger<RequestTransformationMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes the HTTP request through the transformation pipeline and either
    /// blocks the request (HTTP 403) or passes it to the next middleware with
    /// mutations applied.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.IsEnabled)
        {
            await _next(context);
            return;
        }

        // Resolve identity written by ApiKeyAuthenticationMiddleware.
        var apiKey = context.Items.TryGetValue("ApiKey", out var k) ? k as ApiKey : null;
        var consumerId = context.Items.TryGetValue("ConsumerId", out var c) ? c as string : null;

        // Optionally buffer the request body so scripts can inspect it.
        if (_options.EnableBodyCapture && HasBody(context.Request))
            context.Request.EnableBuffering(_options.MaxBodySizeBytes);

        var transformContext = new TransformationContext(context.Request, apiKey?.Id, consumerId);

        // Capture body when buffering is enabled and the request has one.
        if (_options.EnableBodyCapture && HasBody(context.Request))
            transformContext.Body = await ReadBodyAsync(context.Request, context.RequestAborted);

        var result = await _pipeline.ApplyAsync(transformContext, context.RequestAborted);

        if (result.IsBlocked)
        {
            _logger.LogWarning(
                "Request blocked by transformation pipeline — reason: {BlockReason}",
                result.BlockReason ?? "(no reason supplied)");

            var problemDetails = GatewayProblemDetailsFactory.CreateTransformationBlockedProblem(
                context,
                result.BlockReason ?? "Request blocked by transformation policy");
            await context.WriteProblemAsync(problemDetails);

            return;
        }

        // Apply header mutations back onto the live request.
        ApplyHeaderMutations(context.Request, transformContext);

        // Apply query-string mutations.
        ApplyQueryMutations(context.Request, transformContext);

        // Apply path mutation when the script or built-in action rewrote it.
        if (!string.Equals(transformContext.Path, context.Request.Path.ToString(),
            StringComparison.OrdinalIgnoreCase))
        {
            context.Request.Path = new PathString(transformContext.Path);
        }

        // If the body was modified, swap in the rewritten content.
        if (_options.EnableBodyCapture && transformContext.Body is not null)
            await ReplaceBodyAsync(context.Request, transformContext.Body);

        if (!result.Success)
        {
            _logger.LogWarning(
                "Transformation pipeline completed with {ErrorCount} non-fatal error(s): {Errors}",
                result.Errors.Count,
                string.Join("; ", result.Errors.Select(e => $"{e.Key}: {e.Value}")));
        }

        await _next(context);
    }

    // -------------------------------------------------------------------------
    // Mutation helpers
    // -------------------------------------------------------------------------

    private static void ApplyHeaderMutations(HttpRequest request, TransformationContext ctx)
    {
        // Add or overwrite headers present in the transformed context.
        foreach (var (key, value) in ctx.Headers)
            request.Headers[key] = value;
    }

    private static void ApplyQueryMutations(HttpRequest request, TransformationContext ctx)
    {
        if (!ctx.QueryParameters.Any()) return;

        var qs = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(
            string.Empty, ctx.QueryParameters!);

        request.QueryString = new QueryString(qs);
    }

    private static bool HasBody(HttpRequest request) =>
        request.ContentLength > 0
        || string.Equals(request.Method, "POST", StringComparison.OrdinalIgnoreCase)
        || string.Equals(request.Method, "PUT", StringComparison.OrdinalIgnoreCase)
        || string.Equals(request.Method, "PATCH", StringComparison.OrdinalIgnoreCase);

    private static async Task<string?> ReadBodyAsync(HttpRequest request, CancellationToken ct)
    {
        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync(ct);
        request.Body.Position = 0;
        return body;
    }

    private static async Task ReplaceBodyAsync(HttpRequest request, string body)
    {
        var bytes = Encoding.UTF8.GetBytes(body);
        var stream = new MemoryStream(bytes);
        request.Body = stream;
        request.ContentLength = bytes.Length;
        request.Body.Position = 0;
        await Task.CompletedTask;
    }
}

/// <summary>
/// Extension methods that register and expose the request transformation middleware.
/// </summary>
public static class RequestTransformationMiddlewareExtensions
{
    /// <summary>
    /// Adds the <see cref="RequestTransformationMiddleware"/> to the application request pipeline.
    /// </summary>
    /// <remarks>
    /// Must be called <em>after</em> <c>UseApiKeyAuthentication()</c> so that the
    /// authenticated <see cref="ApiKey"/> identity is available to transformation rules.
    /// </remarks>
    /// <param name="app">The application builder.</param>
    /// <returns>The original <paramref name="app"/> for fluent chaining.</returns>
    public static IApplicationBuilder UseRequestTransformation(this IApplicationBuilder app) =>
        app.UseMiddleware<RequestTransformationMiddleware>();
}