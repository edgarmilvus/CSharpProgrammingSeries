
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

# Source File: solution_exercise_9.cs
# Description: Solution for Exercise 9
# ==========================================

using System;
using System.IO;
using Microsoft.ML.OnnxRuntime;

namespace LocalAI.Inference
{
    public class RobustModelLoader
    {
        public static InferenceSession? LoadModelSafely(string modelPath)
        {
            try
            {
                return new InferenceSession(modelPath);
            }
            catch (OnnxRuntimeException ex)
            {
                // Specific ONNX errors
                string msg = ex.Message;
                string userMessage = "An unknown ONNX error occurred.";

                if (msg.Contains("invalid proto") || msg.Contains("opset"))
                {
                    userMessage = "The model file appears corrupted or incompatible. Please re-download the model from Hugging Face.";
                }
                else if (msg.Contains("kernel") || msg.Contains("operator"))
                {
                    userMessage = "The model uses unsupported operators. Check ONNX Runtime version.";
                }

                Console.WriteLine($"[ONNX Error] {userMessage}");
                LogError(ex);
            }
            catch (DllNotFoundException ex)
            {
                Console.WriteLine("[Dependency Error] Native ONNX Runtime library not found. Check your PATH.");
                LogError(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[General Error] {ex.Message}");
                LogError(ex);
            }

            return null;
        }

        private static void LogError(Exception ex)
        {
            // Append to error log
            string logContent = $"[{DateTime.Now}] {ex.GetType().Name}: {ex.Message}\nStack: {ex.StackTrace}\n\n";
            File.AppendAllText("error_log.txt", logContent);
            Console.WriteLine("Details written to error_log.txt");
        }
    }
}
