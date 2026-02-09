
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
using System.Threading;
using System.Threading.Tasks;

namespace AI_Streaming_Delegates
{
    // 1. DEFINING THE DELEGATE
    // This is the "Contract". Any method matching this signature 
    // (void return, takes a string) can be registered as a callback.
    // We use 'delegate' to define a type that represents references to methods.
    public delegate void TokenReceivedHandler(string token);

    public class LLMEngine
    {
        // 2. THE FIELD FOR STORAGE
        // This private field holds the list of subscribers (callbacks).
        // It uses the built-in 'Delegate' class which is multicast-capable.
        private Delegate? _onTokenReceived;

        // 3. REGISTRATION METHOD
        // This allows external code to attach a method to our engine.
        // We restrict the input to our specific delegate type for type safety.
        public void RegisterTokenCallback(TokenReceivedHandler callback)
        {
            // We combine the new callback with any existing ones.
            _onTokenReceived = Delegate.Combine(_onTokenReceived, callback);
        }

        // 4. GENERATION LOGIC (SIMULATED)
        // This simulates the heavy tensor math of an LLM.
        public async Task GenerateResponseAsync(string prompt)
        {
            Console.WriteLine($"\n[Engine]: Processing prompt: '{prompt}'...");
            
            // Simulated tokens that the LLM would output
            string[] tokens = { "Hello", " ", "User", ", ", "I", " am", " an", " AI." };

            foreach (string token in tokens)
            {
                // Simulate network/processing delay
                await Task.Delay(300); 

                // 5. THE INVOCATION
                // Check if a callback exists, then invoke it.
                // This is the "Event-Driven" moment.
                if (_onTokenReceived != null)
                {
                    // Dynamic Invocation: The delegate calls all attached methods.
                    _onTokenReceived.DynamicInvoke(token);
                }
            }
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            // 6. INSTANCE CREATION
            LLMEngine engine = new LLMEngine();

            // 7. LAMBDA EXPRESSION INTRODUCTION
            // Here we define the callback logic inline.
            // The '(string token)' is the parameter list.
            // The '=>' separates parameters from the body.
            // The '{ ... }' is the execution logic.
            TokenReceivedHandler uiUpdater = (string token) => 
            {
                // In a real app, this would append to a TextBlock in WPF/WinUI
                Console.Write(token); 
            };

            // 8. REGISTERING THE LAMBDA
            engine.RegisterTokenCallback(uiUpdater);

            // --- Alternative: Registering directly without a variable ---
            // You can also pass the lambda directly to the registration method:
            /*
            engine.RegisterTokenCallback((token) => {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(token);
                Console.ResetColor();
            });
            */

            // 9. TRIGGERING THE STREAM
            Console.WriteLine("Starting Stream...");
            await engine.GenerateResponseAsync("Tell me a story.");
            Console.WriteLine("\n[Stream Complete]");
        }
    }
}
