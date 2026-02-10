
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

// Source File: theory_theoretical_foundations_part2.cs
// Description: Theoretical Foundations
// ==========================================

// This is a conceptual example of configuring the kernel.
// It demonstrates how the DI container is populated.
var builder = Kernel.CreateBuilder();

// Here, we are registering a concrete implementation (AzureOpenAITextGenerationService)
// against an interface (ITextGenerationService).
// We are telling the DI container: "When someone asks for an ITextGenerationService,
// give them an instance of AzureOpenAITextGenerationService."
builder.Services.AddAzureOpenAIChatCompletion(
    deploymentName: "gpt-4",
    endpoint: "https://your-endpoint.openai.azure.com/",
    apiKey: "your-api-key");

// The kernel is built, and the DI container is now sealed for this instance.
Kernel kernel = builder.Build();
