
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

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;

public class QuantizedDocument
{
    [Key]
    public int Id { get; set; }
    
    // 2. EF Core Implementation: Storage Optimization
    // Original vector (for re-ranking) - stored as BLOB/JSON or separate table
    public float[] OriginalVector { get; set; } 
    
    // Compressed vector (Binary Quantization)
    // 768 dimensions -> 768 bits -> 96 bytes
    public byte[] CompressedVector { get; set; } 
}

public class EdgeContext : DbContext
{
    public DbSet<QuantizedDocument> Documents { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Local file-based DB for edge scenario
        optionsBuilder.UseSqlite("Data Source=edge_vector.db");
    }
}

public class QuantizationService
{
    private readonly EdgeContext _context;

    public QuantizationService(EdgeContext context)
    {
        _context = context;
    }

    // 4. Compression Logic (Binarization)
    public static byte[] CompressVector(float[] vector)
    {
        // Binary Quantization: Map float to 1 bit (1 if positive, 0 if negative/zero)
        int byteCount = (vector.Length / 8) + (vector.Length % 8 > 0 ? 1 : 0);
        byte[] bytes = new byte[byteCount];

        for (int i = 0; i < vector.Length; i++)
        {
            int byteIndex = i / 8;
            int bitIndex = i % 8;
            
            if (vector[i] > 0)
            {
                bytes[byteIndex] |= (byte)(1 << bitIndex);
            }
        }
        return bytes;
    }

    // 3. Two-Stage Search Algorithm
    public async Task<List<QuantizedDocument>> SearchAsync(float[] queryVector, int candidateCount = 1000)
    {
        // Stage 1: Candidate Selection (using Compressed Vectors)
        // We calculate Hamming distance on the compressed bytes.
        // Note: In a real DB, this logic is pushed down to the DB engine or done in a specialized vector store.
        // Here we simulate fetching candidates based on compressed similarity.
        
        var allDocs = await _context.Documents.ToListAsync(); // Warning: Heavy for 10M records, but simulates the logic
        
        // Calculate Hamming Distance (XOR + BitCount) on compressed vectors
        var queryCompressed = CompressVector(queryVector);
        
        var candidates = allDocs
            .Select(d => new 
            { 
                Doc = d, 
                Distance = HammingDistance(d.CompressedVector, queryCompressed) 
            })
            .OrderBy(x => x.Distance) // Lower Hamming distance = more similar
            .Take(candidateCount)
            .ToList();

        // Stage 2: Re-ranking (using Original Vectors)
        // Load full precision vectors for the candidates and calculate exact Euclidean distance
        var results = candidates
            .Select(x => new 
            { 
                x.Doc, 
                ExactDistance = EuclideanDistance(x.Doc.OriginalVector, queryVector) 
            })
            .OrderBy(x => x.ExactDistance)
            .Select(x => x.Doc)
            .ToList();

        return results;
    }

    private int HammingDistance(byte[] v1, byte[] v2)
    {
        int distance = 0;
        for (int i = 0; i < v1.Length; i++)
        {
            // XOR operation highlights differing bits
            byte xor = (byte)(v1[i] ^ v2[i]);
            // Count set bits (population count)
            distance += CountSetBits(xor);
        }
        return distance;
    }

    private int CountSetBits(byte b)
    {
        int count = 0;
        while (b > 0)
        {
            count += b & 1;
            b >>= 1;
        }
        return count;
    }

    private double EuclideanDistance(float[] v1, float[] v2)
    {
        double sum = 0;
        for (int i = 0; i < v1.Length; i++) sum += Math.Pow(v1[i] - v2[i], 2);
        return Math.Sqrt(sum);
    }
}
