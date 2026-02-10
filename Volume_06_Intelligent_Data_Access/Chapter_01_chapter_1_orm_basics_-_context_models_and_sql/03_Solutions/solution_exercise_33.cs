
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

// Source File: solution_exercise_33.cs
// Description: Solution for Exercise 33
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

// 1. Versioned Entity
public class VersionedDocument
{
    public int Id { get; set; }
    public string Title { get; set; }
    
    // List of historical vectors
    public List<VectorVersion> VectorHistory { get; set; } = new();
    
    // Current active vector
    public float[] CurrentVector => VectorHistory.LastOrDefault()?.Vector;
}

public class VectorVersion
{
    public Guid VersionId { get; set; }
    public float[] Vector { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string ModelVersion { get; set; } // e.g., "v2.1"
}

// 2. Diff Algorithm
public class VectorDiff
{
    public static double CalculateDistance(float[] v1, float[] v2)
    {
        // Euclidean distance between versions
        double sum = 0;
        for (int i = 0; i < v1.Length; i++)
        {
            sum += Math.Pow(v1[i] - v2[i], 2);
        }
        return Math.Sqrt(sum);
    }
}

// 3. Rollback Mechanism
public class VectorVersionControl
{
    public void Rollback(VersionedDocument doc, Guid versionId)
    {
        var version = doc.VectorHistory.FirstOrDefault(v => v.VersionId == versionId);
        if (version != null)
        {
            // Move specific version to end of list (making it current)
            doc.VectorHistory.Remove(version);
            doc.VectorHistory.Add(version);
        }
    }
}
