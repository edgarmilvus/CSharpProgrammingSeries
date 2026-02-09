
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

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using System;
using System.Threading.Tasks;

namespace SemanticKernelPlannerExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. KERNEL SETUP
            // We initialize the kernel to act as the orchestrator.
            // In a real scenario, this would include configuration for LLM services (e.g., Azure OpenAI).
            // For this demonstration, we simulate the environment.
            var kernel = Kernel.CreateBuilder().Build();

            // 2. PLUGIN REGISTRATION (SKILLS)
            // We register our available tools (plugins) that the planner can use.
            // These represent specific capabilities: checking weather and booking appointments.
            // Note: In a real implementation, these would be decorated with [KernelFunction].
            // Here, we conceptually register them for the planner to reference.
            var calendarPlugin = new CalendarPlugin();
            var weatherPlugin = new WeatherPlugin();

            // 3. GOAL DEFINITION
            // The user's natural language request is the input for the planner.
            // The planner's job is to break this down into actionable steps.
            string userGoal = "Check the weather for tomorrow and if it's sunny, book a meeting with the client at 2 PM.";

            Console.WriteLine($"Processing Goal: \"{userGoal}\"\n");

            // 4. PLANNER INITIALIZATION
            // We use the FunctionCallingStepwisePlanner, which is the core engine for auto-orchestration.
            // It utilizes the LLM to generate a step-by-step plan based on available functions.
            var planner = new FunctionCallingStepwisePlanner();

            // 5. EXECUTION
            // The planner generates a plan and executes it. It handles parameter resolution
            // and function calling automatically.
            try
            {
                // Note: In a real environment, this requires a configured LLM.
                // Since we cannot run the actual LLM here, we will simulate the planner's output
                // to demonstrate the logic flow and structure.
                // var result = await planner.ExecuteAsync(kernel, userGoal);
                
                // SIMULATION OF PLANNER OUTPUT
                SimulatePlannerExecution(kernel, calendarPlugin, weatherPlugin, userGoal);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during planning: {ex.Message}");
            }
        }

        // SIMULATION HELPER
        // Since we don't have a live LLM connection in this console app, 
        // we simulate the planner's decision process to demonstrate the concepts.
        static void SimulatePlannerExecution(Kernel kernel, CalendarPlugin calendar, WeatherPlugin weather, string goal)
        {
            Console.WriteLine("--- PLANNER GENERATED PLAN ---");
            Console.WriteLine("Step 1: Invoke 'GetWeatherForecast' with parameter 'date' = 'tomorrow'.");
            string forecast = weather.GetWeatherForecast("tomorrow");
            Console.WriteLine($"Result: {forecast}");

            Console.WriteLine("\nStep 2: Analyze forecast result.");
            bool isSunny = forecast.Contains("Sunny", StringComparison.OrdinalIgnoreCase);

            if (isSunny)
            {
                Console.WriteLine("Condition met: Weather is sunny.");
                Console.WriteLine("Step 3: Invoke 'BookMeeting' with parameters 'time' = '2 PM' and 'attendee' = 'Client'.");
                string bookingResult = calendar.BookMeeting("2 PM", "Client");
                Console.WriteLine($"Result: {bookingResult}");
            }
            else
            {
                Console.WriteLine("Condition failed: Weather is not sunny. Skipping meeting booking.");
            }
            
            Console.WriteLine("\n--- PLAN EXECUTION COMPLETE ---");
        }
    }

    // MOCK PLUGIN: CALENDAR
    // Represents a semantic function (skill) available to the planner.
    public class CalendarPlugin
    {
        public string BookMeeting(string time, string attendee)
        {
            return $"Meeting booked with {attendee} at {time}.";
        }
    }

    // MOCK PLUGIN: WEATHER
    // Represents a semantic function (skill) available to the planner.
    public class WeatherPlugin
    {
        public string GetWeatherForecast(string date)
        {
            // Simulating LLM or API response
            return $"Forecast for {date}: Sunny, 75°F.";
        }
    }
}
