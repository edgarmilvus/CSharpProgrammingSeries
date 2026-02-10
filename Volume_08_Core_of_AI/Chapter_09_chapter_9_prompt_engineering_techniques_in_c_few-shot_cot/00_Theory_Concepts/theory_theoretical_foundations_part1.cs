
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

// Source File: theory_theoretical_foundations_part1.cs
// Description: Theoretical Foundations
// ==========================================

using System.Collections.Immutable;

namespace AI.Engineering.Core.Prompts
{
    // Using Records for immutable data transfer objects representing Few-Shot examples.
    // This ensures thread-safety when the Kernel processes parallel requests.
    public record FewShotExample(string Input, string Output);

    public class EntityExtractionPrompt
    {
        public string TaskDescription { get; init; } = "Extract the primary entity and sentiment from the text.";
        
        // We use an ImmutableList to guarantee that the examples provided 
        // to the model remain constant throughout the prompt construction lifecycle.
        public ImmutableList<FewShotExample> Examples { get; init; } = ImmutableList<FewShotExample>.Empty;

        // The construction of the final prompt string is a pure function.
        // Given the same inputs, it produces the exact same prompt structure.
        public string BuildPrompt(string query)
        {
            // Theoretical construction of the prompt string
            // 1. System Role / Task Description
            // 2. Iteration of Examples (Input -> Output)
            // 3. Delimiter (e.g., "---")
            // 4. The actual user query
            
            // In a real implementation, this would use a StringBuilder or 
            // a templating engine like Handlebars.Net to avoid GC pressure.
            return $"{TaskDescription}\n\n" +
                   string.Join("\n", Examples.Select(ex => $"Input: {ex.Input}\nOutput: {ex.Output}")) +
                   $"\n---\nInput: {query}\nOutput:";
        }
    }
}
