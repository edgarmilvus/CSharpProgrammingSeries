
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

// Source File: solution_exercise_18.cs
// Description: Solution for Exercise 18
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public class SimulatedTask
{
    public string Description { get; set; } = "";
    public int DueHour { get; set; } // Simulated hour from start
}

async Task RunTemporalAgentAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var reminderAgent = new ChatCompletionAgent(kernel, "Generate a polite reminder for a task.");
    var escalationAgent = new ChatCompletionAgent(kernel, "Generate an urgent escalation message.");

    var tasks = new List<SimulatedTask> {
        new SimulatedTask { Description = "Submit Report", DueHour = 25 }, // Due after 24h
        new SimulatedTask { Description = "Fix Bug", DueHour = 10 }        // Due soon
    };

    // Simulation Loop (0 to 48 hours)
    for (int currentHour = 0; currentHour <= 48; currentHour += 12)
    {
        Console.WriteLine($"\n--- Simulated Time: {currentHour}:00 ---");
        
        foreach (var task in tasks)
        {
            int hoursLeft = task.DueHour - currentHour;

            if (hoursLeft <= 0)
            {
                // Overdue
                var msg = await escalationAgent.InvokeAsync($"Task: {task.Description} is overdue.");
                Console.WriteLine($"[Escalation]: {msg}");
            }
            else if (hoursLeft <= 24)
            {
                // Due within 24 hours
                var msg = await reminderAgent.InvokeAsync($"Task: {task.Description} is due in {hoursLeft} hours.");
                Console.WriteLine($"[Reminder]: {msg}");
            }
        }
    }
}
