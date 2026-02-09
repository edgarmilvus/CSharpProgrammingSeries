
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

// File: Configuration/ChatEndpointOptions.cs
namespace Configuration;

public class ChatEndpointOptions
{
    public int MaxTokens { get; set; }
    public double Temperature { get; set; }
    public string SystemPrompt { get; set; } = "You are a helpful assistant.";
}

// File: Services/ChatController.cs
using Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Services;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IOptionsSnapshot<ChatEndpointOptions> _optionsSnapshot;

    public ChatController(IOptionsSnapshot<ChatEndpointOptions> optionsSnapshot)
    {
        _optionsSnapshot = optionsSnapshot;
    }

    [HttpGet("chat/{modelVersion}")]
    public IActionResult GetChatConfig(string modelVersion)
    {
        // Normalize input to match registered name
        var modelName = modelVersion.ToLower() switch
        {
            "gpt-4" => "Gpt4",
            "gpt-3.5-turbo" => "Gpt35",
            _ => null
        };

        if (modelName == null)
        {
            return NotFound($"Model version '{modelVersion}' is not supported.");
        }

        try
        {
            // Dynamically retrieve the named options
            var options = _optionsSnapshot.Get(modelName);
            
            return Ok(new 
            { 
                Model = modelName, 
                Config = options 
            });
        }
        catch (OptionsValidationException ex)
        {
            // Handle validation errors specific to the named option
            return BadRequest($"Configuration error for {modelName}: {string.Join(", ", ex.Failures)}");
        }
    }
}

// File: Program.cs (Relevant sections)
using Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// 1. Register Named Options
builder.Services.AddOptions<ChatEndpointOptions>("Gpt4")
    .Bind(builder.Configuration.GetSection("ChatModels:Gpt4"))
    .Validate(opt => opt.MaxTokens <= 4096, "GPT-4 MaxTokens cannot exceed 4096.")
    .ValidateOnStart();

builder.Services.AddOptions<ChatEndpointOptions>("Gpt35")
    .Bind(builder.Configuration.GetSection("ChatModels:Gpt35"));

// 2. Post-Configuration Logic (Alternative to inline Validate for complex logic)
// This is handled above via the .Validate() chain, but could be done via IOptionsFactory if needed.
// The requirement specifically asked for validation logic during creation, which .Validate() covers.

var app = builder.Build();
app.MapControllers();
app.Run();
