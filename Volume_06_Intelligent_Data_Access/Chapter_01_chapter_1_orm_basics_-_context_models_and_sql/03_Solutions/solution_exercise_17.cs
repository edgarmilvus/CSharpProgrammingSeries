
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

// Source File: solution_exercise_17.cs
// Description: Solution for Exercise 17
// ==========================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueConversion;
using System;
using System.Linq;

// 1. Quantization Interface
public interface IVectorQuantizer
{
    byte[] Quantize(float[] vector);
    float[] Dequantize(byte[] quantized);
}

// 2. Scalar Quantization (Float32 -> Int8)
public class ScalarQuantizer : IVectorQuantizer
{
    public byte[] Quantize(float[] vector)
    {
        // Map float range [-1, 1] to byte [0, 255]
        return vector.Select(v => (byte)((v + 1) * 127.5f)).ToArray();
    }

    public float[] Dequantize(byte[] quantized)
    {
        // Map back
        return quantized.Select(b => (b / 127.5f) - 1).ToArray();
    }
}

// 3. Value Converter using Quantization
public class QuantizedVectorConverter : ValueConverter<float[], byte[]>
{
    private static readonly ScalarQuantizer _quantizer = new();

    public QuantizedVectorConverter() 
        : base(
            v => _quantizer.Quantize(v),
            v => _quantizer.Dequantize(v)
        )
    { }
}

// 4. Tiered Storage System
public class TieredStorageContext : DbContext
{
    public DbSet<Document> Documents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Store as Quantized (1 byte per dimension vs 4 bytes)
        modelBuilder.Entity<Document>()
            .Property(d => d.Vector)
            .HasConversion(new QuantizedVectorConverter())
            .HasColumnType("varbinary(max)"); // Store as binary blob

        // 5. Vector Pruning Strategy
        // We can add a flag to mark low-importance vectors
        modelBuilder.Entity<Document>()
            .Property(d => d.IsArchived)
            .HasDefaultValue(false);
    }

    public async Task<List<Document>> GetActiveVectors()
    {
        // Only query non-archived, high-priority vectors
        return await Documents.Where(d => !d.IsArchived).ToListAsync();
    }
}
