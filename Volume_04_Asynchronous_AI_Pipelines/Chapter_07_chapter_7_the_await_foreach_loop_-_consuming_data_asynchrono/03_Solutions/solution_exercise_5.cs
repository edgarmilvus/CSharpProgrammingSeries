
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class AsyncLLMStreamProcessor
{
    private enum ProcessingState { Normal, CodeBlock }

    public async Task ProcessStreamAsync(IAsyncEnumerable<string> tokenStream, CancellationToken ct = default)
    {
        var state = ProcessingState.Normal;
        var codeBuffer = new StringBuilder();

        Console.WriteLine("Starting LLM Stream Processing...\n");

        await foreach (var token in tokenStream.WithCancellation(ct))
        {
            switch (state)
            {
                case ProcessingState.Normal:
                    if (token == "<START_CODE_BLOCK>")
                    {
                        state = ProcessingState.CodeBlock;
                        // Optional: Print a notification
                        Console.WriteLine("\n[Entering Code Mode...]");
                    }
                    else
                    {
                        // Print non-code tokens immediately
                        Console.Write(token);
                    }
                    break;

                case ProcessingState.CodeBlock:
                    if (token == "<END_CODE_BLOCK>")
                    {
                        // Exit Code Mode: Format and Print buffer
                        state = ProcessingState.Normal;
                        Console.WriteLine("\n