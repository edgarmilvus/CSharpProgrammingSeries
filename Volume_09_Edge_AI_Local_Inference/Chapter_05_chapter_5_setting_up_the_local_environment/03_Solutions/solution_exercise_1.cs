
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

using System;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace LocalAI.Inference
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing LocalAI Environment...");

            try
            {
                // Access the singleton instance of the ONNX Runtime environment
                var ortEnv = OrtEnv.Instance;

                // Get the SessionOptions from the environment to check providers
                // Note: In newer versions, GetAvailableProviders is on OrtEnv or SessionOptions
                var availableProviders = ortEnv.SessionOptions.GetAvailableProviders();

                Console.WriteLine("\nAvailable Execution Providers:");
                foreach (var provider in availableProviders)
                {
                    Console.WriteLine($" - {provider}");
                }

                // Verify specific expected providers
                if (Array.Exists(availableProviders, p => p == "CUDAExecutionProvider"))
                {
                    Console.WriteLine("\n[SUCCESS] CUDA Execution Provider is available.");
                }
                else
                {
                    Console.WriteLine("\n[INFO] CUDA Execution Provider not found. Falling back to CPU.");
                }
            }
            catch (DllNotFoundException ex)
            {
                // This catches missing native binaries (onnxruntime.dll, onnxruntime_providers_cuda.dll, etc.)
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[CRITICAL ERROR] Native library not found.");
                Console.ResetColor();
                Console.WriteLine($"Details: {ex.Message}");
                Console.WriteLine("Suggestion: Ensure that the ONNX Runtime native dependencies are installed.");
                Console.WriteLine("If using CUDA, verify that CUDA Toolkit and cuDNN are installed and in the PATH.");
            }
            catch (OnnxRuntimeException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[CRITICAL ERROR] ONNX Runtime initialization failed.");
                Console.ResetColor();
                Console.WriteLine($"Details: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
