
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

// File: Services/IInferenceService.cs
using System.Threading.Tasks;

namespace AiApi.Services
{
    public interface IInferenceService
    {
        Task<string> PredictAsync(string input);
    }
}

// File: Services/OnnxInferenceService.cs
using System;
using System.Threading.Tasks;

namespace AiApi.Services
{
    public class OnnxInferenceService : IInferenceService
    {
        private readonly string _modelPath;

        // Constructor accepting configuration
        public OnnxInferenceService(string modelPath)
        {
            _modelPath = modelPath;
            // Simulate expensive model loading
            Console.WriteLine($"Loading AI Model from: {_modelPath}");
        }

        public Task<string> PredictAsync(string input)
        {
            // Simulate prediction logic
            return Task.FromResult($"Prediction for '{input}' using model at {_modelPath}: 0.85");
        }
    }
}

// File: Controllers/PredictionController.cs
using Microsoft.AspNetCore.Mvc;
using AiApi.Services;

namespace AiApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PredictionController : ControllerBase
    {
        private readonly IInferenceService _inferenceService;

        public PredictionController(IInferenceService inferenceService)
        {
            _inferenceService = inferenceService;
        }

        [HttpGet("predict")]
        public async Task<IActionResult> GetPrediction([FromQuery] string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return BadRequest("Input is required.");
            }

            var result = await _inferenceService.PredictAsync(input);
            return Ok(result);
        }
    }
}

// File: Program.cs
using AiApi.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Register IInferenceService as a Singleton using a factory delegate
// Why Singleton? AI models are often hundreds of megabytes. Loading them into memory
// is a CPU and memory-intensive operation. Singleton ensures the model is loaded once
// and reused for every request, preventing memory leaks and performance degradation 
// that would occur if we loaded the model for every request (Transient/Scoped).
builder.Services.AddSingleton<IInferenceService>(sp => 
{
    // Read configuration value
    var modelPath = builder.Configuration["ModelPath"] ?? "default_model.onnx";
    return new OnnxInferenceService(modelPath);
});

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
