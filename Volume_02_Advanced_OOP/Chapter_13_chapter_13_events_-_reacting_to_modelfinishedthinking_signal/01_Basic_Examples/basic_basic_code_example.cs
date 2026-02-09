
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AI_Data_Structures.Events
{
    // 1. Define Custom Event Arguments
    // We need a class to carry data from the event source (the Model) to the listeners.
    // In a real scenario, this would contain complex Tensor objects. 
    // Here, we simulate it with a string and a status code.
    public class ModelFinishedThinkingEventArgs : EventArgs
    {
        public string InferenceResult { get; set; }
        public int TensorHash { get; set; }
        public DateTime CompletionTime { get; set; }

        public ModelFinishedThinkingEventArgs(string result, int hash)
        {
            InferenceResult = result;
            TensorHash = hash;
            CompletionTime = DateTime.Now;
        }
    }

    // 2. The AI Model Class (The Subject)
    // This class encapsulates the heavy computation. It knows nothing about the UI.
    public class NeuralNetworkModel
    {
        // Define a delegate type that matches the signature of our event handler.
        // It takes an object (sender) and our custom EventArgs.
        public delegate void ModelFinishedThinkingHandler(object sender, ModelFinishedThinkingEventArgs e);

        // The Event declaration using the delegate.
        // 'event' keyword ensures encapsulation; external classes can only += or -=.
        public event ModelFinishedThinkingHandler ModelFinishedThinking;

        // A method simulating a long-running inference task
        public async Task RunInferenceAsync(string inputPrompt)
        {
            Console.WriteLine($"[Model] Processing input: '{inputPrompt}'...");
            
            // Simulate heavy tensor calculation (e.g., matrix multiplication)
            await Task.Delay(2000); 

            // Create the data payload
            var resultPayload = new ModelFinishedThinkingEventArgs(
                result: $"Analysis of '{inputPrompt}' complete. Confidence: 0.98", 
                hash: inputPrompt.GetHashCode()
            );

            // 3. Raising the Event
            // We check for null to ensure there are subscribers.
            // We pass 'this' as the sender (the model itself).
            OnModelFinishedThinking(resultPayload);
        }

        // Helper method to raise the event safely
        protected virtual void OnModelFinishedThinking(ModelFinishedThinkingEventArgs e)
        {
            // Thread safety: Copy the delegate reference to a local variable
            ModelFinishedThinkingHandler handler = ModelFinishedThinking;
            
            if (handler != null)
            {
                // Invoke all subscribed handlers
                handler(this, e);
            }
        }
    }

    // 4. The UI / Listener Class (The Observer)
    // This class reacts to the event. It could be a Dashboard, a Logger, or a Chart.
    public class DashboardDisplay
    {
        public void Subscribe(NeuralNetworkModel model)
        {
            // 5. Using Lambda Expressions (Book 2 Concept)
            // Instead of creating a separate named method, we define the handler inline.
            // This is concise and captures local variables if needed.
            model.ModelFinishedThinking += (sender, e) => 
            {
                // 'sender' is the object that raised the event (the NeuralNetworkModel)
                // 'e' is our ModelFinishedThinkingEventArgs containing the data
                Console.WriteLine($"\n[Dashboard] Event Received!");
                Console.WriteLine($"   > Result: {e.InferenceResult}");
                Console.WriteLine($"   > Tensor Hash: {e.TensorHash}");
                Console.WriteLine($"   > Processed At: {e.CompletionTime:T}");
            };
        }
    }

    // Main Program Execution
    class Program
    {
        static async Task Main(string[] args)
        {
            // Instantiate the components
            var aiModel = new NeuralNetworkModel();
            var dashboard = new DashboardDisplay();

            // Subscribe the dashboard to the model's event
            dashboard.Subscribe(aiModel);

            // Trigger the async inference
            // The Main thread continues immediately, but the event handler 
            // will execute once the background task finishes.
            await aiModel.RunInferenceAsync("Identify tensor shape [4x4]");

            // Keep console open
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
