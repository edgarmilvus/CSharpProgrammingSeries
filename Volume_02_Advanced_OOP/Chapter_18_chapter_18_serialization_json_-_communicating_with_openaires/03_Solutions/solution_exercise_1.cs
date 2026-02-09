
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
using System.Text.Json;

// 1. Define response classes
public class TextResponse
{
    public string Content { get; set; }
    public double Confidence { get; set; }
}

public class ImageResponse
{
    public string Url { get; set; }
    public string Size { get; set; }
}

// 2. Define the delegate type
public delegate string SerializationDelegate<T>(T response);

public class ModelResponseHandler
{
    // 3. Generic method accepting the delegate
    public void ProcessResponse<T>(T response, SerializationDelegate<T> serializer)
    {
        if (response == null || serializer == null)
        {
            Console.WriteLine("Invalid response or serializer.");
            return;
        }

        // Invoke the delegate passed in
        string jsonOutput = serializer(response);
        Console.WriteLine($"Processed Response: {jsonOutput}");
    }
}

public class Exercise1
{
    public static void Run()
    {
        var handler = new ModelResponseHandler();

        // 4. Implement Lambda expressions for serialization
        // Lambda for TextResponse: Serializes to a specific format with a "type" field
        SerializationDelegate<TextResponse> textSerializer = (textResp) =>
        {
            return JsonSerializer.Serialize(new 
            { 
                type = "text", 
                content = textResp.Content, 
                confidence = textResp.Confidence 
            });
        };

        // Lambda for ImageResponse: Serializes to a specific format with a "type" field
        SerializationDelegate<ImageResponse> imageSerializer = (imgResp) =>
        {
            return JsonSerializer.Serialize(new 
            { 
                type = "image", 
                url = imgResp.Url, 
                dimensions = imgResp.Size 
            });
        };

        // 5. Demonstrate usage
        var textObj = new TextResponse { Content = "Hello AI", Confidence = 0.98 };
        var imgObj = new ImageResponse { Url = "https://api.ai/img1", Size = "1024x1024" };

        Console.WriteLine("--- Text Response ---");
        handler.ProcessResponse(textObj, textSerializer);

        Console.WriteLine("\n--- Image Response ---");
        handler.ProcessResponse(imgObj, imageSerializer);
    }
}
