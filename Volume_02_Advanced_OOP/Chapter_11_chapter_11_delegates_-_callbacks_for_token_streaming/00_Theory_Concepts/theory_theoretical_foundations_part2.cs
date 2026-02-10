
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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LLMGenerator
{
    // We define a delegate type specifically for token streaming.
    // This signature takes a string (the token) and returns void.
    public delegate void TokenCallback(string token);

    // A list of simulated tokens representing the LLM's output.
    private readonly List<string> _simulatedTokens = new List<string> 
    { 
        "The", " quick", " brown", " fox", " jumps", " over", " the", " lazy", " dog." 
    };

    /// <summary>
    /// Generates tokens asynchronously and invokes the callback for each token.
    /// </summary>
    /// <param name="onToken">The delegate to call when a token is generated.</param>
    /// <param name="onComplete">The delegate to call when the stream finishes.</param>
    public async Task GenerateAsync(TokenCallback onToken, Action onComplete)
    {
        foreach (var token in _simulatedTokens)
        {
            // Simulate network latency
            await Task.Delay(100); 
            
            // INVOKE THE DELEGATE
            // This is the "callback". The generator calls back to the caller
            // with the specific data.
            onToken?.Invoke(token);
        }

        // Signal that the stream is finished
        onComplete?.Invoke();
    }
}
