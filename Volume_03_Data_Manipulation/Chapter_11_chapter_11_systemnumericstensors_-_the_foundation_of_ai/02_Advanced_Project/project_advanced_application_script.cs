
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Book3_Chapter11_AdvancedApp
{
    // 1. Domain Model: Represents a raw data point from our source.
    // We use a record for immutability, aligning with functional principles.
    public record ReviewData(
        int Id, 
        string Text, 
        double Rating, 
        DateTime Timestamp, 
        bool IsVerified
    );

    // 2. Domain Model: Represents the processed data ready for AI embedding.
    public record ProcessedReview(
        int Id,
        string NormalizedText,
        double ScaledRating, // 0.0 to 1.0
        double TimeOfDay     // Normalized hour of day (0.0 to 1.0)
    );

    public class DataPipeline
    {
        // 3. Simulated Data Source: In a real app, this comes from a database or file.
        // We return IEnumerable to enable deferred execution.
        public static IEnumerable<ReviewData> GetRawReviews()
        {
            // Yielding individual records to simulate a stream
            yield return new ReviewData(1, "Great product!", 5.0, DateTime.Now.AddHours(-2), true);
            yield return new ReviewData(2, null, 2.0, DateTime.Now.AddHours(-5), false); // Invalid: Null text
            yield return new ReviewData(3, "  Okay...  ", 3.5, DateTime.Now.AddHours(-10), true);
            yield return new ReviewData(4, "Terrible.", 1.0, DateTime.Now.AddHours(-24), true);
            yield return new ReviewData(5, "Amazing value", 5.0, DateTime.Now.AddHours(-30), false);
            yield return new ReviewData(6, "", 0.0, DateTime.Now.AddHours(-35), true); // Invalid: Empty text
            yield return new ReviewData(7, "Just okay", 3.0, DateTime.Now.AddHours(-40), true);
            yield return new ReviewData(8, "Worth every penny", 4.5, DateTime.Now.AddHours(-48), true);
        }

        public static void RunPipeline()
        {
            Console.WriteLine("--- Starting Functional Data Pipeline ---");

            // 4. Step 1: Define the Source (Deferred Execution)
            // At this point, no data is processed. 'rawSource' is just a query definition.
            var rawSource = GetRawReviews();
            Console.WriteLine($"Query defined. Total items in source (deferred): {rawSource.Count()}");

            // 5. Step 2: Filtering (Where)
            // We remove invalid data. This is a declarative filter.
            // Note: We chain methods. This is still deferred.
            var validData = rawSource
                .Where(r => !string.IsNullOrWhiteSpace(r.Text)) // Filter out null/empty text
                .Where(r => r.Rating >= 0.0 && r.Rating <= 5.0); // Validate rating range

            // 6. Step 3: Transformation (Select)
            // We map raw data to the 'ProcessedReview' structure.
            // We calculate derived values (scaling) inside the lambda.
            // CRITICAL: No side effects. We do not modify external variables here.
            var transformedData = validData.Select(r => 
            {
                // Normalize Rating: 0-5 -> 0-1
                double scaledRating = r.Rating / 5.0;

                // Normalize Time: Extract hour (0-23) -> 0-1
                double timeOfDay = r.Timestamp.Hour / 24.0;

                // Clean Text: Trim whitespace
                string cleanText = r.Text.Trim();

                return new ProcessedReview(
                    r.Id,
                    cleanText,
                    scaledRating,
                    timeOfDay
                );
            });

            // 7. Step 4: Shuffling (Ordering)
            // AI models should not learn sequence order. We randomize the stream.
            // We use a Guid (random value) to sort the data.
            // NOTE: OrderBy is a "Deferred" operation but requires evaluating the key selector.
            var shuffledData = transformedData
                .OrderBy(r => Guid.NewGuid()); 

            // 8. Step 5: Immediate Execution (ToList)
            // Until now, nothing was calculated. The pipeline is a blueprint.
            // .ToList() forces execution of the entire chain (Filter -> Transform -> Shuffle).
            // This is "Immediate Execution".
            var finalDataset = shuffledData.ToList();

            Console.WriteLine($"Pipeline executed. Items processed: {finalDataset.Count}");
            Console.WriteLine("--------------------------------------------------");

            // 9. Step 6: Display Results (Side Effects allowed ONLY here, outside the query)
            // We iterate over the materialized list.
            foreach (var item in finalDataset)
            {
                Console.WriteLine($"ID: {item.Id} | Text: '{item.NormalizedText}' | Scaled Rating: {item.ScaledRating:F2} | Time Norm: {item.TimeOfDay:F2}");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Execute the pipeline
            DataPipeline.RunPipeline();
        }
    }
}
