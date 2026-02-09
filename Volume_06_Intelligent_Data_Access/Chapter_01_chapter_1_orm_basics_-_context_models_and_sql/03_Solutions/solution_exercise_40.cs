
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

# Source File: solution_exercise_40.cs
# Description: Solution for Exercise 40
# ==========================================

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// 1. Formula-Aware Tokenization
public class ScientificPreprocessor
{
    public string Preprocess(string content)
    {
        // 1. Extract Formulas
        // LaTeX formulas like $E=mc^2$ are distinct semantic units
        var formulaMatches = Regex.Matches(content, @"\$(.*?)\$");
        
        // 2. Replace formulas with tokens
        string processed = content;
        foreach (Match match in formulaMatches)
        {
            processed = processed.Replace(match.Value, " [FORMULA_START] " + match.Value + " [FORMULA_END] ");
        }

        return processed;
    }
}

// 2. Cross-Disciplinary Search
public class ResearchVectorService
{
    private readonly IVectorRepository<Paper> _repo;

    public async Task<List<Paper>> FindSimilarResearch(string abstractText)
    {
        // 1. Preprocess
        var preprocessor = new ScientificPreprocessor();
        var cleanText = preprocessor.Preprocess(abstractText);

        // 2. Generate Embedding
        // The model must be trained on scientific text (e.g., SciBERT)
        var vector = await GenerateScientificEmbedding(cleanText);

        // 3. Search
        return await _repo.SearchAsync(vector, 10);
    }

    private async Task<float[]> GenerateScientificEmbedding(string text)
    {
        // Mock: Use a model that understands scientific context
        return new float[768];
    }
}
