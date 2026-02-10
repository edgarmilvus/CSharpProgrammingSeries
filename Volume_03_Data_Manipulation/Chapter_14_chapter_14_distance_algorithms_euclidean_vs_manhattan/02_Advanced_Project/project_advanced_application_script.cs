
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

// Advanced Application Script: Customer Segmentation Pipeline
// Context: An e-commerce platform needs to segment customers based on purchasing behavior.
// Problem: Identify distinct customer groups for targeted marketing campaigns.
// Solution: Use vector representations of customer data (spending, frequency, recency)
// and apply distance algorithms to find natural clusters.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataManipulation
{
    // Define a custom vector type for customer data
    // This encapsulates the multi-dimensional nature of our features
    public struct CustomerVector
    {
        public string CustomerId { get; }
        public double MonthlySpend { get; }      // Feature 1: Average monthly spending ($)
        public double PurchaseFrequency { get; } // Feature 2: Purchases per month
        public double RecencyScore { get; }      // Feature 3: Days since last purchase (normalized 0-1)
        
        // Constructor to create immutable vectors
        public CustomerVector(string id, double spend, double freq, double recency)
        {
            CustomerId = id;
            MonthlySpend = spend;
            PurchaseFrequency = freq;
            RecencyScore = recency;
        }
        
        // Return values as an array for distance calculations
        // This enables generic distance algorithms without hardcoding properties
        public double[] ToArray() => new[] { MonthlySpend, PurchaseFrequency, RecencyScore };
    }

    public static class DistanceAlgorithms
    {
        // Euclidean Distance: sqrt(sum((x_i - y_i)^2))
        // Geometric interpretation: Straight-line distance in n-dimensional space
        // Best for: Continuous data, natural clusters, when magnitude matters
        public static double Euclidean(double[] vectorA, double[] vectorB)
        {
            if (vectorA.Length != vectorB.Length)
                throw new ArgumentException("Vectors must have same dimensionality");
            
            // Using LINQ for functional aggregation
            // Deferred execution: The query isn't executed until .Sum() is called
            var squaredDifferences = vectorA
                .Select((value, index) => Math.Pow(value - vectorB[index], 2));
            
            // Immediate execution: .Sum() triggers calculation
            return Math.Sqrt(squaredDifferences.Sum());
        }

        // Manhattan Distance: sum(|x_i - y_i|)
        // Geometric interpretation: Grid-like path distance (city blocks)
        // Best for: Discrete/categorical data, high dimensions, when outliers matter
        public static double Manhattan(double[] vectorA, double[] vectorB)
        {
            if (vectorA.Length != vectorB.Length)
                throw new ArgumentException("Vectors must have same dimensionality");
            
            // Absolute differences using LINQ
            return vectorA
                .Select((value, index) => Math.Abs(value - vectorB[index]))
                .Sum();
        }
    }

    public static class DataPreprocessing
    {
        // Normalization: Scale features to [0, 1] range
        // Critical for distance algorithms: prevents high-magnitude features from dominating
        public static List<CustomerVector> NormalizeFeatures(List<CustomerVector> rawData)
        {
            // Extract raw values for each feature using projection
            var monthlySpends = rawData.Select(c => c.MonthlySpend).ToList();
            var frequencies = rawData.Select(c => c.PurchaseFrequency).ToList();
            var recencies = rawData.Select(c => c.RecencyScore).ToList();

            // Calculate min/max for each feature (immediate execution)
            double minSpend = monthlySpends.Min();
            double maxSpend = monthlySpends.Max();
            double minFreq = frequencies.Min();
            double maxFreq = frequencies.Max();
            double minRecency = recencies.Min();
            double maxRecency = recencies.Max();

            // Handle edge case: constant values (division by zero)
            double Normalize(double value, double min, double max)
            {
                if (max - min < 1e-10) return 0.5; // Return midpoint if no variance
                return (value - min) / (max - min);
            }

            // Transform data using functional pipeline
            // Pure function: No side effects, returns new collection
            return rawData
                .Select(c => new CustomerVector(
                    c.CustomerId,
                    Normalize(c.MonthlySpend, minSpend, maxSpend),
                    Normalize(c.PurchaseFrequency, minFreq, maxFreq),
                    Normalize(c.RecencyScore, minRecency, maxRecency)
                ))
                .ToList(); // Immediate execution: materialize results
        }

        // Shuffle data for unbiased sampling and k-fold cross-validation
        // Fisher-Yates shuffle implemented via LINQ (functional approach)
        public static List<T> Shuffle<T>(IEnumerable<T> source, Random rng)
        {
            // Deferred execution: The query isn't executed until .ToList()
            var shuffled = source
                .Select(x => new { Value = x, Order = rng.NextDouble() }) // Assign random key
                .OrderBy(x => x.Order)                                     // Sort by random key
                .Select(x => x.Value);                                     // Project back to original type
            
            return shuffled.ToList(); // Immediate execution
        }

        // Split data into training and testing sets
        // Returns a tuple of two lists (functional decomposition)
        public static (List<T> Train, List<T> Test) TrainTestSplit<T>(
            List<T> data, double trainRatio = 0.7, int seed = 42)
        {
            var rng = new Random(seed);
            var shuffled = Shuffle(data, rng);
            
            int trainSize = (int)(data.Count * trainRatio);
            
            // Partition using Skip and Take (functional alternative to loops)
            var train = shuffled.Take(trainSize).ToList();
            var test = shuffled.Skip(trainSize).ToList();
            
            return (train, test);
        }
    }

    public class CustomerClusterer
    {
        // K-Means clustering using Euclidean distance
        // Returns cluster assignments (centroid index) for each data point
        public static List<int> KMeansEuclidean(
            List<CustomerVector> data, int k, int maxIterations = 100)
        {
            if (data.Count == 0) return new List<int>();
            if (k <= 0 || k > data.Count) throw new ArgumentException("Invalid k value");

            var rng = new Random(42);
            
            // Initialize centroids randomly from data points
            // Using .OrderBy(x => rng.Next()) for functional random selection
            var centroids = data
                .OrderBy(x => rng.Next())
                .Take(k)
                .Select(c => c.ToArray())
                .ToList();

            // Iterative clustering loop
            // While imperative, this is necessary for k-means convergence
            // Could be replaced with recursive LINQ in a purely functional approach
            List<int> assignments = Enumerable.Repeat(0, data.Count).ToList();
            bool changed = true;
            int iteration = 0;

            while (changed && iteration < maxIterations)
            {
                changed = false;
                iteration++;

                // Assignment step: For each data point, find closest centroid
                // Using PLINQ for parallel computation (allowed for heavy tasks)
                var newAssignments = data
                    .AsParallel() // Parallel LINQ for performance
                    .Select((point, index) =>
                    {
                        // Find minimum distance and corresponding centroid index
                        var distances = centroids
                            .Select((centroid, centroidIndex) => new
                            {
                                Index = centroidIndex,
                                Distance = DistanceAlgorithms.Euclidean(point.ToArray(), centroid)
                            });
                        
                        return distances.Aggregate(
                            (min, current) => current.Distance < min.Distance ? current : min
                        ).Index;
                    })
                    .ToList();

                // Check for changes (immediate execution of comparison)
                changed = !newAssignments.SequenceEqual(assignments);
                assignments = newAssignments;

                // Update centroids: Calculate mean of each cluster
                // GroupBy is a powerful LINQ operator for aggregation
                centroids = data
                    .Select((point, index) => new { Point = point, Cluster = assignments[index] })
                    .GroupBy(x => x.Cluster)
                    .Select(group =>
                    {
                        // Calculate mean for each dimension
                        var pointsInCluster = group.Select(x => x.Point.ToArray()).ToList();
                        int dimensions = pointsInCluster[0].Length;
                        
                        var meanVector = new double[dimensions];
                        for (int d = 0; d < dimensions; d++)
                        {
                            // Pure functional aggregation
                            meanVector[d] = pointsInCluster.Average(p => p[d]);
                        }
                        return meanVector;
                    })
                    .ToList();
            }

            return assignments;
        }

        // Find optimal k using the Elbow Method
        // Calculates sum of squared errors (SSE) for different k values
        public static Dictionary<int, double> FindOptimalK(
            List<CustomerVector> data, int maxK = 10)
        {
            // Generate k values from 2 to maxK
            var kValues = Enumerable.Range(2, maxK - 1);
            
            // Parallel computation for performance
            var sseResults = kValues
                .AsParallel()
                .Select(k =>
                {
                    // Run k-means
                    var assignments = KMeansEuclidean(data, k);
                    
                    // Calculate SSE: sum of squared distances to assigned centroid
                    // Group data by cluster
                    var grouped = data
                        .Select((point, index) => new { Point = point, Cluster = assignments[index] })
                        .GroupBy(x => x.Cluster);
                    
                    double sse = 0;
                    foreach (var group in grouped)
                    {
                        // Get centroid for this cluster
                        var points = group.Select(x => x.Point.ToArray()).ToList();
                        var centroid = points
                            .SelectMany(p => p.Select((val, idx) => new { val, idx }))
                            .GroupBy(x => x.idx)
                            .Select(g => g.Average(x => x.val))
                            .ToArray();
                        
                        // Sum squared distances
                        foreach (var point in points)
                        {
                            double dist = DistanceAlgorithms.Euclidean(point, centroid);
                            sse += dist * dist;
                        }
                    }
                    
                    return new { K = k, SSE = sse };
                })
                .ToDictionary(x => x.K, x => x.SSE);
            
            return sseResults;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== Customer Segmentation Pipeline ===\n");

            // Step 1: Generate synthetic customer data
            // Simulating real-world e-commerce dataset
            var rawCustomers = new List<CustomerVector>
            {
                new CustomerVector("C001", 1200, 8, 0.9),  // High-value, frequent, recent
                new CustomerVector("C002", 50, 1, 0.2),    // Low-value, infrequent, old
                new CustomerVector("C003", 1100, 7, 0.85), // Similar to C001
                new CustomerVector("C004", 200, 3, 0.6),   // Medium-value
                new CustomerVector("C005", 45, 1, 0.1),    // Similar to C002
                new CustomerVector("C006", 1300, 9, 0.95), // Similar to C001
                new CustomerVector("C007", 180, 2, 0.5),   // Similar to C004
                new CustomerVector("C008", 800, 5, 0.7),   // Medium-high value
                new CustomerVector("C009", 60, 1, 0.3),    // Similar to C002
                new CustomerVector("C010", 950, 6, 0.75),  // Similar to C001
                new CustomerVector("C011", 220, 3, 0.55),  // Similar to C004
                new CustomerVector("C012", 40, 1, 0.15),   // Similar to C002
                new CustomerVector("C013", 1150, 8, 0.88), // Similar to C001
                new CustomerVector("C014", 190, 2, 0.45),  // Similar to C004
                new CustomerVector("C015", 750, 4, 0.65),  // Similar to C008
            };

            Console.WriteLine($"Generated {rawCustomers.Count} customer records.");
            Console.WriteLine($"Features: MonthlySpend, PurchaseFrequency, RecencyScore\n");

            // Step 2: Preprocessing pipeline
            // Normalize features to ensure equal contribution to distance metrics
            Console.WriteLine("--- Preprocessing ---");
            var normalizedData = DataPreprocessing.NormalizeFeatures(rawCustomers);
            Console.WriteLine($"Normalized {normalizedData.Count} records to [0, 1] range.\n");

            // Step 3: Train/Test Split (70/30)
            // Demonstrates deferred execution vs immediate execution
            Console.WriteLine("--- Data Splitting (Deferred vs Immediate) ---");
            var split = DataPreprocessing.TrainTestSplit(normalizedData, 0.7, 42);
            
            Console.WriteLine($"Training set size: {split.Train.Count}");
            Console.WriteLine($"Test set size: {split.Test.Count}\n");

            // Step 4: Distance Algorithm Comparison
            // Compare Euclidean vs Manhattan on a sample pair
            Console.WriteLine("--- Distance Algorithm Comparison ---");
            var sampleA = split.Train[0].ToArray();
            var sampleB = split.Train[1].ToArray();

            double euclideanDist = DistanceAlgorithms.Euclidean(sampleA, sampleB);
            double manhattanDist = DistanceAlgorithms.Manhattan(sampleA, sampleB);

            Console.WriteLine($"Sample A: ({split.Train[0].MonthlySpend:F3}, {split.Train[0].PurchaseFrequency:F3}, {split.Train[0].RecencyScore:F3})");
            Console.WriteLine($"Sample B: ({split.Train[1].MonthlySpend:F3}, {split.Train[1].PurchaseFrequency:F3}, {split.Train[1].RecencyScore:F3})");
            Console.WriteLine($"Euclidean Distance: {euclideanDist:F4}");
            Console.WriteLine($"Manhattan Distance: {manhattanDist:F4}");
            Console.WriteLine($"Ratio (Euc/Man): {euclideanDist / manhattanDist:F4}\n");

            // Step 5: Find Optimal K using Elbow Method
            // Parallel computation for performance
            Console.WriteLine("--- Finding Optimal K (Elbow Method) ---");
            Console.WriteLine("Computing SSE for k = 2 to 10...");
            
            var sseResults = CustomerClusterer.FindOptimalK(split.Train, 10);
            
            // Display results in a formatted table
            Console.WriteLine("k\tSSE");
            Console.WriteLine(new string('-', 20));
            foreach (var kvp in sseResults.OrderBy(x => x.Key))
            {
                Console.WriteLine($"{kvp.Key}\t{kvp.Value:F2}");
            }

            // Find elbow point (simple heuristic: largest decrease in SSE)
            var sseDecreases = sseResults
                .OrderBy(x => x.Key)
                .Select((x, i) => new { K = x.Key, Decrease = i == 0 ? 0 : sseResults[x.Key - 1] - x.Value })
                .ToList();
            
            var optimalK = sseDecreases
                .OrderByDescending(x => x.Decrease)
                .First().K;
            
            Console.WriteLine($"\nOptimal K identified: {optimalK}\n");

            // Step 6: Apply K-Means Clustering
            // Using Euclidean distance for cluster assignment
            Console.WriteLine("--- Customer Segmentation (K-Means) ---");
            var clusterAssignments = CustomerClusterer.KMeansEuclidean(split.Train, optimalK);
            
            // Analyze cluster sizes
            var clusterStats = clusterAssignments
                .GroupBy(a => a)
                .Select(g => new { Cluster = g.Key, Count = g.Count() })
                .OrderBy(x => x.Cluster);
            
            Console.WriteLine("Cluster Distribution:");
            foreach (var stat in clusterStats)
            {
                Console.WriteLine($"  Cluster {stat.Cluster}: {stat.Count} customers");
            }

            // Step 7: Profile Clusters
            // Calculate average features per cluster
            Console.WriteLine("\nCluster Profiles (Averages):");
            var clusteredData = split.Train
                .Select((c, i) => new { Customer = c, Cluster = clusterAssignments[i] })
                .GroupBy(x => x.Cluster);
            
            foreach (var cluster in clusteredData.OrderBy(g => g.Key))
            {
                var customers = cluster.Select(x => x.Customer).ToList();
                double avgSpend = customers.Average(c => c.MonthlySpend);
                double avgFreq = customers.Average(c => c.PurchaseFrequency);
                double avgRecency = customers.Average(c => c.RecencyScore);
                
                Console.WriteLine($"  Cluster {cluster.Key}:");
                Console.WriteLine($"    Avg Monthly Spend: ${avgSpend * 1000:F0} (normalized)");
                Console.WriteLine($"    Avg Frequency: {avgFreq:F2} purchases/month");
                Console.WriteLine($"    Avg Recency: {avgRecency:F2} (0=old, 1=recent)");
            }

            // Step 8: Apply to Test Set (Prediction)
            // Assign new customers to existing clusters
            Console.WriteLine("\n--- Applying Model to Test Set ---");
            var testAssignments = CustomerClusterer.KMeansEuclidean(split.Test, optimalK);
            
            Console.WriteLine($"Assigned {split.Test.Count} test customers to clusters.");
            
            // Display test predictions
            Console.WriteLine("\nTest Set Predictions:");
            var testResults = split.Test
                .Select((c, i) => new { c.CustomerId, Cluster = testAssignments[i] })
                .Take(5); // Show first 5
            
            foreach (var result in testResults)
            {
                Console.WriteLine($"  {result.CustomerId} -> Cluster {result.Cluster}");
            }

            Console.WriteLine("\n=== Pipeline Complete ===");
            Console.WriteLine("Key Concepts Demonstrated:");
            Console.WriteLine("1. Custom Vector Type for multi-dimensional data");
            Console.WriteLine("2. Functional Preprocessing (Normalization, Shuffling, Splitting)");
            Console.WriteLine("3. Deferred vs Immediate Execution in LINQ");
            Console.WriteLine("4. Euclidean vs Manhattan Distance Comparison");
            Console.WriteLine("5. Parallel LINQ (PLINQ) for performance");
            Console.WriteLine("6. K-Means Clustering with Euclidean Distance");
            Console.WriteLine("7. Elbow Method for Optimal K Selection");
            Console.WriteLine("8. Pure Functional Programming (No side effects in queries)");
        }
    }
}
