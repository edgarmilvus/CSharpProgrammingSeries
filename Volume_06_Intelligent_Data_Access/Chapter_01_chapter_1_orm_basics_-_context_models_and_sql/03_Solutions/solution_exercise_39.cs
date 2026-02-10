
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

// Source File: solution_exercise_39.cs
// Description: Solution for Exercise 39
// ==========================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 1. Hybrid Search (Vector + Keyword)
public class LegalSearchService
{
    private readonly IVectorRepository<LegalDoc> _vectorRepo;
    private readonly ILuceneIndex _keywordIndex; // Traditional inverted index

    public async Task<List<LegalDoc>> SearchAsync(string query, string citation = null)
    {
        // 1. Keyword Filter (for Citations)
        // "Smith v. Jones (2023)" is best found via exact keyword match, not vector.
        if (!string.IsNullOrEmpty(citation))
        {
            var ids = _keywordIndex.SearchIds(citation);
            return await _vectorRepo.GetByIdsAsync(ids);
        }

        // 2. Vector Search (for Semantic Meaning)
        var vector = await GenerateEmbedding(query);
        var vectorResults = await _vectorRepo.SearchAsync(vector, 10);

        // 3. Reranking for Precision
        // Legal documents often have standard structures. We can boost scores for documents
        // that contain specific sections (e.g., "Holding", "Dictum").
        return vectorResults
            .OrderByDescending(d => d.HasHoldingSection ? 1.1 : 1.0) // Boost
            .ThenByDescending(d => d.Similarity)
            .ToList();
    }
}
