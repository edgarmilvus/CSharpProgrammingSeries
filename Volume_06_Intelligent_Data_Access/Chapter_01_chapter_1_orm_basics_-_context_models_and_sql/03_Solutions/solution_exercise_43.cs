
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

# Source File: solution_exercise_43.cs
# Description: Solution for Exercise 43
# ==========================================

using System.Collections.Generic;
using System.Linq;

// 1. Real-Time Clustering
public class NewsClusterService
{
    public List<NewsCluster> ClusterArticles(List<Article> articles)
    {
        // 1. Calculate Pairwise Similarities
        // 2. Group articles with similarity > threshold
        // 3. This happens in real-time as news breaks

        var clusters = new List<NewsCluster>();

        foreach (var article in articles)
        {
            var vector = article.Vector;
            
            // Find existing cluster
            var match = clusters.FirstOrDefault(c => 
                CalculateSimilarity(c.Centroid, vector) > 0.85
            );

            if (match != null)
            {
                match.Articles.Add(article);
                // Update centroid
                match.Centroid = UpdateCentroid(match.Centroid, vector);
            }
            else
            {
                clusters.Add(new NewsCluster { Centroid = vector, Articles = new List<Article> { article } });
            }
        }

        return clusters;
    }

    private float[] UpdateCentroid(float[] a, float[] b) => a; // Mock
    private double CalculateSimilarity(float[] a, float[] b) => 0.9; // Mock
}

public class NewsCluster
{
    public float[] Centroid { get; set; }
    public List<Article> Articles { get; set; }
}
