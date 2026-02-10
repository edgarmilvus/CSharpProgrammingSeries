
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ScheduleManager
{
    [KernelFunction("GetCalendarEvents")]
    public async Task<List<string>> GetCalendarEventsAsync(DateTime date)
    {
        Console.WriteLine($"[Skill] Checking calendar for {date:yyyy-MM-dd}...");
        // Mock implementation: Simulate finding a meeting
        return new List<string> { "Meeting with Design Team" };
    }

    [KernelFunction("SendEmail")]
    public async Task SendEmailAsync(string recipient, string subject, string body)
    {
        Console.WriteLine($"[Skill] Email sent to {recipient}: {subject}");
        Console.WriteLine($"[Skill] Body: {body}");
    }

    [KernelFunction("GetEventOrganizer")]
    public async Task<string> GetEventOrganizerAsync(string eventTitle)
    {
        Console.WriteLine($"[Skill] Fetching organizer for: {eventTitle}");
        return "organizer@example.com";
    }
}

public class ZeroShotPlannerExample
{
    public static async Task RunAsync()
    {
        // 1. Initialize Kernel
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            modelId: "gpt-4o-mini", 
            apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "fake-key-for-demo"
        );
        
        // 2. Register Native Skills
        var kernel = builder.Build();
        var scheduleManager = new ScheduleManager();
        kernel.ImportPluginFromObject(scheduleManager, "Schedule");

        // 3. Configure Planner
        // Note: SequentialPlanner is used here as it handles conditional logic via function descriptions.
        var planner = new SequentialPlanner(new SequentialPlannerOptions
        {
            // Allow the planner to iterate or handle conditional logic implicitly 
            // by understanding the goal description.
            RelevancyThreshold = 0.5 
        });

        // 4. Define Goal
        string goal = "Check my calendar for next Tuesday. If there is a meeting, send a reminder email to the organizer.";

        Console.WriteLine($"Goal: {goal}\n");

        try
        {
            // 5. Generate Plan
            var plan = await planner.CreatePlanAsync(kernel, goal);
            
            Console.WriteLine("Generated Plan Steps:");
            foreach (var step in plan.Steps)
            {
                Console.WriteLine($"- {step.Name}: {step.Description}");
            }
            Console.WriteLine();

            // 6. Execute Plan
            Console.WriteLine("Executing Plan...");
            var result = await plan.InvokeAsync(kernel);

            Console.WriteLine($"\nFinal Result: {result.Result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

// Entry point for testing
// await ZeroShotPlannerExample.RunAsync();
