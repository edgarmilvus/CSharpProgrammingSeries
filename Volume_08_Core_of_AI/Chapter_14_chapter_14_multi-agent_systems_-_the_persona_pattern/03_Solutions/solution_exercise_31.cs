
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

// Source File: solution_exercise_31.cs
// Description: Solution for Exercise 31
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

async Task RunMeetingMinutesAsync()
{
    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion("gpt-4o", "api-key-here");
    var kernel = builder.Build();

    var preprocessor = new ChatCompletionAgent(kernel, "Clean the transcript: remove filler words and timestamps.");
    var analyzer = new ChatCompletionAgent(kernel, "Extract 'Decisions' and 'Action Items' with assignees.");
    var formatter = new ChatCompletionAgent(kernel, "Format the extracted data into clean Markdown.");

    string transcript = @"
        [00:01] Alice: Let's discuss the launch. 
        [00:05] Bob: We need to fix the bug. Alice, can you do it? 
        [00:10] Alice: Decision: We launch on Friday.";

    // 1. Preprocess
    var cleanTranscript = await preprocessor.InvokeAsync(transcript);

    // 2. Analyze
    var analysis = await analyzer.InvokeAsync(cleanTranscript.ToString());

    // 3. Format
    var markdown = await formatter.InvokeAsync(analysis.ToString());

    Console.WriteLine($"Meeting Minutes:\n{markdown}");
}
