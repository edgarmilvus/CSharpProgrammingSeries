
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

# Source File: solution_exercise_2.cs
# Description: Solution for Exercise 2
# ==========================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Tensor
{
    public string Name { get; set; }
    public double[,] Values { get; set; }

    public Tensor(string name, double[,] values)
    {
        Name = name;
        Values = values;
    }
}

// Custom Converter for Tensor
public class TensorJsonConverter : JsonConverter<Tensor>
{
    public override Tensor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
        {
            var root = doc.RootElement;
            
            string name = root.GetProperty("Name").GetString();
            
            // Parse Shape array to get dimensions
            JsonElement shapeElement = root.GetProperty("Shape");
            int rows = shapeElement[0].GetInt32();
            int cols = shapeElement[1].GetInt32();
            
            // Parse flattened Data array
            JsonElement dataElement = root.GetProperty("Data");
            double[,] values = new double[rows, cols];
            
            // Reconstruct 2D array from flattened data
            int index = 0;
            var enumerator = dataElement.EnumerateArray();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (enumerator.MoveNext())
                    {
                        values[i, j] = enumerator.Current.GetDouble();
                    }
                }
            }
            
            return new Tensor(name, values);
        }
    }

    public override void Write(Utf8JsonWriter writer, Tensor tensor, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        writer.WriteString("Name", tensor.Name);
        
        // Write Shape
        writer.WritePropertyName("Shape");
        writer.WriteStartArray();
        writer.WriteNumberValue(tensor.Values.GetLength(0)); // Rows
        writer.WriteNumberValue(tensor.Values.GetLength(1)); // Cols
        writer.WriteEndArray();
        
        // Write Data (Flattened)
        writer.WritePropertyName("Data");
        writer.WriteStartArray();
        foreach (var val in tensor.Values)
        {
            writer.WriteNumberValue(val);
        }
        writer.WriteEndArray();
        
        writer.WriteEndObject();
    }
}

public class Exercise2
{
    public static void Run()
    {
        // Register the converter
        var options = new JsonSerializerOptions();
        options.Converters.Add(new TensorJsonConverter());
        
        // Create a 2x3 Tensor
        double[,] matrix = { { 1.1, 2.2, 3.3 }, { 4.4, 5.5, 6.6 } };
        var tensor = new Tensor("HiddenLayer", matrix);

        // Serialize
        string json = JsonSerializer.Serialize(tensor, options);
        Console.WriteLine($"Serialized JSON:\n{json}\n");

        // Deserialize (Round-trip)
        var deserializedTensor = JsonSerializer.Deserialize<Tensor>(json, options);
        Console.WriteLine($"Deserialized Name: {deserializedTensor.Name}");
        Console.WriteLine($"Deserialized Dimensions: {deserializedTensor.Values.GetLength(0)}x{deserializedTensor.Values.GetLength(1)}");
        
        // Verify data integrity
        Console.WriteLine($"Value at [1, 1]: {deserializedTensor.Values[1, 1]}");
    }
}
