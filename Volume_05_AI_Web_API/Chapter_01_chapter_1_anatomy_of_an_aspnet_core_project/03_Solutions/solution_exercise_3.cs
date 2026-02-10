
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

// File: GlobalUsings.cs
global using Microsoft.AspNetCore.Mvc;
global using System.Collections.Generic;
global using System.Threading.Tasks;

// File: Models/DTOs/InferenceRequest.cs
namespace AiApi.Models.DTOs
{
    public record InferenceRequest(string InputData, Dictionary<string, object> Parameters);
}

// File: Models/Core/TensorData.cs (Hypothetical domain model)
namespace AiApi.Models.Core
{
    public class TensorData
    {
        public float[] Values { get; set; }
        public int[] Dimensions { get; set; }
    }
}

// File: Services/ITokenizerService.cs
namespace AiApi.Services
{
    public interface ITokenizerService
    {
        string Tokenize(string input);
    }
}

// File: Services/IModelLoader.cs
namespace AiApi.Services
{
    public interface IModelLoader
    {
        void LoadModel(string modelName);
    }
}

// File: Services/ModelOrchestrator.cs
namespace AiApi.Services
{
    public class ModelOrchestrator
    {
        private readonly ITokenizerService _tokenizer;
        private readonly IModelLoader _modelLoader;

        public ModelOrchestrator(ITokenizerService tokenizer, IModelLoader modelLoader)
        {
            _tokenizer = tokenizer;
            _modelLoader = modelLoader;
        }

        public string ProcessRequest(Models.DTOs.InferenceRequest request)
        {
            // Simulate processing logic
            _modelLoader.LoadModel("default");
            var tokens = _tokenizer.Tokenize(request.InputData);
            return $"Processed input: {request.InputData} with {tokens.Length} tokens.";
        }
    }
}

// File: Controllers/InferenceController.cs
using AiApi.Models.DTOs;
using AiApi.Services;

namespace AiApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InferenceController : ControllerBase
    {
        private readonly ModelOrchestrator _orchestrator;

        public InferenceController(ModelOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpPost("process")]
        public IActionResult Process([FromBody] InferenceRequest request)
        {
            var result = _orchestrator.ProcessRequest(request);
            return Ok(result);
        }
    }
}

// File: Program.cs
using AiApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddSingleton<ITokenizerService, DummyTokenizerService>();
builder.Services.AddSingleton<IModelLoader, DummyModelLoader>();
builder.Services.AddSingleton<ModelOrchestrator>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Dummy implementations for compilation
namespace AiApi.Services
{
    public class DummyTokenizerService : ITokenizerService { public string Tokenize(string input) => input; }
    public class DummyModelLoader : IModelLoader { public void LoadModel(string modelName) { } }
}
