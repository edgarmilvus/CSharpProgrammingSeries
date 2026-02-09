
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;

public record SearchOptions(
    string SearchText, 
    double MatchThreshold = 0.5, 
    string? FilterByType = null
);

public static class LogQueryExtensions
{
    public static IQueryable<Step> SearchLogsByContent(
        this DbContext context, 
        SearchOptions options)
    {
        var query = context.Set<Step>().AsQueryable();

        // 1. Tag the query for SQL debugging
        query = query.TagWith($"Search: Text='{options.SearchText}', Threshold={options.MatchThreshold}");

        // 2. Pre-Filter by Type (Pushed to DB)
        if (!string.IsNullOrEmpty(options.FilterByType))
        {
            query = query.Where(s => s.StepType == options.FilterByType);
        }

        // 3. Hybrid Search Logic
        string term = options.SearchText;

        // Check if GUID
        if (Guid.TryParse(term, out var guidVal))
        {
            // Exact match on ID (Pushed to DB)
            return query.Where(s => s.StepId == guidVal);
        }

        // Check if Date (Simple heuristic for demo)
        if (DateTimeOffset.TryParse(term, out var dateVal))
        {
            // Range filter (Pushed to DB)
            // Match if step occurred on this day
            var nextDay = dateVal.AddDays(1);
            return query.Where(s => s.ChainExecution.StartTime >= dateVal && s.ChainExecution.StartTime < nextDay);
        }

        // 4. Text Search (Pushed to DB via SQL LIKE)
        // We filter records containing the text first
        var filteredQuery = query.Where(s => 
            s.InputPayload.Contains(term) || 
            s.OutputPayload.Contains(term)
        );

        // 5. Simulated "Semantic" Scoring (Client-Side)
        // In a real scenario, this would be a DB function call (e.g., pgvector distance).
        // Here, we execute the DB query, then calculate a score in memory.
        // WARNING: ToList() triggers the DB query. Only do this if the result set is small.
        // If the dataset is huge, the logic should be restructured to do more filtering in DB first.
        
        var results = filteredQuery.ToList(); // Materialize

        // Client-side evaluation for the "Similarity Score"
        var scoredResults = results.Select(s => new 
        {
            Step = s,
            Score = CalculateSimulatedSimilarity(s, term)
        })
        .Where(r => r.Score >= options.MatchThreshold)
        .Select(r => r.Step);

        return scoredResults.AsQueryable();
    }

    private static double CalculateSimulatedSimilarity(Step step, string term)
    {
        // Simulate vector distance (e.g., cosine similarity)
        // Here we just check if the term appears in the output more than input, 
        // or arbitrary logic for demonstration.
        if (step.OutputPayload.Contains(term)) return 0.9;
        if (step.InputPayload.Contains(term)) return 0.7;
        return 0.1;
    }
}
