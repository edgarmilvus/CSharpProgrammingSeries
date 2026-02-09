
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

// File: Configuration/ValidatedOptions.cs
using System.ComponentModel.DataAnnotations;

namespace Configuration;

public class ValidatedOptions
{
    [Required(ErrorMessage = "API Key is mandatory.")]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    [Url(ErrorMessage = "Base URL must be a valid URL.")]
    public string BaseUrl { get; set; } = string.Empty;

    [Range(10, 300, ErrorMessage = "Timeout must be between 10 and 300 seconds.")]
    public int TimeoutSeconds { get; set; } = 30;
}

// File: Program.cs (Relevant sections)
using Configuration;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Options services (if not already added by WebApplicationBuilder)
builder.Services.AddOptions();

// 2. Register options with validation
builder.Services.AddOptions<ValidatedOptions>()
    .Bind(builder.Configuration.GetSection("LlmProvider"))
    .ValidateDataAnnotations() // Enforces the attributes defined in the class
    .ValidateOnStart(); // Ensures validation runs at startup, not first request

// Note: If validation fails, the app will throw an OptionsValidationException 
// during the startup sequence (when the host is built).

var app = builder.Build();
app.Run();
