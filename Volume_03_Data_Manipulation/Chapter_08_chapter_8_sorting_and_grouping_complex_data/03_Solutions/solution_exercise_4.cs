
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

public record UserEmbedding(string UserId, float[] Vector);
public record Anchor(string Category, float[] Vector);

public class VectorGroupingExercise
{
    private static double Distance(float[] v1, float[] v2)
    {
        double sum = 0;
        for (int i = 0; i < v1.Length; i++)
        {
            sum += Math.Pow(v1[i] - v2[i], 2);
        }
        return Math.Sqrt(sum);
    }

    public static Dictionary<string, List<string>> GroupUsersByNearestAnchor(List<UserEmbedding> users, List<Anchor> anchors)
    {
        return users
            .Select(u => new 
            {
                User = u,
                // Nested query: Project anchors to distance pairs, order them, and take the first.
                ClosestAnchor = anchors
                    .Select(a => new { Anchor = a, Dist = Distance(u.Vector, a.Vector) })
                    .OrderBy(d => d.Dist)
                    .First().Anchor
            })
            .GroupBy(x => x.ClosestAnchor.Category)
            .ToDictionary(
                g => g.Key, 
                g => g.Select(x => x.User.UserId).ToList()
            );
    }
}
