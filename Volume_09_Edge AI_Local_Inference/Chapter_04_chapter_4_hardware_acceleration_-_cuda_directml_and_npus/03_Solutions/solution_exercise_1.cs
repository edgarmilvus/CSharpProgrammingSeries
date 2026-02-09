
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

using System;
using System.Runtime.InteropServices;
using Microsoft.ML.OnnxRuntime;

namespace HardwareAccelerationExercises
{
    public static class HardwareAcceleratorFactory
    {
        /// <summary>
        /// Probes system capabilities to determine the optimal execution provider.
        /// Priority: CUDA > DirectML (Windows) > CPU.
        /// </summary>
        /// <returns>The string name of the execution provider.</returns>
        public static string GetOptimalExecutionProvider()
        {
            Console.WriteLine("=== Hardware Detection Started ===");

            // 1. Check for NVIDIA CUDA
            if (IsCudaAvailable())
            {
                Console.WriteLine("✅ [Detection] NVIDIA GPU and CUDA libraries detected.");
                Console.WriteLine("Selected Provider: CUDAExecutionProvider");
                return "CUDAExecutionProvider";
            }

            // 2. Check for DirectML (Windows Only)
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && IsDirectMLAvailable())
            {
                Console.WriteLine("✅ [Detection] Windows OS detected and DirectML support verified.");
                Console.WriteLine("Selected Provider: DmlExecutionProvider");
                return "DmlExecutionProvider";
            }

            // 3. Fallback to CPU
            Console.WriteLine("⚠️ [Detection] No compatible GPU hardware found or libraries missing.");
            Console.WriteLine("Selected Provider: CPUExecutionProvider");
            return "CPUExecutionProvider";
        }

        /// <summary>
        /// Simulates checking for CUDA availability.
        /// In a real scenario, this might P/Invoke cudaRuntimeGetVersion or check for specific DLLs.
        /// </summary>
        private static bool IsCudaAvailable()
        {
            try
            {
                // Check if the CUDA execution provider is registered in the ONNX Runtime
                // Note: This is a simulation. Real implementation might involve checking native library existence.
                var availableProviders = OrtEnv.Instance().GetAvailableProviders();
                foreach (var provider in availableProviders)
                {
                    if (provider.Contains("CUDA", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Debug] Exception checking CUDA: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Simulates checking for DirectML availability on Windows.
        /// </summary>
        private static bool IsDirectMLAvailable()
        {
            try
            {
                // Check if DirectML provider is available in the ONNX Runtime build
                var availableProviders = OrtEnv.Instance().GetAvailableProviders();
                foreach (var provider in availableProviders)
                {
                    if (provider.Contains("DML", StringComparison.OrdinalIgnoreCase) || 
                        provider.Contains("DirectML", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Debug] Exception checking DirectML: {ex.Message}");
                return false;
            }
        }
    }
}
