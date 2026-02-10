
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

// 1. Define InputItem
public class InputItem
{
    /// <summary>
    /// Unique identifier for the input item.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The content to process.
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;
}

// 2. Define Parameters with Swagger Examples
public class InferenceParameters
{
    /// <summary>
    /// Controls randomness. Lower is more deterministic.
    /// </summary>
    [SwaggerSchema(Description = "Controls randomness (0.0 - 1.0).")]
    [SwaggerExample("0.7")] // Provides a concrete example in the UI
    public double? Temperature { get; set; } 

    /// <summary>
    /// Maximum tokens to generate.
    /// </summary>
    [SwaggerExample("1024")]
    public int? MaxTokens { get; set; }
}

// 3. Define the Main Request Model
public class InferenceRequest
{
    /// <summary>
    /// List of input items to process.
    /// </summary>
    [Required]
    public List<InputItem> InputData { get; set; } = new();

    /// <summary>
    /// Optional parameters for the inference.
    /// </summary>
    public InferenceParameters? Parameters { get; set; }
}

// 4. Controller with XML Comments and Attributes
[ApiController]
[Route("api/[controller]")]
public class ModelController : ControllerBase
{
    /// <summary>
    /// Performs complex inference on structured input data.
    /// </summary>
    /// <remarks>
    /// This endpoint accepts a list of items and optional hyperparameters.
    /// If parameters are omitted, default values are used.
    /// </remarks>
    [HttpPost("infer")]
    [SwaggerOperation(Summary = "Run inference", Description = "Processes complex inputs.")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult Infer([FromBody] InferenceRequest request)
    {
        // Mock implementation
        return Ok($"Processed {request.InputData.Count} items with Temp: {request.Parameters?.Temperature ?? 0.5}");
    }
}
