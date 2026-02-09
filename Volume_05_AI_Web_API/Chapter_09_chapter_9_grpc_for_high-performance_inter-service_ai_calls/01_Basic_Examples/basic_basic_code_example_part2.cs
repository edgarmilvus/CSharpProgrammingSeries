
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

# Source File: basic_basic_code_example_part2.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Threading.Tasks;
using Grpc.Net.Client;
using AiGrpcDemo;

class ClientProgram
{
    static async Task Main(string[] args)
    {
        // 1. Create the channel (connection) to the server
        // Note: For .NET Core 3.1/5+, use 'http://localhost:5000'. 
        // For .NET 6+, gRPC requires HTTPS by default. 
        // We use 'http://localhost:5000' for simplicity in this local dev example.
        var channel = GrpcChannel.ForAddress("http://localhost:5000");
        
        // 2. Create the client
        var client = new ModelService.ModelServiceClient(channel);

        // 3. Prepare the request
        var request = new PredictionRequest
        {
            ModelName = "SentimentAnalysis-v2",
            InputText = "The performance of this API is incredible."
        };

        try
        {
            // 4. Call the remote method
            var response = await client.PredictAsync(request);

            // 5. Process the response
            Console.WriteLine($"Result: {response.Result}");
            Console.WriteLine($"Confidence: {response.ConfidenceScore}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling gRPC service: {ex.Message}");
        }
        
        // Keep console open
        Console.ReadKey();
    }
}
