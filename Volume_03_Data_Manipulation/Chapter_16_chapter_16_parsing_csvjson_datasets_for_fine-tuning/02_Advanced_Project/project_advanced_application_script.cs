
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
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace HighPerformanceAI
{
    /*
     * PROBLEM CONTEXT: 
     * We are preparing data for fine-tuning an AI model that predicts energy consumption.
     * The raw dataset is a massive CSV file (simulated here in memory) containing thousands of sensor readings.
     * Each row has: SensorID (int), Temperature (float), Humidity (float), and Timestamp (long).
     * 
     * CONSTRAINTS:
     * 1. The dataset is too large to fit entirely on the Heap in multiple copies (memory pressure).
     * 2. We need to normalize the data (scale Temperature and Humidity) and calculate a weighted feature vector
     *    for the model input. This must be done with maximum throughput.
     * 3. We cannot use LINQ or standard `string.Split` because they generate garbage (allocations) on every row,
     *    which would trigger the Garbage Collector (GC) constantly, stalling the training pipeline.
     */

    public class MemoryEfficientCsvParser
    {
        // SIMULATION: A buffer representing a CSV file loaded into memory.
        // In a real scenario, this might be a memory-mapped file or a network stream buffer.
        private const string RawCsvData = 
            "101,22.5,45.0,1672531200\n" +
            "102,23.1,44.2,1672531260\n" +
            "103,21.8,46.1,1672531320\n" +
            "104,24.0,43.5,1672531380";

        public static void ProcessDataset()
        {
            Console.WriteLine("--- Starting High-Performance CSV Parsing ---");
            
            // 1. MEMORY ALLOCATION STRATEGY: STACK vs HEAP
            // We convert the static string to a ReadOnlySpan<char>.
            // Span is a 'ref struct', meaning it lives on the STACK (or in CPU registers).
            // It does NOT allocate memory on the Heap. It is a "view" into existing memory.
            ReadOnlySpan<char> buffer = RawCsvData.AsSpan();

            // We use a small stack-allocated buffer for parsing individual numbers.
            // 'stackalloc' allocates memory on the stack, which is automatically reclaimed when the method exits.
            // This is ZERO allocation overhead. Safe for small buffers (e.g., 64 chars for a float string).
            Span<char> numberBuffer = stackalloc char[64];

            // 2. PARSING LOOP
            // We iterate through the buffer manually. No iterators, no LINQ.
            int position = 0;
            while (position < buffer.Length)
            {
                // Find the end of the current line
                int lineEnd = buffer.Slice(position).IndexOf('\n');
                if (lineEnd == -1) lineEnd = buffer.Length - position;
                
                // Get the current line as a Span
                ReadOnlySpan<char> line = buffer.Slice(position, lineEnd);
                
                // Parse the line
                ParseAndProcessLine(line, numberBuffer);
                
                // Move to the next line (+1 for the newline character)
                position += lineEnd + 1;
            }
        }

        // 3. ZERO-ALLOCATION PARSING LOGIC
        // We pass 'numberBuffer' as a writable Span to reuse the same stack memory for every number.
        private static void ParseAndProcessLine(ReadOnlySpan<char> line, Span<char> numberBuffer)
        {
            // We need to extract 4 fields: ID, Temp, Humidity, Timestamp.
            // We use indices to slice the line without copying data.
            int start = 0;
            
            // Field 1: SensorID (int)
            int commaIndex = line.IndexOf(',');
            ReadOnlySpan<char> idSpan = line.Slice(start, commaIndex);
            int sensorId = ParseInt(idSpan);
            
            // Field 2: Temperature (float)
            start = commaIndex + 1;
            commaIndex = line.Slice(start).IndexOf(',') + start;
            ReadOnlySpan<char> tempSpan = line.Slice(start, commaIndex - start);
            float temperature = ParseFloat(tempSpan, numberBuffer);
            
            // Field 3: Humidity (float)
            start = commaIndex + 1;
            commaIndex = line.Slice(start).IndexOf(',') + start;
            ReadOnlySpan<char> humiditySpan = line.Slice(start, commaIndex - start);
            float humidity = ParseFloat(humiditySpan, numberBuffer);
            
            // Field 4: Timestamp (long)
            start = commaIndex + 1;
            ReadOnlySpan<char> timestampSpan = line.Slice(start);
            long timestamp = ParseLong(timestampSpan);

            // 4. AI CONTEXT: TENSOR PREPARATION (SIMD)
            // In AI training, raw data is converted into feature vectors.
            // Here we create a feature vector: [Temperature, Humidity, NormalizedID]
            // We use System.Numerics.Vector<T> for hardware-accelerated math (SIMD).
            // This processes multiple data points in a single CPU instruction.
            
            // Normalize ID for the model (arbitrary scaling)
            float normalizedId = sensorId / 1000.0f;

            // Create a Vector3 containing our features.
            // This packs 3 floats into a single SIMD register.
            Vector3 features = new Vector3(temperature, humidity, normalizedId);

            // Apply a transformation (e.g., scaling factor for the model input)
            Vector3 scalingFactor = new Vector3(0.1f, 0.1f, 1.0f);
            Vector3 scaledFeatures = Vector3.Multiply(features, scalingFactor);

            // Log the result to demonstrate processing
            // (Note: Console.WriteLine allocates string memory, but we are doing it once per row for demo)
            Console.WriteLine($"Row Parsed | ID: {sensorId} | Temp: {temperature} | Hum: {humidity} | Features: {scaledFeatures}");
        }

        // Helper: Parses an integer from a Span<char> without allocation.
        private static int ParseInt(ReadOnlySpan<char> span)
        {
            int result = 0;
            foreach (char c in span)
            {
                if (c >= '0' && c <= '9')
                    result = result * 10 + (c - '0');
            }
            return result;
        }

        // Helper: Parses a long from a Span<char>.
        private static long ParseLong(ReadOnlySpan<char> span)
        {
            long result = 0;
            foreach (char c in span)
            {
                if (c >= '0' && c <= '9')
                    result = result * 10 + (c - '0');
            }
            return result;
        }

        // Helper: Parses a float. 
        // CRITICAL: We convert the float substring to a standard string temporarily to use float.Parse.
        // WHY? Parsing floats manually is complex (handling decimal points, exponents). 
        // In a true zero-allocation scenario, we would write a custom float parser or use experimental libraries.
        // Here, we demonstrate the flow, but acknowledge that parsing complex types often requires a managed string bridge
        // unless using advanced low-level primitives.
        private static float ParseFloat(ReadOnlySpan<char> span, Span<char> tempBuffer)
        {
            // Copy the span to the temp buffer to create a null-terminated string
            span.CopyTo(tempBuffer);
            tempBuffer[span.Length] = '\0'; 
            
            // Convert to string (Allocation occurs here for the string object)
            // In production AI pipelines, data is often pre-processed or stored in binary formats (e.g., .bin) 
            // to avoid text parsing entirely.
            string floatString = new string(tempBuffer.Slice(0, span.Length));
            
            return float.Parse(floatString);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            MemoryEfficientCsvParser.ProcessDataset();
        }
    }
}
