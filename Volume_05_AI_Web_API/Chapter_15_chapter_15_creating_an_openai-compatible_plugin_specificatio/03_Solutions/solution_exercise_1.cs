
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace LegacyPlugin.Controllers
{
    [ApiController]
    [Route(".well-known")]
    public class PluginManifestController : ControllerBase
    {
        [HttpGet("ai-plugin.json")]
        [Produces("application/json")]
        public IActionResult GetManifest()
        {
            var manifest = new PluginManifest
            {
                SchemaVersion = "v1",
                NameForModel = "EmployeeLookup",
                NameForHuman = "Employee Directory",
                DescriptionForModel = "Use this tool to retrieve employee details by ID. Useful for HR queries.",
                DescriptionForHuman = "Look up employee information.",
                Auth = new AuthConfig { Type = "none" },
                Api = new ApiConfig
                {
                    Type = "openapi",
                    Url = "http://localhost:5000/.well-known/openapi.json",
                    IsUserAuthenticated = false
                },
                LogoUrl = "http://localhost:5000/logo.png",
                ContactEmail = "support@example.com",
                LegalInfoUrl = "http://example.com/legal"
            };

            return Ok(manifest);
        }
    }

    public class PluginManifest
    {
        [JsonPropertyName("schema_version")]
        public string SchemaVersion { get; set; }

        [JsonPropertyName("name_for_model")]
        public string NameForModel { get; set; }

        [JsonPropertyName("name_for_human")]
        public string NameForHuman { get; set; }

        [JsonPropertyName("description_for_model")]
        public string DescriptionForModel { get; set; }

        [JsonPropertyName("description_for_human")]
        public string DescriptionForHuman { get; set; }

        [JsonPropertyName("auth")]
        public AuthConfig Auth { get; set; }

        [JsonPropertyName("api")]
        public ApiConfig Api { get; set; }

        [JsonPropertyName("logo_url")]
        public string LogoUrl { get; set; }

        [JsonPropertyName("contact_email")]
        public string ContactEmail { get; set; }

        [JsonPropertyName("legal_info_url")]
        public string LegalInfoUrl { get; set; }
    }

    public class AuthConfig
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class ApiConfig
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("is_user_authenticated")]
        public bool IsUserAuthenticated { get; set; }
    }
}

// Unit Test (using xUnit and WebApplicationFactory)
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Xunit;

public class PluginManifestTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PluginManifestTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetManifest_ReturnsSuccessAndCorrectContentType()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/.well-known/ai-plugin.json");

        response.EnsureSuccessStatusCode(); // Assert HTTP 200
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var manifest = await response.Content.ReadFromJsonAsync<PluginManifest>();
        Assert.NotNull(manifest);
        Assert.Equal("EmployeeLookup", manifest.NameForModel);
    }
}
