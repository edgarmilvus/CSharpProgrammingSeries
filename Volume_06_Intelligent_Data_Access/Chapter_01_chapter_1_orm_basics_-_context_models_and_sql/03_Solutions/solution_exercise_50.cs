
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

# Source File: solution_exercise_50.cs
# Description: Solution for Exercise 50
# ==========================================

using System.Collections.Generic;
using System.Linq;

// 1. Geo-Routing Service
public class GlobalVectorService
{
    private readonly Dictionary<string, IVectorRepository> _regionalRepos;

    public async Task<List<Document>> SearchAsync(string query, string userRegion)
    {
        // 1. Data Residency Check
        // Route the query to the region where the user is located
        // This ensures GDPR / Data Sovereignty compliance
        
        var repo = GetRepositoryForRegion(userRegion);

        // 2. Generate Vector (Region specific model if needed)
        var vector = await GenerateEmbedding(query);

        // 3. Search Local Region
        return await repo.SearchAsync(vector, 10);
    }

    private IVectorRepository GetRepositoryForRegion(string region)
    {
        // Map "EU", "US", "Asia" to specific database endpoints
        return _regionalRepos[region];
    }

    // 4. Cross-Region Replication (for Global Queries)
    public async Task<List<Document>> GlobalSearchAsync(string query)
    {
        // Only for authorized users (e.g., Global Admins)
        // Aggregate results from all regions
        var tasks = _regionalRepos.Values.Select(r => r.SearchAsync(query, 10));
        var results = await Task.WhenAll(tasks);
        
        return results.SelectMany(x => x).ToList();
    }
}
