// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Configuration;
using ApiKeyGateway.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddControllers();
builder.Services.AddGatewayServices(builder.Configuration);
builder.Services.AddGatewayDocumentation();
builder.Services.AddHealthChecks().AddApiKeyGatewayHealthCheck();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Key Gateway v1");
        options.RoutePrefix = "docs";
    });
}

if (builder.Configuration.GetValue<bool>("Gateway:RequireSsl"))
{
    app.UseHttpsRedirection();
}

// Must be first app middleware so exceptions from authentication,
// transformation and controllers are converted to structured JSON errors.
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseCors("AllowAll");
app.UseApiKeyAuthentication();
app.UseRequestTransformation();
app.MapControllers();

app.MapHealthChecks("/health");

// Only checks tagged "ready" (see AddApiKeyGatewayHealthCheck). "/health/ready" is taken by HealthController.
app.MapHealthChecks("/health/checks", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains(HealthCheckRegistrationExtensions.ReadyTag)
});

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("API Key Gateway starting up");

app.Run();

/// <summary>
/// Exposes the implicit top-level Program class publicly so integration tests
/// can bootstrap the application via WebApplicationFactory&lt;Program&gt;.
/// </summary>
public partial class Program { }
