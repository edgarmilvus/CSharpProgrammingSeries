
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

// Source File: solution_exercise_24.cs
// Description: Solution for Exercise 24
// ==========================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// 1. Unified API Specification
public interface IVectorSearchApi
{
    Task<SearchResponse> SearchAsync(SearchRequest request);
}

public class SearchRequest
{
    public string QueryText { get; set; }
    public string UserId { get; set; }
    public string Platform { get; set; } // "Web", "iOS", "Android"
    public int Limit { get; set; }
}

public class SearchResponse
{
    public List<DocumentResult> Results { get; set; }
    public string AlgorithmVersion { get; set; }
}

// 2. Platform-Specific Optimizations (Server-side)
public class PlatformAwareSearchService : IVectorSearchApi
{
    public async Task<SearchResponse> SearchAsync(SearchRequest request)
    {
        // 3. Generate Embeddings (Consistent)
        // Must use the EXACT same model/version across all platforms
        var vector = await GenerateEmbedding(request.QueryText);

        // 4. Platform-Specific Result Formatting
        var results = await ExecuteVectorSearch(vector, request.Limit);

        // Mobile: Send smaller payloads (less metadata)
        if (request.Platform == "iOS" || request.Platform == "Android")
        {
            results = CompressResultsForMobile(results);
        }

        // Web: Can handle richer payloads
        if (request.Platform == "Web")
        {
            results = AddWebMetadata(results);
        }

        return new SearchResponse
        {
            Results = results,
            AlgorithmVersion = "v2.1" // Client can log this for debugging
        };
    }

    private List<DocumentResult> CompressResultsForMobile(List<DocumentResult> results)
    {
        // Remove heavy fields, truncate text
        return results;
    }

    private List<DocumentResult> AddWebMetadata(List<DocumentResult> results)
    {
        // Add preview images, full text, etc.
        return results;
    }

    private async Task<float[]> GenerateEmbedding(string text) => new float[128];
    private async Task<List<DocumentResult>> ExecuteVectorSearch(float[] vec, int limit) => new();
}

// 5. Client-Side Consistency Check
public class ClientConsistency
{
    public bool VerifyResultIntegrity(SearchResponse response)
    {
        // Check version match
        if (response.AlgorithmVersion != "v2.1")
        {
            // Prompt user to update app
            return false;
        }
        return true;
    }
}
