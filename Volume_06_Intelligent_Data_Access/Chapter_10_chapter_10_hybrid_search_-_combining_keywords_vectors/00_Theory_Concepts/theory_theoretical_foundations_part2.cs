
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

public async Task<IEnumerable<SearchResult>> HybridSearchAsync(string query, string embedding)
{
    // Phase 1: Parallel Execution
    var keywordTask = _context.Documents
        .FromSqlRaw("SELECT * FROM documents WHERE search_vector @@ to_tsquery({0})", query)
        .Select(d => new SearchResult { Id = d.Id, Rank = /* rank from SQL */ })
        .ToListAsync();

    var vectorTask = _context.Documents
        .FromSqlRaw("SELECT * FROM documents ORDER BY embedding <-> {0} LIMIT 50", embedding)
        .Select(d => new SearchResult { Id = d.Id, Rank = /* rank from SQL */ })
        .ToListAsync();

    await Task.WhenAll(keywordTask, vectorTask);

    var keywordResults = await keywordTask;
    var vectorResults = await vectorTask;

    // Phase 2: RRF Fusion
    var fusedScores = new Dictionary<int, double>();
    
    // Apply RRF to keyword results
    foreach (var result in keywordResults)
    {
        fusedScores[result.Id] = 1.0 / (60 + result.Rank);
    }

    // Apply RRF to vector results
    foreach (var result in vectorResults)
    {
        if (fusedScores.ContainsKey(result.Id))
        {
            fusedScores[result.Id] += 1.0 / (60 + result.Rank);
        }
        else
        {
            fusedScores[result.Id] = 1.0 / (60 + result.Rank);
        }
    }

    // Phase 3: Final Retrieval
    var topIds = fusedScores.OrderByDescending(kvp => kvp.Value).Take(10).Select(kvp => kvp.Key).ToList();
    
    return await _context.Documents
        .Where(d => topIds.Contains(d.Id))
        .Select(d => new SearchResult { Id = d.Id, /* ... */ })
        .ToListAsync();
}
