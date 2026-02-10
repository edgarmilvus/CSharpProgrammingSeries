
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

// 1. Project Setup (Program.cs)
// Ensure you have the Swashbuckle.AspNetCore NuGet package installed.
// dotnet add package Swashbuckle.AspNetCore

using Microsoft.OpenApi.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2. Configure Swagger Generator
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "AI Model API",
        Description = "An API for classifying text sentiment.",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Support",
            Email = "support@example.com"
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    // 7. Enable Swagger UI middleware
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Model API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at the root
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// --------------------------------------------------

// 3. Define Request/Response Models
public class ClassificationRequest
{
    /// <summary>
    /// The text content to be classified.
    /// </summary>
    [Required(ErrorMessage = "Text is required.")]
    public string Text { get; set; } = string.Empty;
}

public class ClassificationResult
{
    /// <summary>
    /// The predicted label (e.g., "Positive", "Negative").
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Confidence score between 0.0 and 1.0.
    /// </summary>
    public double Confidence { get; set; }
}

// 4. Create API Controller
[ApiController]
[Route("api/[controller]")]
public class ModelController : ControllerBase
{
    /// <summary>
    /// Classifies the input text into a sentiment category.
    /// </summary>
    [HttpPost("classify")]
    // 6. Explicitly define response types for Swagger
    [ProducesResponseType(typeof(ClassificationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Classify([FromBody] ClassificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest("Text cannot be empty.");
        }

        // Mock implementation
        var result = new ClassificationResult
        {
            Label = "Positive",
            Confidence = 0.95
        };

        return Ok(result);
    }
}
