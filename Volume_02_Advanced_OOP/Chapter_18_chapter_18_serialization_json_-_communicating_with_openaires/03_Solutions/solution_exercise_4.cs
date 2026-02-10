
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.Diagnostics;
using System.Text.Json;

public class AIModelConfig
{
    public string ModelName { get; set; }
    public double Temperature { get; set; }
    public int MaxTokens { get; set; }
    public string SystemPrompt { get; set; }
    public string ApiKey { get; set; } // Sensitive data
}

public class ApiClient
{
    // Define a delegate for the callback
    public delegate void SerializationCallback(string jsonPayload);

    // Method accepting the delegate and lambda
    public void SerializeForAPI(
        AIModelConfig config, 
        Func<AIModelConfig, object> selector, 
        SerializationCallback callback)
    {
        var sw = Stopwatch.StartNew();
        
        // 1. Apply the Lambda Selector (Projection)
        // This creates an anonymous object containing only the whitelisted fields
        object projectedData = selector(config);
        
        // 2. Serialize the projected data
        string json = JsonSerializer.Serialize(projectedData, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        sw.Stop();
        Console.WriteLine($"Serialization took {sw.ElapsedMilliseconds}ms");
        
        // 3. Invoke the Callback Delegate
        callback(json);
    }
}

public class Exercise4
{
    public static void Run()
    {
        var config = new AIModelConfig
        {
            ModelName = "gpt-4-turbo",
            Temperature = 0.7,
            MaxTokens = 4096,
            SystemPrompt = "You are a helpful assistant.",
            ApiKey = "sk-1234567890" // This should NOT be sent
        };

        var client = new ApiClient();

        // Define the Lambda Expression for whitelisting fields
        Func<AIModelConfig, object> selector = (cfg) => new
        {
            // Whitelist specific fields, explicitly renaming them if needed
            model = cfg.ModelName,
            temp = cfg.Temperature
            // Note: MaxTokens, SystemPrompt, and ApiKey are excluded here
        };

        // Define the Lambda for the Callback
        Action<string> logCallback = (json) => 
        {
            Console.WriteLine("--- API Payload (Safe) ---");
            Console.WriteLine(json);
            Console.WriteLine("--------------------------");
        };

        // Execute
        // We cast the Action<string> to the specific delegate type if necessary, 
        // or simply pass it (Action is compatible with most delegate patterns in C#)
        client.SerializeForAPI(config, selector, logCallback);
    }
}
