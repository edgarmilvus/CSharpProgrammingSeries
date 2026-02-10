
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

// Source File: solution_exercise_28.cs
// Description: Solution for Exercise 28
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

async Task RunSentimentAnalysisRouterAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var sentimentAgent = new ChatCompletionAgent(kernel, "Analyze sentiment: Positive, Neutral, or Negative. Answer only with the word.");
    var salesAgent = new ChatCompletionAgent(kernel, "You are a Sales Agent. Upsell products.");
    var supportAgent = new ChatCompletionAgent(kernel, "You are Support. Help with issues.");
    var retentionAgent = new ChatCompletionAgent(kernel, "You are Retention. Apologize and offer discounts.");

    string userInput = "I love this product but it crashed.";

    // 1. Analyze Sentiment
    var sentiment = await sentimentAgent.InvokeAsync(userInput);
    Console.WriteLine($"Detected Sentiment: {sentiment}");

    // 2. Route
    string response = "";
    if (sentiment.ToString().Contains("Positive"))
    {
        response = (await salesAgent.InvokeAsync(userInput)).ToString();
    }
    else if (sentiment.ToString().Contains("Negative"))
    {
        response = (await retentionAgent.InvokeAsync(userInput)).ToString();
    }
    else
    {
        response = (await supportAgent.InvokeAsync(userInput)).ToString();
    }

    Console.WriteLine($"Routed to: {(sentiment.ToString().Contains("Negative") ? "Retention" : sentiment.ToString().Contains("Positive") ? "Sales" : "Support")}");
    Console.WriteLine($"Response: {response}");
}
