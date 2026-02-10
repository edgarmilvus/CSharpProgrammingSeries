
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt; // Conceptually used for validation logic if JWT, but here simple string check

namespace LegacyPlugin.Filters
{
    // 1. Custom Attribute accepting Options for DI
    public class PluginAuthAttribute : TypeFilterAttribute
    {
        public PluginAuthAttribute() : base(typeof(PluginAuthFilter)) { }
    }

    // 2. The Filter Implementation
    public class PluginAuthFilter : IAuthorizationFilter
    {
        private readonly PluginAuthOptions _options;

        public PluginAuthFilter(IOptions<PluginAuthOptions> options)
        {
            _options = options.Value;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Check for Authorization header
            if (!context.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Validate Bearer Token
            var token = authHeader.ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token) || !_options.AllowedTokens.Contains(token))
            {
                context.Result = new UnauthorizedResult();
                return;
            }
        }
    }

    // 3. Configuration Options Class
    public class PluginAuthOptions
    {
        public List<string> AllowedTokens { get; set; } = new List<string>();
    }
}

// Program.cs Configuration
using LegacyPlugin.Filters;

var builder = WebApplication.CreateBuilder(args);

// Bind configuration (e.g., appsettings.json: { "PluginAuth": { "AllowedTokens": [ "sk-1234567890", "sk-abc" ] } })
builder.Services.Configure<PluginAuthOptions>(builder.Configuration.GetSection("PluginAuth"));

builder.Services.AddControllers();

var app = builder.Build();

// ... middleware ...

app.MapControllers();

app.Run();

// Updated Manifest Controller (Snippet)
[HttpGet("ai-plugin.json")]
public IActionResult GetManifest()
{
    var manifest = new PluginManifest
    {
        // ... other properties ...
        Auth = new AuthConfig 
        { 
            Type = "service_http",
            AuthorizationType = "bearer" 
        },
        // ...
    };
    return Ok(manifest);
}

// Updated Manifest Model
public class AuthConfig
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("authorization_type")]
    public string AuthorizationType { get; set; }
}
