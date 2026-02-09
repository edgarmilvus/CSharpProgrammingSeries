
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

# Source File: solution_exercise_31.cs
# Description: Solution for Exercise 31
# ==========================================

using System.Collections.Generic;
using System.Linq;

// 1. Accessible DTO
public class AccessibleDocumentResult
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Snippet { get; set; }
    
    // Accessibility Metadata
    public string AriaDescription { get; set; } // "Similarity: High, Contains relevant data about AI"
    public string ScreenReaderText { get; set; } // Explicit text for screen readers
}

// 2. Accessibility Service
public class AccessibilityEnhancer
{
    public List<AccessibleDocumentResult> EnhanceResults(List<DocumentResult> rawResults)
    {
        return rawResults.Select(r => new AccessibleDocumentResult
        {
            Id = r.Document.Id,
            Title = r.Document.Title,
            Snippet = r.Document.Content.Substring(0, 100),
            
            // 3. Generate Context for Screen Readers
            // Vector search is abstract. We need to explain the result.
            ScreenReaderText = $"Result {r.Document.Title}. Similarity score {r.Similarity:P0}. " +
                               $"Content preview: {r.Document.Content.Substring(0, 50)}...",
            
            // 4. ARIA Attributes (for UI binding)
            AriaDescription = $"Ranked {r.Similarity} out of 1.0 relevance."
        }).ToList();
    }
}

// 3. Keyboard Navigation Support (API Level)
public class SearchRequestWithFocus
{
    public string Query { get; set; }
    public int? FocusIndex { get; set; } // If user navigates via keyboard, request specific result
}
