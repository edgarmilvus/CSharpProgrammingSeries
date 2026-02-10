
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

// Source File: solution_exercise_49.cs
// Description: Solution for Exercise 49
// ==========================================

using System.Collections.Generic;

// 1. Open Data Standard (JSON-LD / Schema.org)
public class OpenResearchService
{
    // Use open formats for data sharing
    public ResearchPaper ExportPaper(int id)
    {
        var paper = GetPaper(id);
        
        // 2. Embeddings as Open Data
        // Include the vector in the export so others don't need to re-calculate
        return paper;
    }

    // 3. Collaborative Platform
    public async Task UploadPaper(ResearchPaper paper)
    {
        // 1. Generate Vector
        // 2. Store in Open Source DB (e.g., Milvus or Qdrant)
        // 3. Make accessible to other researchers (with permissions)
        await _repo.UpsertAsync(paper);
    }
}
