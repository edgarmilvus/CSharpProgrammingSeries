
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public static class DataCleaner
{
    public static IEnumerable<PreprocessedText> CleanTextData(IEnumerable<TextData> rawData)
    {
        // Regex pattern to match HTML tags (e.g., <br>, <div>)
        // Compiled for performance in repeated execution scenarios.
        var htmlRegex = new Regex("<.*?>", RegexOptions.Compiled);

        return rawData
            // 1. Filter: Immediate check for null or whitespace. 
            // Deferred execution allows this check to happen only when enumerated.
            .Where(d => !string.IsNullOrWhiteSpace(d.RawText))
            
            // 2. Transformation: Clean the text.
            // We project into an anonymous type to hold the cleaned text alongside original metadata.
            .Select(d => new 
            { 
                Cleaned = htmlRegex.Replace(d.RawText, ""), // Remove HTML tags
                d.Label, 
                d.Score 
            })
            
            // 3. Tokenization & Flattening: SelectMany is the key operator here.
            // It takes a single input (d) and a collection selector (d.Cleaned.Split(...)).
            // It projects each token from the split collection into the final output type.
            .SelectMany(
                d => d.Cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries), 
                (d, token) => new PreprocessedText(token, d.Score)
            );
    }
}
