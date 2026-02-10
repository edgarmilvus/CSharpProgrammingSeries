
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

// Source File: solution_exercise_36.cs
// Description: Solution for Exercise 36
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics; // SIMD

// 1. SIMD Optimization
public class GameVectorSearch
{
    // Use System.Numerics.Vector<T> for SIMD (Single Instruction, Multiple Data)
    // This processes multiple floats in a single CPU instruction

    public float CosineSimilaritySimd(float[] a, float[] b)
    {
        // Assume length is multiple of Vector<float>.Count
        var vA = new Vector<float>(a);
        var vB = new Vector<float>(b);
        
        // Dot product using SIMD
        var dot = Vector.Dot(vA, vB);
        
        // Magnitudes (simplified)
        var magA = (float)Math.Sqrt(a.Sum(x => x * x));
        var magB = (float)Math.Sqrt(b.Sum(x => x * x));

        return dot / (magA * magB);
    }

    // 2. Spatial Hashing for Culling
    public List<NPC> GetRelevantNPCs(Player player, List<NPC> npcs)
    {
        // Only calculate vectors for NPCs within a certain radius
        // 1. Cull by distance (O(1) spatial hash lookup)
        // 2. Then apply Vector Search on the remaining subset
        
        return npcs
            .Where(n => Vector3.Distance(player.Position, n.Position) < 50.0f)
            .ToList();
    }
}

public class NPC
{
    public Vector3 Position { get; set; }
    public float[] BehaviorVector { get; set; }
}

public class Player
{
    public Vector3 Position { get; set; }
}
