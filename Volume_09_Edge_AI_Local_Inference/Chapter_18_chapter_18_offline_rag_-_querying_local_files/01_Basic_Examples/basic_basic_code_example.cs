
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace LocalOfflineRag
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Local Offline RAG: Hello World ===\n");

            // 1. Setup: Define local file paths (Simulated local documents)
            string localDocsDir = Path.Combine(Path.GetTempPath(), "LocalRagDocs");
            Directory.CreateDirectory(localDocsDir);
            
            // Create dummy local files if they don't exist
            string doc1Path = Path.Combine(localDocsDir, "doc1.txt");
            string doc2Path = Path.Combine(localDocsDir, "doc2.txt");
            
            if (!File.Exists(doc1Path))
                File.WriteAllText(doc1Path, "The capital of France is Paris. It is known for the Eiffel Tower.");
            if (!File.Exists(doc2Path))
                File.WriteAllText(doc2Path, "The capital of Japan is Tokyo. It is known for its bustling streets and technology.");

            // 2. Load Local Documents
            var documents = LoadDocuments(localDocsDir);
            Console.WriteLine($"Loaded {documents.Count} documents from local storage.");

            // 3. Initialize Embedding Engine (Simulated ONNX Inference)
            // In a real scenario, you would load a specific ONNX model like 'all-MiniLM-L6-v2.onnx'
            var embeddingEngine = new LocalEmbeddingEngine();

            // 4. Generate Embeddings for Documents (Offline)
            // We convert text into vector representations
            var documentVectors = new List<VectorEntry>();
            foreach (var doc in documents)
            {
                var vector = embeddingEngine.GetEmbedding(doc.Content);
                documentVectors.Add(new VectorEntry { Text = doc.Content, Vector = vector });
                Console.WriteLine($"Generated embedding for: {doc.FileName}");
            }

            // 5. User Query
            string userQuery = "What is the capital of France?";
            Console.WriteLine($"\nUser Query: \"{userQuery}\"");

            // 6. Generate Query Embedding
            var queryVector = embeddingEngine.GetEmbedding(userQuery);

            // 7. Perform Semantic Search (Cosine Similarity)
            var relevantContext = SemanticSearch(documentVectors, queryVector, topK: 1);

            // 8. Construct Prompt for Local LLM
            string prompt = $@"
You are a helpful assistant. Answer the question based ONLY on the provided context.

Context:
{relevantContext}

Question: {userQuery}
Answer:";

            // 9. Run Local LLM Inference (Simulated ONNX Generation)
            // In a real scenario, you would use a decoder model like Phi-2 or Llama
            var llmEngine = new LocalLlmEngine();
            string response = llmEngine.Generate(prompt);

            Console.WriteLine($"\nLocal LLM Response:\n{response}");
        }

        // --- Helper Methods & Classes ---

        static List<Document> LoadDocuments(string directoryPath)
        {
            var docs = new List<Document>();
            foreach (var file in Directory.GetFiles(directoryPath))
            {
                docs.Add(new Document
                {
                    FileName = Path.GetFileName(file),
                    Content = File.ReadAllText(file)
                });
            }
            return docs;
        }

        static string SemanticSearch(List<VectorEntry> documentVectors, float[] queryVector, int topK)
        {
            // Calculate Cosine Similarity between query and each document
            var scores = documentVectors.Select(doc => new
            {
                Text = doc.Text,
                Score = CosineSimilarity(doc.Vector, queryVector)
            })
            .OrderByDescending(x => x.Score)
            .Take(topK);

            // Combine top results into context
            StringBuilder contextBuilder = new StringBuilder();
            foreach (var item in scores)
            {
                contextBuilder.AppendLine($"- {item.Text} (Relevance: {item.Score:P})");
            }
            
            return contextBuilder.ToString();
        }

        static float CosineSimilarity(float[] vecA, float[] vecB)
        {
            float dotProduct = 0f;
            float magnitudeA = 0f;
            float magnitudeB = 0f;

            for (int i = 0; i < vecA.Length; i++)
            {
                dotProduct += vecA[i] * vecB[i];
                magnitudeA += vecA[i] * vecA[i];
                magnitudeB += vecB[i] * vecB[i];
            }

            magnitudeA = (float)Math.Sqrt(magnitudeA);
            magnitudeB = (float)Math.Sqrt(magnitudeB);

            if (magnitudeA == 0 || magnitudeB == 0) return 0;
            return dotProduct / (magnitudeA * magnitudeB);
        }
    }

    // --- Simulated ONNX Embedding Engine ---
    // Real implementation would use: InferenceSession.Run() on an ONNX embedding model
    public class LocalEmbeddingEngine
    {
        // Simulating a 384-dimensional embedding model (e.g., all-MiniLM-L6-v2)
        private const int Dimension = 384;
        private Random _rng = new Random(42); // Fixed seed for reproducibility

        public float[] GetEmbedding(string text)
        {
            // In a real ONNX implementation:
            // 1. Tokenize text
            // 2. Create InputTensor
            // 3. session.Run(inputs)
            // 4. Extract output tensor
            
            // SIMULATION: We generate a deterministic vector based on string hashing
            // to simulate semantic similarity for this demo.
            float[] vector = new float[Dimension];
            int hash = text.GetHashCode();
            
            // Fill vector with pseudo-random values based on hash
            _rng = new Random(hash); 
            for (int i = 0; i < Dimension; i++)
            {
                vector[i] = (float)_rng.NextDouble();
            }
            
            // Normalize vector (L2 norm)
            double sumSquares = 0;
            foreach (var val in vector) sumSquares += val * val;
            double norm = Math.Sqrt(sumSquares);
            for (int i = 0; i < Dimension; i++)
            {
                vector[i] = (float)(vector[i] / norm);
            }

            return vector;
        }
    }

    // --- Simulated ONNX LLM Engine ---
    // Real implementation would use: InferenceSession.Run() with past_key_values
    public class LocalLlmEngine
    {
        public string Generate(string prompt)
        {
            // In a real ONNX implementation:
            // 1. Tokenize prompt
            // 2. Initialize empty input_ids
            // 3. Loop: Run model, get logits, sample next token, append to input_ids
            // 4. Detokenize output
            
            // SIMULATION: Rule-based response for the demo context
            if (prompt.Contains("capital of France") || prompt.Contains("Paris"))
            {
                return "Based on the retrieved context, the capital of France is Paris.";
            }
            return "I do not have enough context to answer that question.";
        }
    }

    // --- Data Models ---
    public class Document
    {
        public string FileName { get; set; }
        public string Content { get; set; }
    }

    public class VectorEntry
    {
        public string Text { get; set; }
        public float[] Vector { get; set; }
    }
}
