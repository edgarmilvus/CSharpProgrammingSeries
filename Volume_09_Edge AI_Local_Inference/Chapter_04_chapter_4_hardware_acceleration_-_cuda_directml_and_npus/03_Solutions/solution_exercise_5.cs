
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using System;
using System.IO;
using Microsoft.ML.OnnxRuntime;

namespace HardwareAccelerationExercises
{
    public class RobustSessionFactory
    {
        /// <summary>
        /// Creates an InferenceSession with robust fallback logic.
        /// </summary>
        public static InferenceSession CreateSessionWithFallback(string modelPath)
        {
            // Prioritize CUDA as per requirements
            string primaryProvider = "CUDAExecutionProvider";
            
            // 1. Try-Catch Strategy: Attempt Primary Hardware
            try
            {
                Console.WriteLine($"Attempting to initialize session with {primaryProvider}...");
                
                using var options = new SessionOptions();
                options.AppendExecutionProvider_CUDA(0); // Attempt to attach CUDA
                
                // If we reach here, CUDA libraries are present. 
                // Note: We must return the session, so we cannot use 'using' here 
                // because the caller needs to dispose it later.
                return new InferenceSession(modelPath, options);
            }
            catch (DllNotFoundException ex)
            {
                // 2. Fallback Mechanism
                Console.WriteLine($"⚠️ Hardware Acceleration Failed: {ex.Message}");
                Console.WriteLine($"CUDA library not found. Falling back to CPU.");
                
                return CreateCpuSession(modelPath);
            }
            catch (OnnxRuntimeException ex)
            {
                Console.WriteLine($"⚠️ ONNX Runtime Error: {ex.Message}");
                Console.WriteLine($"Hardware initialization failed. Falling back to CPU.");
                
                return CreateCpuSession(modelPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Unexpected Error: {ex.Message}");
                Console.WriteLine($"Falling back to CPU for safety.");
                
                return CreateCpuSession(modelPath);
            }
        }

        /// <summary>
        /// Helper to create a CPU-only session.
        /// </summary>
        private static InferenceSession CreateCpuSession(string modelPath)
        {
            try
            {
                // 4. Graceful Degradation
                // CPU execution provider is usually default, but we can be explicit.
                var options = SessionOptions.MakeSessionOptionWithCpuProvider(); 
                Console.WriteLine("✅ CPU Session created successfully.");
                return new InferenceSession(modelPath, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Critical Failure: Could not create CPU session either. {ex.Message}");
                throw; // Re-throw if even CPU fails, as the app cannot run.
            }
        }
    }

    /* 
     * UNIT TEST SIMULATION (Pseudo-code)
     * 
     * To verify the fallback logic, we can simulate the failure path:
     * 
     * public void Test_CUDA_Fallback()
     * {
     *     // Arrange: Rename the hypothetical CUDA DLL to break detection
     *     string cudaDllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "onnxruntime_providers_cuda.dll");
     *     string backupPath = cudaDllPath + ".bak";
     *     
     *     bool renamed = false;
     *     try 
     *     {
     *         if (File.Exists(cudaDllPath))
     *         {
     *             File.Move(cudaDllPath, backupPath);
     *             renamed = true;
     *         }
     * 
     *         // Act: Attempt to create session
     *         // This should trigger DllNotFoundException inside AppendExecutionProvider_CUDA
     *         var session = RobustSessionFactory.CreateSessionWithFallback("model.onnx");
     * 
     *         // Assert
     *         Assert.IsNotNull(session);
     *         // Verify logs indicate "Falling back to CPU"
     *     }
     *     finally 
     *     {
     *         // Cleanup: Restore DLL
     *         if (renamed) File.Move(backupPath, cudaDllPath);
     *     }
     * }
     */
}
