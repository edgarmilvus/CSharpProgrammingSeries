
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.IO;
using System.Threading.Tasks;
using LLama;
using LLama.Common;

namespace LlamaSharpHelloWorld
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. Define the model path
            // We assume you have downloaded a GGUF model file (e.g., "phi-2.Q4_K_M.gguf")
            // and placed it in the executable's directory or a known location.
            // For this example, we look for "phi-2.Q4_K_M.gguf" in the current directory.
            string modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "phi-2.Q4_K_M.gguf");

            if (!File.Exists(modelPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: Model file not found at {modelPath}");
                Console.WriteLine("Please download a Phi-2 GGUF model (e.g., from HuggingFace) and place it in the executable directory.");
                Console.ResetColor();
                return;
            }

            // 2. Configure execution parameters
            // We use the default parameters, but we explicitly set the GPU layer count to 0
            // to force CPU execution (ensuring this runs on any machine without CUDA/ROCm drivers).
            // In a real scenario with a dedicated GPU, you would set this to a high number (e.g., 33 for Phi-2).
            var parameters = new ModelParams(modelPath)
            {
                GpuLayerCount = 0 // 0 = CPU only
            };

            // 3. Load the model
            // The LLamaWeights object holds the loaded model weights in memory.
            // This is the most memory-intensive part of the process.
            using var model = await LLamaWeights.LoadFromFileAsync(parameters);

            // 4. Create an executor
            // The InteractiveExecutor handles the conversation state (history).
            // It manages the context window and prompt processing.
            var executor = new InteractiveExecutor(model);

            // 5. Define the prompt
            // We use a simple instruction format.
            string prompt = "Write a short, creative haiku about a robot running C# code.";

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"User: {prompt}");
            Console.ResetColor();
            Console.WriteLine("AI: ");

            // 6. Infer (Generate) the response
            // We stream the response token-by-token to the console.
            // The inference process calculates the probability distribution of the next token
            // based on the prompt and previous tokens, then samples according to parameters.
            await foreach (var token in executor.InferAsync(prompt, new InferenceParams()
            {
                Temperature = 0.7f, // Controls randomness (0.0 = deterministic, 1.0 = random)
                MaxTokens = 100,     // Maximum tokens to generate
                AntiPrompts = new[] { "\n", "User:" } // Stop generation if these strings appear
            }))
            {
                Console.Write(token);
            }

            Console.WriteLine();
        }
    }
}
