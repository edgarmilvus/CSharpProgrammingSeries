
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
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OfflineCodingAssistant
{
    // Core Logic: Simulates the local inference engine (e.g., ONNX Runtime execution)
    public class LocalInferenceEngine
    {
        // Mocks the heavy lifting of running a model like Phi-3 or Llama locally
        public async Task<string> GenerateResponseAsync(string context, string query)
        {
            // Simulate processing delay typical of local CPU/GPU inference
            await Task.Delay(150); 
            
            // A deterministic, rule-based mock response to ensure the output is educational
            // In a real app, this would be: session.Run(inputIds);
            string baseResponse = $"Based on the provided context:\n\nContext:\n\"{context}\"\n\nQuery: {query}\n\nAnalysis: ";
            
            // Simulate token streaming (character by character)
            StringBuilder streamedResponse = new StringBuilder();
            foreach (char c in baseResponse)
            {
                streamedResponse.Append(c);
                // In a real console app, we might use Console.Write to avoid newlines
                Console.Write(c); 
                await Task.Delay(20); // Simulate token generation speed
            }
            
            // Add specific analysis based on the mock context
            if (context.Contains("public"))
            {
                string analysis = "This method is publicly accessible. Ensure input validation is strict.";
                foreach (char c in analysis)
                {
                    streamedResponse.Append(c);
                    Console.Write(c);
                    await Task.Delay(20);
                }
            }

            Console.WriteLine(); // Newline after streaming finishes
            return streamedResponse.ToString();
        }
    }

    // Data Management: Handles reading and parsing local source files (RAG Simulation)
    public class SourceCodeManager
    {
        private string _projectDirectory;

        public SourceCodeManager(string projectDirectory)
        {
            _projectDirectory = projectDirectory;
        }

        // Scans directory for C# files and extracts method signatures
        public List<string> ExtractMethodSignatures()
        {
            List<string> signatures = new List<string>();

            if (!Directory.Exists(_projectDirectory))
            {
                Console.WriteLine($"[!] Directory not found: {_projectDirectory}");
                return signatures;
            }

            string[] files = Directory.GetFiles(_projectDirectory, "*.cs");
            
            foreach (string file in files)
            {
                try
                {
                    string[] lines = File.ReadAllLines(file);
                    foreach (string line in lines)
                    {
                        // Simple heuristic: look for lines containing "public" or "private" and ending with ")"
                        // This simulates the "Chunking" and "Embedding" phase of RAG
                        if ((line.Contains("public") || line.Contains("private")) && line.Contains("(") && line.Contains(")"))
                        {
                            // Clean up the line for display
                            string cleanLine = line.Trim();
                            if (cleanLine.Length > 0)
                            {
                                signatures.Add(cleanLine);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[!] Error reading file {file}: {ex.Message}");
                }
            }

            return signatures;
        }
    }

    // UI Layer: Handles user input and orchestration
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("--- Offline Coding Assistant v1.0 ---");
            Console.WriteLine("Initializing Local Inference Engine...");

            // Initialize components
            var inferenceEngine = new LocalInferenceEngine();
            
            // Point to the current directory for demonstration
            var codeManager = new SourceCodeManager(Directory.GetCurrentDirectory());

            Console.WriteLine("Scanning local source files...");
            var methods = codeManager.ExtractMethodSignatures();

            if (methods.Count == 0)
            {
                Console.WriteLine("No methods found in current directory.");
                return;
            }

            Console.WriteLine($"Found {methods.Count} method signatures.");
            
            // Build the context string (Retrieval step)
            StringBuilder contextBuilder = new StringBuilder();
            foreach (var method in methods)
            {
                contextBuilder.AppendLine($"- {method}");
            }
            string context = contextBuilder.ToString();

            // User Interaction Loop
            while (true)
            {
                Console.Write("\n[You]: ");
                string query = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(query)) continue;
                if (query.ToLower() == "exit") break;

                Console.Write("[Assistant]: ");
                
                // Execute Inference
                try 
                {
                    await inferenceEngine.GenerateResponseAsync(context, query);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during inference: {ex.Message}");
                }
            }
        }
    }
}
