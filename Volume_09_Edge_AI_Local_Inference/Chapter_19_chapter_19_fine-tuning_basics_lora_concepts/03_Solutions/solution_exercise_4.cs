
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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SafeEdgeDeployment
{
    public class SafeInferenceEngine
    {
        private readonly ILogger<SafeInferenceEngine> _logger;
        private readonly IConfiguration _configuration;
        private InferenceSession _currentSession;
        private bool _usingFallback = false;

        public SafeInferenceEngine(ILogger<SafeInferenceEngine> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public void Initialize()
        {
            string loraPath = _configuration["ModelPaths:LoRA"];
            string basePath = _configuration["ModelPaths:Base"];

            try
            {
                _logger.LogInformation($"Attempting to load LoRA model from {loraPath}...");
                
                // Validate file existence
                if (!File.Exists(loraPath)) throw new FileNotFoundException("LoRA model file not found.", loraPath);

                // Attempt Load
                var options = new SessionOptions();
                _currentSession = new InferenceSession(loraPath, options);
                _usingFallback = false;
                _logger.LogInformation("LoRA model loaded successfully.");
            }
            catch (OnnxRuntimeException ex)
            {
                _logger.LogError(ex, "ONNX Runtime error during LoRA model loading. Operator not supported or shape mismatch.");
                FallbackToBase(basePath);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, "LoRA model file missing.");
                FallbackToBase(basePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error loading LoRA model.");
                FallbackToBase(basePath);
            }
        }

        private void FallbackToBase(string basePath)
        {
            _logger.LogWarning("Falling back to base model.");
            try
            {
                if (!File.Exists(basePath)) throw new FileNotFoundException("Base model file missing. Cannot recover.");
                
                _currentSession?.Dispose();
                _currentSession = new InferenceSession(basePath);
                _usingFallback = true;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "CRITICAL: Base model also failed to load. Engine is dead.");
                throw;
            }
        }

        public IReadOnlyCollection<float> RunInference(float[] input)
        {
            if (_currentSession == null) throw new InvalidOperationException("Engine not initialized.");

            try
            {
                // Input Validation
                // Check if the model expects a specific shape. If LoRA changed it, we adapt here.
                var inputMeta = _currentSession.InputMetadata.First().Value;
                var expectedShape = inputMeta.Dimensions;
                
                // Simple shape check (assuming dynamic batch/seq, check rank)
                if (inputMeta.IsString) throw new InvalidOperationException("Model expects string input, but float array provided.");

                var tensor = new DenseTensor<float>(input, new int[] { 1, input.Length });
                var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input", tensor) };

                using (var results = _currentSession.Run(inputs))
                {
                    var output = results.First().AsTensor<float>().ToArray();
                    return output;
                }
            }
            catch (OnnxRuntimeException ex)
            {
                _logger.LogError(ex, "Inference failed. This might be due to input shape mismatch with the LoRA adapter.");
                // If running LoRA and it fails, we could attempt to reload base here, 
                // but usually inference failures are data-dependent, not model-structure dependent.
                throw;
            }
        }

        public bool IsUsingFallback() => _usingFallback;
    }

    // Mock Program to demonstrate usage
    class Program
    {
        static void Main(string[] args)
        {
            // Setup Configuration (Mocking appsettings.json)
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ModelPaths:Base"] = "phi3-base.onnx",
                    ["ModelPaths:LoRA"] = "phi3-lora-merged.onnx"
                })
                .Build();

            // Setup Logger (Mocking)
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<SafeInferenceEngine>();

            var engine = new SafeInferenceEngine(logger, config);
            
            try
            {
                engine.Initialize();
                
                // Simulate inference
                float[] dummyInput = new float[10]; 
                var result = engine.RunInference(dummyInput);
                
                if (engine.IsUsingFallback())
                {
                    Console.WriteLine("Result generated using BASE model (Fallback active).");
                }
                else
                {
                    Console.WriteLine("Result generated using LORA model.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Engine failed to start or run: {ex.Message}");
            }
        }
    }
}
