
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Text.Json;

// 1. Setup: Define the data structure for our task
public class ShoppingTask
{
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public bool IsComplete => !string.IsNullOrEmpty(ItemName) && Quantity > 0;

    public override string ToString() => $"Task: Buy {Quantity}x {ItemName} [{(IsComplete ? "Ready" : "Pending")}]";
}

// 2. The "Tools" (Skills) the Agent can use to interact with the world
public class TaskManagerPlugin
{
    [KernelFunction("update_task")]
    [Description("Updates the current shopping task with specific details.")]
    public ShoppingTask UpdateTask(
        [Description("The name of the item to buy")] string itemName,
        [Description("The quantity of the item")] int quantity)
    {
        Console.WriteLine($"[System]: Updating task details...");
        return new ShoppingTask { ItemName = itemName, Quantity = quantity };
    }

    [KernelFunction("finalize_task")]
    [Description("Marks the task as complete and ready for execution.")]
    public string FinalizeTask(ShoppingTask task)
    {
        if (!task.IsComplete)
            throw new InvalidOperationException("Task is not ready to be finalized.");
        
        return $"✅ Finalized: {task}";
    }
}

// 3. The Main Execution Loop
public class Program
{
    public static async Task Main()
    {
        // --- Configuration ---
        // NOTE: In a real scenario, replace with your actual Azure OpenAI or OpenAI key.
        // For this demo, we use a fake client to ensure the code runs without external keys.
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: "gpt-4", 
                apiKey: "fake-key-for-demo") 
            .Build();

        // Register our plugin
        var plugin = new TaskManagerPlugin();
        kernel.Plugins.AddFromObject(plugin, "TaskManager");

        // Initial User Request
        string userRequest = "I need to buy milk.";
        
        // The "Memory" of the agent (Current State)
        ShoppingTask currentTask = new ShoppingTask(); 

        // --- The Autonomous Loop ---
        Console.WriteLine($"🤖 Agent Loop Started. User Request: \"{userRequest}\"\n");

        int maxIterations = 5; // Safety break to prevent infinite loops
        int iteration = 0;

        while (iteration < maxIterations && !currentTask.IsComplete)
        {
            iteration++;
            Console.WriteLine($"--- Iteration {iteration} ---");

            // A. Construct the prompt dynamically based on current state
            string prompt = $"""
                Analyze the user request: "{userRequest}".
                
                Current Task State:
                {JsonSerializer.Serialize(currentTask)}

                Instructions:
                1. If the ItemName is missing, identify it from the request or ask the user.
                2. If the Quantity is missing, ask the user for it.
                3. Use the 'update_task' function ONLY when you have both name and quantity.
                4. If the task is complete, use 'finalize_task'.
                5. If you need to ask the user a question, output the question directly.
                """;

            // B. Invoke the Kernel (Reasoning Step)
            var result = await kernel.InvokePromptAsync(prompt);

            // C. Parse the result to update state or interact
            // In a real app, the LLM might return a JSON object or a natural language string.
            // Here, we simulate the LLM's behavior for the sake of the "Hello World" example.
            // *Note: In a production environment, the Kernel handles function calling automatically.*
            
            // Simulating the LLM's decision logic for this standalone example:
            // (Since we don't have a real LLM connected to execute the function calls automatically 
            // in this specific code snippet structure without complex orchestration setup)
            
            if (string.IsNullOrEmpty(currentTask.ItemName))
            {
                // Simulated LLM reasoning: "Item name is missing from state, extract it."
                currentTask.ItemName = "milk"; // Extracted from userRequest
                Console.WriteLine($"🤖 Agent: Detected item is '{currentTask.ItemName}'.");
            }
            else if (currentTask.Quantity == 0)
            {
                // Simulated LLM reasoning: "Quantity is missing. I need to ask the user."
                Console.WriteLine($"🤖 Agent: Quantity is unknown. Asking user...");
                
                // Simulate user response
                Console.Write("👤 User (simulated input): ");
                string simulatedUserResponse = "2 gallons"; // Simulating user typing
                Console.WriteLine(simulatedUserResponse);

                // Parse user response for quantity
                if (int.TryParse(new string(simulatedUserResponse.Where(char.IsDigit).ToArray()), out int qty))
                {
                    currentTask.Quantity = qty;
                    Console.WriteLine($"🤖 Agent: Updated quantity to {qty}.");
                }
            }

            // D. Check Termination Condition
            if (currentTask.IsComplete)
            {
                Console.WriteLine($"🤖 Agent: Task is complete. Finalizing...");
                
                // Call the final function
                var finalResult = plugin.FinalizeTask(currentTask);
                Console.WriteLine(finalResult);
                break;
            }
            else
            {
                Console.WriteLine($"🤖 Agent: State updated. Looping again...");
            }
        }

        if (iteration >= maxIterations)
        {
            Console.WriteLine("⚠️ Loop terminated due to max iterations reached.");
        }
    }
}
