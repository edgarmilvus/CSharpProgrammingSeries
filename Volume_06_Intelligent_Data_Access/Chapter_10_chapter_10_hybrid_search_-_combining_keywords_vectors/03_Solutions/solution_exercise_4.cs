
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Security.Cryptography;

public interface ISearchRouter
{
    Task<IEnumerable<SearchResult>> SearchAsync(string query);
}

public class SearchRouter : ISearchRouter
{
    private readonly HybridSearchService _hybridService;
    private readonly IMemoryCache _cache;
    private readonly Dictionary<int, double> _popularityScores; // Simulating a DB for popularity

    public SearchRouter(HybridSearchService hybridService, IMemoryCache cache)
    {
        _hybridService = hybridService;
        _cache = cache;
        _popularityScores = new Dictionary<int, double>();
    }

    public async Task<IEnumerable<SearchResult>> SearchAsync(string query)
    {
        // 1. Determine Search Mode
        var mode = DetermineMode(query);

        // 2. Generate Cache Key
        string cacheKey = GenerateCacheKey(query, mode);

        // 3. Check Cache
        if (_cache.TryGetValue(cacheKey, out List<SearchResult>? cachedResults) && cachedResults != null)
        {
            return cachedResults;
        }

        // 4. Execute Search
        // Note: We pass 0.5 for alpha (default hybrid balance) or adjust based on mode
        double alpha = mode == SearchMode.Keyword ? 0.0 : (mode == SearchMode.Vector ? 1.0 : 0.5);
        var results = await _hybridService.SearchAsync(query, mode, alpha);

        // 5. Apply Feedback Loop (Popularity Boost)
        // We modify the RRF score by adding the popularity score.
        foreach (var result in results)
        {
            if (_popularityScores.TryGetValue(result.Id, out double popScore))
            {
                result.Score += popScore * 0.1; // Apply a small weight to popularity
            }
        }

        // Re-sort after popularity boost
        var finalResults = results.OrderByDescending(r => r.Score).ToList();

        // 6. Cache the result
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5));

        _cache.Set(cacheKey, finalResults, cacheOptions);

        return finalResults;
    }

    private SearchMode DetermineMode(string query)
    {
        // Heuristic logic
        if (Regex.IsMatch(query, @"^\d+$") || query.Contains("ID", StringComparison.OrdinalIgnoreCase))
            return SearchMode.Keyword; // Specific codes/IDs
        
        if (query.Split(' ').Length > 3 && query.Contains('?'))
            return SearchMode.Vector; // Natural language questions
        
        return SearchMode.Hybrid; // Default
    }

    private string GenerateCacheKey(string query, SearchMode mode)
    {
        using var sha256 = SHA256.Create();
        var inputBytes = Encoding.UTF8.GetBytes($"{query}_{mode}");
        var hashBytes = sha256.ComputeHash(inputBytes);
        return Convert.ToBase64String(hashBytes);
    }

    // Interactive Challenge: Feedback Loop
    public void LogUserInteraction(string query, int resultId, bool clicked)
    {
        // In a real system, we would persist this to a DB.
        // Here we update the in-memory dictionary.
        
        if (clicked)
        {
            if (!_popularityScores.ContainsKey(resultId))
                _popularityScores[resultId] = 0;
            
            _popularityScores[resultId] += 1.0;
            
            // Invalidate relevant caches so new rankings take effect
            // (Simplified: Invalidation logic usually scans keys or uses tags)
            // For this demo, we clear the cache to force re-ranking on next search
            _cache.Clear(); 
        }
    }
}
