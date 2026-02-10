
/*
#
# These sources are part of the "C# Programming Series" by Edgar Milvus, 
# for additional info, new volumes, link to stores:
# https://github.com/edgarmilvus/CSharpProgrammingSeries
#
# MIT License
# Copyright (c) 2026 Edgar Milvus
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in all
# copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
# SOFTWARE.
*/

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;

// 1. Configuration Options
// This class holds the configuration settings we will define in appsettings.json.
public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "ApiKey";
    public string? AuthenticationScheme { get; set; } = DefaultScheme;
    public string? ApiKeyHeaderName { get; set; } = "X-API-Key";
}

// 2. The Authentication Handler
// This is the core logic that validates the incoming request.
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    // We inject a configuration instance to retrieve the valid API keys.
    private readonly IConfiguration _configuration;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IConfiguration configuration) 
        : base(options, logger, encoder, clock)
    {
        _configuration = configuration;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Step 1: Check if the header exists
        if (!Request.Headers.TryGetValue(Options.ApiKeyHeaderName, out var apiKeyHeaderValues))
        {
            // No header found, skip authentication (allow other schemes to handle it)
            return AuthenticateResult.NoResult();
        }

        // Step 2: Ensure the header has a value
        var providedApiKey = apiKeyHeaderValues.FirstOrDefault();
        if (string.IsNullOrEmpty(providedApiKey))
        {
            return AuthenticateResult.NoResult();
        }

        // Step 3: Retrieve valid keys from configuration
        // In a real app, these would be stored in a database or Azure Key Vault.
        var validApiKeys = _configuration.GetSection("ApiKeys").Get<List<string>>();

        // Step 4: Validate the key
        if (validApiKeys != null && validApiKeys.Contains(providedApiKey))
        {
            // Key is valid! Create a "Claim" identity.
            // We treat the API Key as an identity. In complex scenarios, 
            // you might look up the owner of the key (e.g., "PartnerA") here.
            var claims = new[] { 
                new Claim(ClaimTypes.Name, "ApiKeyUser"),
                new Claim("ApiKey", providedApiKey) 
            };

            var identity = new ClaimsIdentity(claims, Options.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Options.Scheme);

            return AuthenticateResult.Success(ticket);
        }

        // Step 5: Key is invalid
        return AuthenticateResult.Fail("Invalid API Key provided.");
    }
}

// 3. Extension Method for easy registration
public static class ApiKeyAuthenticationExtensions
{
    public static IServiceCollection AddApiKeyAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = ApiKeyAuthenticationOptions.DefaultScheme;
            options.DefaultChallengeScheme = ApiKeyAuthenticationOptions.DefaultScheme;
        })
        .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
            ApiKeyAuthenticationOptions.DefaultScheme, 
            options => { });

        return services;
    }
}

// 4. Program.cs (The Application Entry Point)
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Register our custom Authentication Handler
builder.Services.AddApiKeyAuthentication();

// Configure Swagger to allow testing the API Key
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "API Key must appear in header: X-API-Key",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Name = "X-API-Key",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable Authentication middleware
app.UseAuthentication();
app.UseAuthorization();

// 5. The Protected Endpoint
// This endpoint requires a valid API Key to access.
app.MapGet("/ai/generate", (HttpContext context) =>
{
    // Retrieve the authenticated user info from the context
    var user = context.User;
    var apiKey = user.FindFirst("ApiKey")?.Value;

    return Results.Ok(new 
    { 
        Message = "AI Model Generated Image Successfully!", 
        AccessedBy = apiKey,
        Timestamp = DateTime.UtcNow
    };
})
.WithName("GenerateImage")
.WithOpenApi(); // Adds this endpoint to Swagger

// Run the app
app.Run();
