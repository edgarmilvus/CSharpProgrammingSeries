
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
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;

namespace SmartHomeOrchestrator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. CONFIGURATION & KERNEL INITIALIZATION
            // In a real application, keys come from secure configuration (e.g., Azure Key Vault).
            // For this example, we assume they are set in environment variables.
            string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            string bingKey = Environment.GetEnvironmentVariable("BING_SEARCH_KEY");

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(bingKey))
            {
                Console.WriteLine("Error: Please set OPENAI_API_KEY and BING_SEARCH_KEY environment variables.");
                return;
            }

            // Initialize the Kernel. This is the central orchestrator.
            // We use the OpenAI connector for LLM capabilities.
            var builder = new KernelBuilder();
            builder.WithOpenAIChatCompletionService("gpt-4", apiKey);
            
            // Add Web Search capabilities using Bing.
            builder.WithLoggingService(); // Essential for debugging SK internals.
            var kernel = builder.Build();

            // 2. PLUG-IN REGISTRATION
            // We register native .NET classes as "Plugins" so the LLM can call them.
            // The LLM doesn't execute code directly; it requests the Kernel to invoke these functions.
            var smartHomePlugin = new SmartHomeControlPlugin();
            kernel.ImportFunctions(smartHomePlugin, "Home");

            // Register the Bing Search connector as a native plugin for external data.
            // This allows the AI to fetch real-time data (e.g., weather).
            var webSearch = new BingConnector(bingKey);
            kernel.ImportFunctions(webSearch, "Web");

            // 3. PROMPT ENGINEERING & EXECUTION
            // We define a complex user request that requires orchestration across multiple plugins.
            string userRequest = "Analyze the current weather in Seattle. If it's raining, turn on the living room lights and set the thermostat to 72 degrees. If it's sunny, turn off the lights.";

            Console.WriteLine($"User Request: \"{userRequest}\"\n");
            Console.WriteLine("Orchestrating Kernel Execution...\n");

            // 4. THE AGENTIC LOOP (Simulated via Sequential Execution)
            // In Chapter 1, we introduce the concept of the Kernel running a prompt.
            // Here we simulate a "Planner" by breaking down the request into logical steps 
            // the Kernel must resolve using its available functions.
            
            // Step A: Determine the weather (using Web Search)
            string weatherQuery = "Current weather in Seattle";
            Console.WriteLine($"[Step 1] Querying Web for: {weatherQuery}");
            
            // The Kernel invokes the Web Search function
            var weatherResult = await kernel.InvokeAsync("Web", "Search", new KernelArguments { ["query"] = weatherQuery });
            
            // We parse the result (basic string manipulation as per constraints)
            string weatherContext = weatherResult.ToString();
            bool isRaining = weatherContext.Contains("rain", StringComparison.OrdinalIgnoreCase) || 
                             weatherContext.Contains("storm", StringComparison.OrdinalIgnoreCase);
            
            Console.WriteLine($"[Result] Weather Analysis: {(isRaining ? "Raining" : "Not Raining (Sunny/Other)"}\n");

            // Step B: Execute Logic based on Analysis (The "Planner" logic)
            // This simulates the dynamic chaining of functions based on context.
            if (isRaining)
            {
                Console.WriteLine("[Step 2] Condition Met: Raining. Executing 'Cozy Mode'...");

                // Execute: Turn on lights
                var lightResult = await kernel.InvokeAsync("Home", "SetLights", new KernelArguments { ["room"] = "Living Room", ["state"] = "On" });
                Console.WriteLine($"  -> {lightResult}");

                // Execute: Set Thermostat
                var tempResult = await kernel.InvokeAsync("Home", "SetThermostat", new KernelArguments { ["temperature"] = "72" });
                Console.WriteLine($"  -> {tempResult}");
            }
            else
            {
                Console.WriteLine("[Step 2] Condition Met: Not Raining. Executing 'Energy Saving Mode'...");

                // Execute: Turn off lights
                var lightResult = await kernel.InvokeAsync("Home", "SetLights", new KernelArguments { ["room"] = "Living Room", ["state"] = "Off" });
                Console.WriteLine($"  -> {lightResult}");
            }

            Console.WriteLine("\nOrchestration Complete.");
        }
    }

    // NATIVE .NET PLUGIN CLASS
    // This class represents a modular capability (Plugin) introduced in Chapter 1.
    // It uses standard C# methods. The attributes (FunctionName) help the Kernel identify them.
    public class SmartHomeControlPlugin
    {
        [KernelFunction("SetLights")]
        public string SetLights(string room, string state)
        {
            // Basic validation logic
            if (string.IsNullOrEmpty(room) || string.IsNullOrEmpty(state))
            {
                return "Error: Room and State are required.";
            }

            // Simulate hardware interaction
            return $"Lights in {room} turned {state.ToUpper()}.";
        }

        [KernelFunction("SetThermostat")]
        public string SetThermostat(string temperature)
        {
            // Basic validation logic
            if (!int.TryParse(temperature, out int temp))
            {
                return "Error: Temperature must be a number.";
            }

            // Simulate hardware interaction
            return $"Thermostat set to {temp}°F.";
        }
    }
}
