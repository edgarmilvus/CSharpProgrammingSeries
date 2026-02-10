
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

using System;
using System.IO;

namespace LocalAI.Inference
{
    // Custom exception for missing models
    public class ModelNotFoundException : Exception
    {
        public ModelNotFoundException(string message) : base(message) { }
    }

    public class ModelVerifier
    {
        public static bool VerifyModelArtifacts(string modelDir)
        {
            // Use Path.Combine for platform-agnostic path construction
            string modelPath = Path.Combine(modelDir, "phi-3-mini.onnx");
            string tokenizerPath = Path.Combine(modelDir, "tokenizer.json");

            if (!File.Exists(modelPath))
            {
                throw new ModelNotFoundException($"Model file not found at: {modelPath}");
            }

            if (!File.Exists(tokenizerPath))
            {
                throw new ModelNotFoundException($"Tokenizer configuration missing at: {tokenizerPath}");
            }

            return true;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Assuming the directory structure described in the prompt
            string modelsRoot = "models/phi-3-mini-onnx";

            try
            {
                if (ModelVerifier.VerifyModelArtifacts(modelsRoot))
                {
                    Console.WriteLine("Model artifacts verified successfully.");
                }
            }
            catch (ModelNotFoundException ex)
            {
                Console.WriteLine($"Verification Failed: {ex.Message}");
            }
        }
    }
}
