
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

# Source File: solution_exercise_12.cs
# Description: Solution for Exercise 12
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;
using System.Text.Json.Serialization;

public class AgentConfig
{
    public string Name { get; set; } = "";
    public string SystemPrompt { get; set; } = "";
    public string Model { get; set; } = "gpt-4o";
}

public class AgentFactory
{
    private readonly Kernel _kernel;

    public AgentFactory(Kernel kernel) => _kernel = kernel;

    public List<ChatCompletionAgent> CreateAgentsFromJson(string jsonConfig)
    {
        var configs = JsonSerializer.Deserialize<List<AgentConfig>>(jsonConfig) ?? new();
        var agents = new List<ChatCompletionAgent>();

        foreach (var config in configs)
        {
            // In a real scenario, we might switch models here based on config.Model
            var agent = new ChatCompletionAgent(_kernel, config.SystemPrompt);
            agent.Name = config.Name; // Assuming Name property exists or tracking separately
            agents.Add(agent);
        }
        return agents;
    }
}

async Task RunAgentFactoryAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    string jsonConfig = """
    [
        { "Name": "FlightFinder", "SystemPrompt": "You find flights." },
        { "Name": "HotelBooker", "SystemPrompt": "You book hotels." }
    ]
    """;

    var factory = new AgentFactory(kernel);
    var agents = factory.CreateAgentsFromJson(jsonConfig);

    Console.WriteLine($"Loaded {agents.Count} agents.");

    // Dynamic Invocation
    Console.Write("What do you want to book? (Flight/Hotel): ");
    string input = Console.ReadLine() ?? "Flight";
    
    var targetAgent = agents.FirstOrDefault(a => a.Name == "FlightFinder" || (input == "Hotel" && a.Name == "HotelBooker"));
    
    if (targetAgent != null)
    {
        var response = await targetAgent.InvokeAsync("Find options for NYC to London.");
        Console.WriteLine($"Agent {targetAgent.Name} Response: {response}");
    }
}
