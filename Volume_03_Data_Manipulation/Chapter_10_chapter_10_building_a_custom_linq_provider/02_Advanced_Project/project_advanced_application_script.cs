
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

// =============================================================================
//  REAL-WORLD CONTEXT: AI EMBEDDING SEARCH ENGINE PRE-PROCESSING
// =============================================================================
//
// Problem: We are building a semantic search engine for a document database.
//          Documents are represented by "Embeddings" (high-dimensional vectors).
//          Before we can search, we must prepare the data.
//
//          1.  Cleaning:   Remove low-quality or irrelevant documents.
//          2.  Normalizing: Ensure all vectors are on the same scale (unit length).
//          3.  Shuffling:  Randomize order to prevent bias during training or batch processing.
//
// Solution: We will use a custom LINQ Provider to translate high-level queries
//           into optimized vector operations, specifically "Cosine Similarity".
//
//           This script demonstrates the "Advanced Application" of building
//           the provider and using it to solve the problem functionally.
// =============================================================================

namespace AdvancedLinqProvider
{
    // -------------------------------------------------------------------------
    //  STEP 1: Domain Models (The Data)
    // -------------------------------------------------------------------------
    // We represent a document as a Vector with a Value (the coordinates) and Metadata.
    // Note: In a real system, 'Value' would be a float[] or double[].
    //       For this educational example, we use a List<double> to be explicit.

    public class Vector
    {
        public List<double> Value { get; set; }
    }

    public class Document
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public Vector Embedding { get; set; }
        public double QualityScore { get; set; } // Used for cleaning
    }

    // -------------------------------------------------------------------------
    //  STEP 2: The Custom LINQ Provider Architecture
    // -------------------------------------------------------------------------
    // To teach Deferred Execution and Expression Trees, we need a custom source.
    // This class mimics how a database driver (like Entity Framework) works.
    
    // A. The Queryable Source
    public class DocumentCollection : IQueryable<Document>
    {
        public DocumentCollection(IEnumerable<Document> data)
        {
            Provider = new VectorQueryProvider(data);
            Expression = Expression.Constant(this);
        }

        // Internal constructor for composition (Select/Where results)
        internal DocumentCollection(VectorQueryProvider provider, Expression expression)
        {
            Provider = provider;
            Expression = expression;
        }

        public Type ElementType => typeof(Document);
        public Expression Expression { get; }
        public IQueryProvider Provider { get; }
        public IEnumerator<Document> GetEnumerator() => Provider.Execute<IEnumerable<Document>>(Expression).GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    // B. The Query Provider (The Translator)
    public class VectorQueryProvider : IQueryProvider
    {
        private readonly IEnumerable<Document> _sourceData;

        public VectorQueryProvider(IEnumerable<Document> sourceData)
        {
            _sourceData = sourceData;
        }

        // CREATE: Builds the query structure (Deferred Execution happens here)
        public IQueryable CreateQuery(Expression expression)
        {
            // We infer the generic type (usually Document or a projection)
            Type elementType = expression.Type.GetGenericArguments()[0];
            return (IQueryable)Activator.CreateInstance(typeof(DocumentCollection<>).MakeGenericType(elementType), this, expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new DocumentCollection<TElement>(this, expression);
        }

        // EXECUTE: Runs the query (Immediate Execution happens here)
        public object Execute(Expression expression)
        {
            // This is where we interpret the Expression Tree and run the logic.
            return ExecuteSequence(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            // Unwrap the result
            return (TResult)Execute(expression);
        }

        // The Core Logic: Interpreting the Expression Tree
        private object ExecuteSequence(Expression expression)
        {
            // 1. Detect if it's a MethodCall (Where, Select, etc.)
            if (expression is MethodCallExpression methodCall)
            {
                var methodName = methodCall.Method.Name;

                // 2. Handle Where (Filtering / Cleaning)
                if (methodName == "Where")
                {
                    // Get the source (the previous collection)
                    var source = ExecuteSequence(methodCall.Arguments[0]) as IEnumerable<Document>;
                    
                    // Get the lambda condition (e.g., d => d.QualityScore > 0.8)
                    // We compile it to a delegate to run it in memory for this example.
                    var lambda = (LambdaExpression)StripQuotes(methodCall.Arguments[1]);
                    var predicate = (Func<Document, bool>)lambda.Compile();

                    // Return the filtered sequence
                    return source.Where(predicate);
                }

                // 3. Handle Select (Projection / Normalization)
                if (methodName == "Select")
                {
                    var source = ExecuteSequence(methodCall.Arguments[0]) as IEnumerable<Document>;
                    var lambda = (LambdaExpression)StripQuotes(methodCall.Arguments[1]);
                    var selector = (Func<Document, Document>)lambda.Compile(); // Simplified for this demo

                    return source.Select(selector);
                }

                // 4. Handle OrderBy (Shuffling / Randomization)
                if (methodName == "OrderBy" || methodName == "OrderByDescending")
                {
                    var source = ExecuteSequence(methodCall.Arguments[0]) as IEnumerable<Document>;
                    // Note: In a real provider, we wouldn't compile a random delegate, 
                    // we would translate the key selector. Here we simulate the shuffle.
                    return source.OrderBy(x => Guid.NewGuid()); 
                }

                // 5. Handle ToList (Immediate Execution Trigger)
                if (methodName == "ToList")
                {
                    var source = ExecuteSequence(methodCall.Arguments[0]) as IEnumerable<Document>;
                    return source.ToList();
                }

                // 6. Handle Vector Similarity (The "Advanced" Feature)
                // We look for a custom extension method we defined elsewhere, 
                // but here we intercept it by name for the example.
                if (methodName == "OrderByCosineDistance")
                {
                    // Arguments: 0 = source, 1 = query vector
                    var source = ExecuteSequence(methodCall.Arguments[0]) as IEnumerable<Document>;
                    var lambda = (LambdaExpression)StripQuotes(methodCall.Arguments[1]);
                    var targetVector = ((ConstantExpression)lambda.Body).Value as Vector;

                    // Perform the Cosine Distance Calculation
                    return source.OrderBy(d => CalculateCosineDistance(d.Embedding, targetVector));
                }
            }

            // 7. Handle Constant (The starting point: new DocumentCollection(...))
            if (expression is ConstantExpression constant)
            {
                if (constant.Value is DocumentCollection coll) return coll;
                if (constant.Value is IEnumerable<Document> list) return list;
            }

            // 8. Handle New (Projections to new objects)
            if (expression is NewExpression newExp)
            {
                // This is a simplified fallback for complex projections
                return _sourceData; 
            }

            throw new NotSupportedException($"Expression type not handled: {expression.NodeType}");
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }

        // The Math: Cosine Similarity = (A . B) / (||A|| * ||B||)
        private double CalculateCosineDistance(Vector a, Vector b)
        {
            if (a.Value.Count != b.Value.Count) throw new ArgumentException("Vectors must be same dimension");

            double dotProduct = 0;
            double normA = 0;
            double normB = 0;

            for (int i = 0; i < a.Value.Count; i++)
            {
                dotProduct += a.Value[i] * b.Value[i];
                normA += a.Value[i] * a.Value[i];
                normB += b.Value[i] * b.Value[i];
            }

            // We return Distance (1 - Similarity) for sorting
            double similarity = dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
            return 1.0 - similarity;
        }
    }

    // Helper generic wrapper for IQueryable
    public class DocumentCollection<T> : IQueryable<T>
    {
        public DocumentCollection(VectorQueryProvider provider, Expression expression)
        {
            Provider = provider;
            Expression = expression;
        }
        public Type ElementType => typeof(T);
        public Expression Expression { get; }
        public IQueryProvider Provider { get; }
        public IEnumerator<T> GetEnumerator() => Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    // -------------------------------------------------------------------------
    //  STEP 3: Extension Methods (The LINQ Syntax)
    // -------------------------------------------------------------------------
    // These allow us to use the "from x in ..." or "source.Where(...)" syntax.
    // We define a custom operator for Vector Similarity.

    public static class VectorExtensions
    {
        // This method signature matches the pattern the Provider looks for.
        // It allows the Provider to intercept the call.
        public static IQueryable<TSource> OrderByCosineDistance<TSource>(
            this IQueryable<TSource> source, 
            Expression<Func<TSource, Vector>> vectorSelector)
        {
            // We don't execute here. We return an IQueryable.
            // The Provider's Execute method will handle the actual math.
            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)),
                    source.Expression,
                    Expression.Quote(vectorSelector)
                )
            );
        }
    }

    // =============================================================================
    //  THE SCRIPT EXECUTION
    // =============================================================================

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("--- Starting Advanced Data Preprocessing Pipeline ---\n");

            // 1. RAW DATA GENERATION (Simulated)
            // We create a dataset of 100 documents with random embeddings.
            // In a real scenario, this comes from a file or database.
            var rawData = GenerateMockData(100);

            // 2. THE QUERY DEFINITION (Declarative / Functional)
            // We define the pipeline. NO CODE RUNS YET. This is Deferred Execution.
            
            var query = rawData
                // A. CLEANING: Remove documents with low quality scores (< 0.5)
                //    The Provider will intercept 'Where' and filter the list.
                .Where(d => d.QualityScore > 0.5)

                // B. NORMALIZATION: We will project the documents into a normalized view.
                //    (In this demo, we simulate normalization by just keeping the object,
                //     but conceptually we are transforming the vector).
                .Select(d => new Document 
                { 
                    Id = d.Id, 
                    Content = d.Content, 
                    Embedding = new Vector { Value = d.Embedding.Value.Select(v => v / 2.0).ToList() }, // Scaling
                    QualityScore = d.QualityScore 
                })

                // C. SHUFFLING: Randomize order to prevent bias.
                //    The Provider intercepts OrderBy and applies a random key.
                .OrderBy(d => d.Id) // We use a placeholder here, provider handles random logic internally for this example

                // D. VECTOR SEARCH: Find documents closest to a query vector.
                //    This is the custom LINQ provider feature.
                //    We define a "Query Vector" (what the user is searching for).
                .OrderByCosineDistance(d => d.Embedding);

            // E. IMMEDIATE EXECUTION
            //    We call ToList(). This forces the Provider to run the pipeline.
            //    If we didn't call ToList(), the query would just be an expression tree object.
            var processedData = query.ToList();

            // 3. RESULTS
            Console.WriteLine($"Pipeline Complete. Processed {processedData.Count} documents.");
            Console.WriteLine("Top 5 Results (Closest to Query Vector):");
            
            // We use a simple loop here ONLY because we are displaying output.
            // The data processing itself was pure LINQ.
            foreach (var doc in processedData.Take(5))
            {
                Console.WriteLine($" - ID: {doc.Id} | Quality: {doc.QualityScore:F2} | Vector Norm: {Math.Sqrt(doc.Embedding.Value.Sum(v => v * v)):F4}");
            }

            // 4. DEMONSTRATING DEFERRED EXECUTION
            Console.WriteLine("\n--- Demonstrating Deferred Execution ---");
            
            // We define the query again, but don't assign it to a variable immediately.
            // This is just the blueprint.
            var deferredQuery = rawData.Where(d => d.QualityScore > 0.8);

            Console.WriteLine("Query defined. No processing done yet.");
            Console.WriteLine($"Raw Data Count: {rawData.Count}");
            
            // Let's change the raw data!
            // If execution was immediate, this wouldn't affect the result.
            // Because it's deferred, the query will look at the modified data when run.
            rawData.RemoveAt(0); 
            Console.WriteLine($"Raw Data Count after removal: {rawData.Count}");

            // NOW we execute
            var result = deferredQuery.ToList();
            Console.WriteLine($"Result Count (should reflect removal): {result.Count}");
        }

        // Helper to generate mock data
        static List<Document> GenerateMockData(int count)
        {
            var rand = new Random();
            var list = new List<Document>();
            for (int i = 0; i < count; i++)
            {
                list.Add(new Document
                {
                    Id = i,
                    Content = $"Doc {i}",
                    QualityScore = rand.NextDouble(), // 0.0 to 1.0
                    Embedding = new Vector
                    {
                        // Generate 3D vectors for simplicity
                        Value = new List<double> { rand.NextDouble(), rand.NextDouble(), rand.NextDouble() }
                    }
                });
            }
            return list;
        }
    }
}
