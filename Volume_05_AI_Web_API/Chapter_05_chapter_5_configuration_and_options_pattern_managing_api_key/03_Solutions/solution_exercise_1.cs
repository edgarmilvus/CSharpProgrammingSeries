
#
# These sources are part of the "C# Programming Series" by Edgar Milvus, 
# you can find it on stores: 
# 
# https://www.amazon.com/dp/B0GKJ3NYL6 or https://tinyurl.com/CSharpProgrammingBooks or 
# https://leanpub.com/u/edgarmilvus (quantity discounts)
# 
# New books info: https://linktr.ee/edgarmilvus 
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

// File: Configuration/LlmProviderOptions.cs
namespace Configuration;

public class LlmProviderOptions
{
    // Required property - no default value
    public string ApiKey { get; set; } = string.Empty;

    // Required property - Uri type ensures valid URL format
    public Uri BaseUrl { get; set; } = null!;

    // Optional property with default value
    public int TimeoutSeconds { get; set; } = 30;
}

// File: Services/LlmService.cs
using Configuration;
using Microsoft.Extensions.Options;

namespace Services;

public class LlmService
{
    private readonly LlmProviderOptions _options;

    public LlmService(IOptions<LlmProviderOptions> options)
    {
        _options = options.Value;
    }

    public string GetProviderConfig()
    {
        // Example usage of the strongly-typed options
        return $"Provider: {_options.BaseUrl}, Key Length: {_options.ApiKey.Length}, Timeout: {_options.TimeoutSeconds}s";
    }
}

// File: Program.cs (Relevant sections)
using Configuration;
using Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Register the options bound to the "LlmProvider" section
builder.Services.Configure<LlmProviderOptions>(
    builder.Configuration.GetSection("LlmProvider"));

// 2. Register the service
builder.Services.AddScoped<LlmService>();

// 3. Validation Logic (Fail Fast)
// We manually validate here to ensure ApiKey is present before the app starts.
// In a real-world scenario, you might use IOptionsValidator or Data Annotations (see Exercise 3).
var config = builder.Configuration.GetSection("LlmProvider").Get<LlmProviderOptions>();
if (string.IsNullOrWhiteSpace(config?.ApiKey))
{
    throw new InvalidOperationException("Configuration validation failed: 'LlmProvider:ApiKey' is missing or empty.");
}

var app = builder.Build();
app.Run();
