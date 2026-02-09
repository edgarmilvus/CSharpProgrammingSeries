
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

# Source File: solution_exercise_45.cs
# Description: Solution for Exercise 45
# ==========================================

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

// 1. Clearance Level Tagging
public class ClassifiedDocument
{
    public int Id { get; set; }
    public float[] Vector { get; set; }
    public string ClearanceLevel { get; set; } // "Public", "Secret", "TopSecret"
}

// 2. Access Control Service
public class ClassifiedSearchService
{
    private readonly IVectorRepository<ClassifiedDocument> _repo;

    public List<ClassifiedDocument> Search(ClaimsPrincipal user, string query)
    {
        // 1. Get User Clearance
        var clearance = user.FindFirst("Clearance")?.Value ?? "Public";

        // 2. Generate Vector
        var vector = GenerateEmbedding(query);

        // 3. Search (Pre-filtered by Clearance)
        // We cannot simply search all and filter later (security risk).
        // The repository must support filtering by metadata during the vector search.
        var results = _repo.SearchAsync(vector, 10, filters: new { ClearanceLevel = clearance });

        return results;
    }

    // 3. FOIA Request Processing
    public List<ClassifiedDocument> ProcessFOIA(string request)
    {
        // 1. Vectorize request
        // 2. Search database
        // 3. Automatically redact "TopSecret" results
        // 4. Return only "Secret" or "Public" matches
        return new List<ClassifiedDocument>();
    }
}
