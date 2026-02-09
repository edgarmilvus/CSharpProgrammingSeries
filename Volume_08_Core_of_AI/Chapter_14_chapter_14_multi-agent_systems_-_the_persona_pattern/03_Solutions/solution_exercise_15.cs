
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

# Source File: solution_exercise_15.cs
# Description: Solution for Exercise 15
# ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public class EventBus
{
    private readonly Dictionary<string, List<Func<string, Task>>> _handlers = new();

    public void Subscribe(string eventName, Func<string, Task> handler)
    {
        if (!_handlers.ContainsKey(eventName)) _handlers[eventName] = new List<Func<string, Task>>();
        _handlers[eventName].Add(handler);
    }

    public async Task PublishAsync(string eventName, string data)
    {
        if (_handlers.ContainsKey(eventName))
        {
            foreach (var handler in _handlers[eventName])
            {
                await handler(data);
            }
        }
    }
}

async Task RunEventDrivenBusAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var bus = new EventBus();
    var logAgent = new ChatCompletionAgent(kernel, "Analyze log. If error, output 'ERROR'. If warning, output 'WARNING'.");
    var pagerAgent = new ChatCompletionAgent(kernel, "Simulate sending a pager alert.");
    var slackAgent = new ChatCompletionAgent(kernel, "Simulate posting to Slack.");

    // Subscriptions
    bus.Subscribe("RawLog", async (log) => {
        var result = await logAgent.InvokeAsync(log);
        Console.WriteLine($"[LogMonitor]: Detected {result}");
        
        if (result.ToString().Contains("ERROR")) await bus.PublishAsync("ErrorFound", log);
        if (result.ToString().Contains("WARNING")) await bus.PublishAsync("WarningFound", log);
    });

    bus.Subscribe("ErrorFound", async (log) => {
        Console.WriteLine("[PagerAgent]: Alert sent!");
        await pagerAgent.InvokeAsync(log);
    });

    bus.Subscribe("WarningFound", async (log) => {
        Console.WriteLine("[SlackAgent]: Warning posted!");
        await slackAgent.InvokeAsync(log);
    });

    // Simulation
    await bus.PublishAsync("RawLog", "System error: Disk full");
    await bus.PublishAsync("RawLog", "System warning: High memory usage");
}
