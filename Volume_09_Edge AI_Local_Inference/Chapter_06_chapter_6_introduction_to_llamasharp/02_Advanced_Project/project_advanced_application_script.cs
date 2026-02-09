
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using LLama;
using LLama.Common;
using LLama.Native;

/*
 * REAL-WORLD PROBLEM: Smart Home IoT Log Analysis
 * 
 * Context: A smart home system generates thousands of log entries daily from various sensors 
 * (motion, temperature, door locks, cameras). A security analyst needs to quickly identify 
 * anomalies and potential security threats (e.g., "Door forced open at 3 AM").
 * 
 * Solution: This application runs a local LLM (Llama/Phi) to perform semantic analysis 
 * on log streams. It categorizes logs into "Normal", "Warning", or "Critical" using 
 * natural language understanding, without sending sensitive data to the cloud.
 */

namespace EdgeAI_SmartHomeAnalyzer
{
    // 1. DATA MODEL: Represents a single IoT log entry
    public class LogEntry
    {
        public string Timestamp { get; set; }
        public string DeviceId { get; set; }
        public string Message { get; set; }
    }

    // 2. ANALYSIS RESULT: Stores the LLM's verdict
    public class AnalysisResult
    {
        public LogEntry OriginalLog { get; set; }
        public string Category { get; set; } // Normal, Warning, Critical
        public string Explanation { get; set; }
        public double ConfidenceScore { get; set; } // Simulated confidence
    }

    // 3. CORE ENGINE: Manages LLM Inference and Logic
    public class SmartHomeAnalyzerEngine
    {
        private readonly LLamaWeights _model;
        private readonly ModelParams _params;
        private readonly InferenceParams _inferenceParams;

        // Constructor: Loads the model into memory
        public SmartHomeAnalyzerEngine(string modelPath)
        {
            // Validate model file existence
            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException($"Model file not found at: {modelPath}. Please download a GGUF model (e.g., Phi-3-mini).");
            }

            // Configure backend parameters (CPU/GPU, Threads)
            // Note: We use a simple configuration suitable for edge devices
            _params = new ModelParams(modelPath)
            {
                // Use fewer threads for low-power devices, or GPU if available
                Threads = 4, 
                GpuLayerCount = 0 // 0 for CPU only, set to >0 if GPU offloading is desired
            };

            // Load the model weights
            _model = LLamaWeights.LoadFromFile(_params);

            // Configure Inference Parameters (Temperature, Max Tokens, etc.)
            _inferenceParams = new InferenceParams()
            {
                Temperature = 0.1f, // Low temp for deterministic classification
                MaxTokens = 100,     // Keep responses short
                AntiPrompts = new List<string> { "\n", "User:", "Assistant:" } // Stop generation
            };
        }

        // 4. INFERENCE LOGIC: Analyzes a single log entry
        public async Task<AnalysisResult> AnalyzeLogAsync(LogEntry log)
        {
            // Construct a precise prompt for the LLM
            // We use a system prompt to define the persona and rules
            string systemPrompt = "You are a security AI for a smart home. Classify the log entry strictly as 'Normal', 'Warning', or 'Critical'. Provide a short reason.";
            string userPrompt = $"Log: {log.Message}";
            
            // Combine into a conversation format
            string fullPrompt = $"{systemPrompt}\nUser: {userPrompt}\nAssistant:";

            // Create an execution context for this specific inference
            // Context is created per request to manage memory efficiently on edge devices
            using var context = _model.CreateExecutionContext(_params);
            
            // Tokenize the prompt
            // We use native tokenization for performance
            var tokens = context.Tokenize(fullPrompt, addBos: true);
            
            // Generate the response stream
            var sb = new StringBuilder();
            await foreach (var token in context.Generate(tokens, _inferenceParams))
            {
                // Decode token to string
                string tokenText = context.Tokenize(token, decode: true);
                sb.Append(tokenText);
            }

            string llmResponse = sb.ToString().Trim();

            // 5. POST-PROCESSING: Parse LLM output
            return ParseLlmResponse(log, llmResponse);
        }

        private AnalysisResult ParseLlmResponse(LogEntry log, string response)
        {
            // Basic parsing logic (avoiding complex Regex/LINQ as per constraints)
            string category = "Unknown";
            string explanation = response;
            double confidence = 0.5;

            // Simple string matching to extract category
            if (response.Contains("Critical")) 
            { 
                category = "Critical"; 
                confidence = 0.95; 
            }
            else if (response.Contains("Warning")) 
            { 
                category = "Warning"; 
                confidence = 0.85; 
            }
            else if (response.Contains("Normal")) 
            { 
                category = "Normal"; 
                confidence = 0.90; 
            }

            return new AnalysisResult
            {
                OriginalLog = log,
                Category = category,
                Explanation = explanation,
                ConfidenceScore = confidence
            };
        }
    }

    // 6. MAIN PROGRAM: Orchestrates the application
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Edge AI: Smart Home Log Analyzer ===");
            Console.WriteLine("Requires a GGUF model file (e.g., Phi-3-mini-Q4.gguf)");
            
            // Path to model (User must provide this)
            // In a real scenario, this would be downloaded automatically or configured
            string modelPath = "models/phi-3-mini-4k-instruct-q4.gguf"; 
            
            // Fallback for demo purposes if file doesn't exist
            if (!File.Exists(modelPath))
            {
                Console.WriteLine($"[!] Model not found: {modelPath}");
                Console.WriteLine("Please download a GGUF model and place it in the 'models' folder.");
                Console.WriteLine("Example: https://huggingface.co/microsoft/Phi-3-mini-4k-instruct-gguf");
                
                // Simulate output for the sake of the chapter example if model is missing
                SimulateDemo();
                return;
            }

            try
            {
                // Initialize the Engine
                var engine = new SmartHomeAnalyzerEngine(modelPath);

                // Simulate incoming IoT Log Stream
                var logs = new LogEntry[]
                {
                    new LogEntry { Timestamp = "2023-10-27 09:15:00", DeviceId = "LIVING_ROOM_MOTION", Message = "Motion detected while system armed" },
                    new LogEntry { Timestamp = "2023-10-27 03:02:15", DeviceId = "BACK_DOOR_LOCK", Message = "Door unlocked manually" },
                    new LogEntry { Timestamp = "2023-10-27 03:02:16", DeviceId = "BACK_DOOR_LOCK", Message = "Door forced open detected" }
                };

                Console.WriteLine("\nProcessing Log Stream...\n");

                // Process logs sequentially (efficient for edge CPU)
                foreach (var log in logs)
                {
                    Console.WriteLine($"[Input] {log.Timestamp} - {log.Message}");
                    
                    // Perform AI Analysis
                    var result = await engine.AnalyzeLogAsync(log);

                    // Output Result
                    Console.ForegroundColor = GetColor(result.Category);
                    Console.WriteLine($"[AI Verdict] {result.Category} (Conf: {result.ConfidenceScore:P0})");
                    Console.ResetColor();
                    Console.WriteLine($"[Reason] {result.Explanation}\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Ensure you have the LLamaSharp and LLamaSharp.Backend.Cpu NuGet packages installed.");
            }
        }

        // Helper to colorize output based on severity
        static ConsoleColor GetColor(string category)
        {
            switch (category)
            {
                case "Critical": return ConsoleColor.Red;
                case "Warning": return ConsoleColor.Yellow;
                case "Normal": return ConsoleColor.Green;
                default: return ConsoleColor.Gray;
            }
        }

        // Simulation for demonstration purposes if no model is available
        static void SimulateDemo()
        {
            Console.WriteLine("\n--- DEMO MODE (No Model Loaded) ---");
            Console.WriteLine("Simulating LLM inference results...\n");
            
            var mockLogs = new[]
            {
                ("Motion detected while system armed", "Normal"),
                ("Door unlocked manually", "Warning"),
                ("Door forced open detected", "Critical")
            };

            foreach (var (msg, cat) in mockLogs)
            {
                Console.WriteLine($"[Input] {msg}");
                Console.ForegroundColor = GetColor(cat);
                Console.WriteLine($"[AI Verdict] {cat}");
                Console.ResetColor();
                Console.WriteLine("[Reason] Based on context, this matches the definition of " + cat + ".\n");
            }
        }
    }
}
