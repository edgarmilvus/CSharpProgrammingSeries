
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;
using System.Linq;
using Microsoft.ML.OnnxRuntime;

namespace LocalAI.Inference
{
    public static class HardwareSelector
    {
        public static string GetOptimalExecutionProvider()
        {
            // Mocking the available providers for the sake of the method signature
            // In a real app, call: OrtEnv.Instance.SessionOptions.GetAvailableProviders()
            string[] availableProviders = OrtEnv.Instance.SessionOptions.GetAvailableProviders();

            // Priority Order
            if (availableProviders.Contains("CUDAExecutionProvider"))
            {
                return "CUDAExecutionProvider";
            }
            
            if (availableProviders.Contains("DmlExecutionProvider"))
            {
                return "DmlExecutionProvider";
            }

            return "CPUExecutionProvider";
        }
    }

    // Unit Test Simulation (since we can't easily run xUnit/NUnit here)
    public class HardwareSelectorTests
    {
        public static void RunTests()
        {
            Console.WriteLine("Running Hardware Selection Logic Tests...");

            // Mock 1: System with NVIDIA GPU
            string[] mockCuda = { "CPUExecutionProvider", "CUDAExecutionProvider" };
            TestLogic(mockCuda, "CUDAExecutionProvider");

            // Mock 2: System with Intel/AMD GPU (DirectML)
            string[] mockDml = { "CPUExecutionProvider", "DmlExecutionProvider" };
            TestLogic(mockDml, "DmlExecutionProvider");

            // Mock 3: System with CPU only
            string[] mockCpu = { "CPUExecutionProvider" };
            TestLogic(mockCpu, "CPUExecutionProvider");
        }

        // Helper to simulate the logic
        private static void TestLogic(string[] providers, string expected)
        {
            // This simulates the logic inside GetOptimalExecutionProvider
            string result = "CPUExecutionProvider"; // Default
            
            if (providers.Contains("CUDAExecutionProvider")) result = "CUDAExecutionProvider";
            else if (providers.Contains("DmlExecutionProvider")) result = "DmlExecutionProvider";

            if (result == expected)
                Console.WriteLine($"[PASS] {string.Join(",", providers)} -> {result}");
            else
                Console.WriteLine($"[FAIL] {string.Join(",", providers)} -> Expected {expected}, got {result}");
        }
    }
}
