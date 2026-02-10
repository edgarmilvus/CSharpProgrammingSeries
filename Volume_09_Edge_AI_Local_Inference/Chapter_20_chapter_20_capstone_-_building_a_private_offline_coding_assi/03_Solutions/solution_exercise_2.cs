
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Numerics.Tensors; // Requires System.Numerics.Tensors package
using System.Threading.Tasks;

public record CodeDocument(string Id, string FilePath, string CodeContent, float[] Embedding);

public class VectorStoreService
{
    private readonly List<CodeDocument> _documents = new();
    private readonly IEmbeddingGenerator _embeddingGenerator; // Abstraction for the ONNX model

    public VectorStoreService(IEmbeddingGenerator embeddingGenerator)
    {
        _embeddingGenerator = embeddingGenerator;
    }

    // 1 & 2. Load, Split, and Embed
    public async Task BuildIndexAsync(string directoryPath)
    {
        var files = Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories);
        
        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file);
            
            // Simple chunking strategy: Split by methods or large blocks
            // For this example, we treat the whole file as a chunk for brevity
            var chunks = new[] { content }; 

            foreach (var chunk in chunks)
            {
                // Generate embedding using the local ONNX model
                var embedding = await _embeddingGenerator.GenerateAsync(chunk);
                
                _documents.Add(new CodeDocument(
                    Id: Guid.NewGuid().ToString(),
                    FilePath: file,
                    CodeContent: chunk,
                    Embedding: embedding
                ));
            }
        }
    }

    // 3. Search with Cosine Similarity
    public IEnumerable<CodeDocument> Search(string query, int topK = 3)
    {
        if (!_documents.Any()) return Enumerable.Empty<CodeDocument>();

        // Embed the query
        var queryEmbedding = _embeddingGenerator.GenerateAsync(query).GetAwaiter().GetResult();
        
        // Convert to Vector<float> for Tensor operations
        var queryVector = new Vector<float>(queryEmbedding);

        // 4. Mathematical Nuance: Normalization for efficient Dot Product
        // Cosine Similarity = (A . B) / (|A| * |B|)
        // If vectors are unit length (normalized), Cosine Similarity = Dot Product.
        
        var scored = _documents.Select(doc => 
        {
            var docVector = new Vector<float>(doc.Embedding);
            
            // Check for zero-length vectors to avoid division by zero
            var magnitude = docVector.Length(); // Euclidean norm
            if (magnitude == 0) return (doc, Score: 0);

            // Calculate Dot Product (efficient)
            // Note: System.Numerics.Tensors provides DotProduct, but Vector<T> is often faster for standard sizes.
            // If dimensions are large, TensorPrimitives.Dot is preferred.
            var dotProduct = Vector.Dot(docVector, queryVector); 
            
            // We assume queryVector is normalized. If not, we normalize both here:
            // Score = dotProduct / (docMagnitude * queryMagnitude)
            // For simplicity in this snippet, we assume pre-normalized embeddings.
            return (doc, Score: dotProduct);
        })
        .OrderByDescending(x => x.Score)
        .Take(topK);

        return scored.Select(x => x.doc);
    }
}

// Singleton Registration Example (Extension Method)
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVectorStore(this IServiceCollection services)
    {
        return services.AddSingleton<VectorStoreService>();
    }
}

// Mock Interface for the Embedding Generator (to keep solution concise)
public interface IEmbeddingGenerator
{
    Task<float[]> GenerateAsync(string input);
}
