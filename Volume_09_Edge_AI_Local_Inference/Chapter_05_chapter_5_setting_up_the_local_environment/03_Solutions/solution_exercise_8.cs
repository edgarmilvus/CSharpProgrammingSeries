
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

// Source File: solution_exercise_8.cs
// Description: Solution for Exercise 8
// ==========================================

using System;
using System.Threading.Tasks;

namespace LocalAI.Inference
{
    class ChatProgram
    {
        // Mocks for the exercise
        static int[] MockTokenizer(string input) => [1, 2, 3, input.Length]; 
        static string MockDetokenizer(int[] ids) => "Simulated response for: " + string.Join(" ", ids);

        static async Task Main(string[] args)
        {
            Console.WriteLine("LocalAI Chat Bot (Phi-3 Mini ONNX)");
            Console.WriteLine("Type 'exit' to quit.");

            // 1. Load Model Configuration
            var config = new ModelConfig(
                ModelPath: "models/phi-3-mini-onnx/phi-3-mini.onnx",
                ExecutionProvider: "CUDAExecutionProvider", // Will fallback via ModelLoader logic
                EnableGraphOptimization: true
            );

            // Pre-load the session (assuming file exists for this demo)
            // In a real app, wrap this in a try-catch for file not found.
            InferenceSession session;
            try 
            {
                session = await ModelLoader.LoadAsync(config);
            }
            catch
            {
                Console.WriteLine("Model loading failed. Exiting.");
                return;
            }

            // 2. The Chat Loop
            while (true)
            {
                Console.Write("\nUser: ");
                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                // 3. & 4. Tokenize & Run Inference (Wrapped in LLMWrapper for disposal logic)
                // Note: We are reusing the session here, but creating new OrtValues per turn.
                using (var llm = new LLMWrapper("dummy_path")) // Reusing wrapper logic, but passing session directly in real code
                {
                    // Simulating the flow:
                    var tokenIds = MockTokenizer(input);
                    
                    // We need to manually handle OrtValue disposal here if not using the wrapper's method
                    // But let's use the Wrapper's RunInference method from Ex 4 for safety.
                    // Since LLMWrapper creates its own session, let's adapt:
                    
                    // ACTUAL INTEGRATION PATTERN:
                    // 1. Tokenize -> int[]
                    // 2. Create OrtValue (using) -> Input
                    // 3. Session.Run(Input) -> Output
                    // 4. Parse Output
                    // 5. Dispose Input/Output
                    // 6. Detokenize

                    // Using the OutputParser logic from Ex 7 and DynamicTensor from Ex 6
                    using var inputTensor = DynamicShapeConfigurator.CreateDynamicTensor(tokenIds);
                    
                    var inputNames = new System.Collections.Generic.List<string> { "input_ids" };
                    var outputNames = new System.Collections.Generic.List<string> { "logits" };
                    var inputs = new System.Collections.Generic.List<OrtValue> { inputTensor };

                    // Run Inference
                    using var outputs = session.Run(inputs, inputNames, outputNames);
                    
                    // Parse (Mocking the output processing for brevity)
                    // OutputParser.ParseLogits(outputs[0]); // Uncomment if model outputs valid logits

                    // Detokenize
                    string response = MockDetokenizer(tokenIds);
                    Console.WriteLine($"Bot: {response}");
                } // inputTensor is disposed here
            }

            // Cleanup
            session.Dispose();
            Console.WriteLine("Chat session ended.");
        }
    }
}
