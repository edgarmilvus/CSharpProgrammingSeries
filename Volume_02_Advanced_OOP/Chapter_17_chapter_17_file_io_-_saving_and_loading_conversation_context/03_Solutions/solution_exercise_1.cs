
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Tensor
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("shape")]
    public int[] Shape { get; set; }

    [JsonPropertyName("data")]
    public double[] Data { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    public Tensor(string name, int[] shape, double[] data)
    {
        Name = name;
        Shape = shape;
        Data = data;
        CreatedAt = DateTime.UtcNow;
    }

    // Empty constructor for deserialization
    public Tensor() { }
}

public class JsonTensorSerializer
{
    public static void SerializeToJson(Tensor tensor, string filePath)
    {
        // Configure options to ensure camelCase output
        var options = new JsonSerializerOptions
        {
            WriteIndented = true, // Makes it human-readable
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        string jsonString = JsonSerializer.Serialize(tensor, options);
        File.WriteAllText(filePath, jsonString);
        Console.WriteLine($"Tensor serialized to {filePath}");
    }

    public static Tensor DeserializeFromJson(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        string jsonString = File.ReadAllText(filePath);
        
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        Tensor tensor = JsonSerializer.Deserialize<Tensor>(jsonString, options);
        return tensor;
    }
}

// Usage Example
public class Program
{
    public static void Main()
    {
        double[] data = { 1.1, 2.2, 3.3, 4.4, 5.5, 6.6 };
        int[] shape = { 2, 3 };
        var myTensor = new Tensor("Layer1_Weights", shape, data);

        JsonTensorSerializer.SerializeToJson(myTensor, "tensor.json");
        
        var loadedTensor = JsonTensorSerializer.DeserializeFromJson("tensor.json");
        Console.WriteLine($"Loaded Tensor: {loadedTensor.Name}, Shape: [{string.Join(",", loadedTensor.Shape)}]");
    }
}
