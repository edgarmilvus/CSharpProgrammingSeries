
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace LocalInferencePrivacyDemo
{
    class Program
    {
        // Configuration: Simulating a local model file path (e.g., a Phi-3 ONNX model)
        // In a real scenario, this would be a quantized ONNX model file.
        const string ModelPath = "models/phi-3-mini-4k-instruct-onnx/cpu_and_mobile/cpu-int4-rtn-block-32-acc-level-4";
        
        // Simulated sensitive data (e.g., medical notes or financial logs) that must NOT leave the device.
        static string[] sensitiveData = new string[]
        {
            "Patient ID: 4592. Diagnosis: Hypertension. Blood Pressure: 145/90. Medication: Lisinopril.",
            "Transaction ID: TX-9982. Amount: $5,400.00. Vendor: 'Global Imports'. Flag: High Risk.",
            "Employee: Jane Doe. SSN: 123-45-6789. Performance Review: Exceeds Expectations."
        };

        static void Main(string[] args)
        {
            Console.WriteLine("--- Local Edge AI Inference Demo: Privacy-First Processing ---");
            Console.WriteLine($"Context: Running inference on device. Model Path: {ModelPath}");
            Console.WriteLine("Objective: Process sensitive text without sending data to the cloud.\n");

            // 1. Validate Environment
            // Check if the "model" exists (simulated file check).
            if (!ValidateModelPresence(ModelPath))
            {
                Console.WriteLine("Error: Model file not found. Local inference requires local assets.");
                return;
            }

            // 2. Initialize Inference Engine
            // In a real app, this loads the ONNX Runtime session.
            // Here, we simulate the initialization overhead.
            var inferenceEngine = new LocalInferenceEngine();
            if (!inferenceEngine.Initialize())
            {
                Console.WriteLine("Error: Failed to initialize local inference engine (Hardware check failed).");
                return;
            }

            // 3. Process Data Loop
            // Iterate through sensitive data. 
            // CRITICAL: No data leaves this loop scope.
            for (int i = 0; i < sensitiveData.Length; i++)
            {
                ProcessSensitiveData(inferenceEngine, sensitiveData[i], i + 1);
            }

            Console.WriteLine("\n--- Batch Processing Complete ---");
            Console.WriteLine("Summary: All data processed locally. No network transmission occurred.");
        }

        /// <summary>
        /// Validates if the model file exists on the local disk.
        /// This emphasizes the "Infrastructure Cost" aspect: managing local storage.
        /// </summary>
        static bool ValidateModelPresence(string path)
        {
            // Simulating a file existence check. 
            // In real code: File.Exists(path);
            Console.WriteLine($"[Check] Verifying model assets at: {path}");
            
            // Simulate existence for the demo (assuming the folder structure is valid conceptually)
            // In a real app, this prevents runtime crashes due to missing dependencies.
            return true; 
        }

        /// <summary>
        /// Encapsulates the logic for handling a single piece of sensitive data.
        /// </summary>
        static void ProcessSensitiveData(LocalInferenceEngine engine, string text, int id)
        {
            Console.WriteLine($"\n--- Processing Item #{id} ---");
            Console.WriteLine($"Input (Local Only): \"{text}\"");

            // Stopwatch to measure latency (Edge AI Pillar: Latency)
            var sw = Stopwatch.StartNew();

            // Execute Inference
            // We ask the model to classify the text type (e.g., Medical, Financial, Personnel)
            // This simulates a local RAG (Retrieval-Augmented Generation) or classification step.
            string prompt = $"Classify the category of this text: {text}";
            string result = engine.RunInference(prompt);

            sw.Stop();

            // Output Result
            Console.WriteLine($"Result: {result}");
            Console.WriteLine($"Latency: {sw.ElapsedMilliseconds}ms (Local Execution)");
            
            // Privacy Check
            Console.WriteLine("Privacy Status: Data never left the device memory.");
        }
    }

    /// <summary>
    /// Simulates the Local Inference Engine wrapper.
    /// In a real implementation, this would wrap the Microsoft.ML.OnnxRuntime NuGet package.
    /// </summary>
    class LocalInferenceEngine
    {
        private bool _isInitialized = false;

        public bool Initialize()
        {
            Console.WriteLine("[System] Loading ONNX Runtime...");
            
            // Simulate hardware acceleration check (CPU/GPU)
            // Real logic checks for CUDA, DirectML, or CoreML availability.
            bool hardwareAccelerated = CheckHardwareCapabilities();
            
            if (hardwareAccelerated)
            {
                Console.WriteLine("[System] Hardware acceleration detected (e.g., GPU).");
            }
            else
            {
                Console.WriteLine("[System] Running on CPU (slower inference, but higher compatibility).");
            }

            // Simulate model loading latency (often 2-5 seconds for 3B+ parameter models)
            System.Threading.Thread.Sleep(1000); 
            
            _isInitialized = true;
            return true;
        }

        private bool CheckHardwareCapabilities()
        {
            // In a real app, this checks:
            // 1. GPU VRAM availability
            // 2. Supported execution providers (CUDA, TensorRT, CoreML)
            // For this demo, we assume CPU execution for maximum portability.
            return false; 
        }

        public string RunInference(string prompt)
        {
            if (!_isInitialized) throw new InvalidOperationException("Engine not initialized.");

            // SIMULATION LOGIC:
            // Real inference involves:
            // 1. Tokenization (converting text to numbers)
            // 2. Matrix Multiplication (The heavy math)
            // 3. Detokenization (converting numbers back to text)
            
            // We simulate the output based on the prompt content to demonstrate the logic flow.
            if (prompt.Contains("Medical") || prompt.Contains("Patient") || prompt.Contains("Blood Pressure"))
            {
                return "Category: Medical Record (Confidence: 98%)";
            }
            else if (prompt.Contains("Transaction") || prompt.Contains("Amount") || prompt.Contains("Risk"))
            {
                return "Category: Financial Log (Confidence: 95%)";
            }
            else if (prompt.Contains("Employee") || prompt.Contains("SSN"))
            {
                return "Category: HR Data (Confidence: 99%)";
            }
            
            return "Category: General Text";
        }
    }
}
