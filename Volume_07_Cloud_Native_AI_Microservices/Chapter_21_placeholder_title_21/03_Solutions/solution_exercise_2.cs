
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

// Services/ModelLoaderService.cs
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace InferenceService.Services;

public class ModelLoaderService : IDisposable
{
    private readonly InferenceSession _session;
    private readonly ILogger<ModelLoaderService> _logger;
    private const string ModelName = "model.onnx";

    public ModelLoaderService(IWebHostEnvironment env, ILogger<ModelLoaderService> logger)
    {
        _logger = logger;
        var modelPath = Path.Combine(env.ContentRootPath, ModelName);
        
        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException($"Model not found at {modelPath}");
        }

        // SessionOptions is critical for performance and GPU support
        var options = new SessionOptions();
        
        // In a real GPU environment, you would configure CUDA provider
        // options.AppendExecutionProvider_CUDA(0); 
        
        _session = new InferenceSession(modelPath, options);
        _logger.LogInformation("Model loaded successfully. Input names: {Inputs}", string.Join(", ", _session.InputMetadata.Keys));
    }

    public InferenceSession Session => _session;

    public void Dispose()
    {
        _session?.Dispose();
    }
}
