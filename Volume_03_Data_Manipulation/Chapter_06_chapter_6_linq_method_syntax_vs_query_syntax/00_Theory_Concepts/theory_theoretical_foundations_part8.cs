
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

# Source File: theory_theoretical_foundations_part8.cs
# Description: Theoretical Foundations
# ==========================================

public class TextPreprocessor
{
    // A pure functional pipeline for AI data prep
    public IEnumerable<string> PrepareForEmbedding(IEnumerable<string> rawCorpus)
    {
        // Declarative Query Syntax for readability in complex logic
        // (Simulating a scenario where we might join with a stop-word list)
        var query = 
            from text in rawCorpus
            where !string.IsNullOrWhiteSpace(text) // Cleaning
            select text.Trim().ToLower();          // Normalization

        // Method Syntax for chaining specific transformations
        // Deferred execution allows this to be composable
        return query
            .SelectMany(t => t.Split(' '))         // Tokenization (Flattening)
            .Where(token => token.Length > 2);     // Filter short tokens
    }
}
