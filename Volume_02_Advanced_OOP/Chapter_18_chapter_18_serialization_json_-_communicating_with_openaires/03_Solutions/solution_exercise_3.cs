
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

// 1. Polymorphic Base Class using Attributes for Type Discrimination
[JsonDerivedType(typeof(TextContent), typeDiscriminator: "text")]
[JsonDerivedType(typeof(ImageContent), typeDiscriminator: "image_url")]
public abstract class MessageContent { }

public class TextContent : MessageContent
{
    // Property name "Text" matches typical API requirements
    public string Text { get; set; }
    public TextContent(string text) { Text = text; }
}

public class ImageContent : MessageContent
{
    // OpenAI expects an object "image_url" containing a "url"
    public ImageUrlDetail ImageUrl { get; set; }
    public ImageContent(string url) { ImageUrl = new ImageUrlDetail { Url = url }; }
}

public class ImageUrlDetail
{
    public string Url { get; set; }
}

// 2. Message Container
public class ChatMessage
{
    public string Role { get; set; }
    
    // This list can contain mixed types (TextContent, ImageContent)
    public List<MessageContent> Content { get; set; }
}

// 3. Request Wrapper
public class ApiRequest
{
    public string Model { get; set; }
    public List<ChatMessage> Messages { get; set; }
}

public class Exercise3
{
    public static void Run()
    {
        // Configure options to support polymorphism and naming conventions
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower // Converts properties to snake_case
        };

        // Construct the request with mixed content
        var request = new ApiRequest
        {
            Model = "gpt-4-vision-preview",
            Messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Role = "user",
                    Content = new List<MessageContent>
                    {
                        new TextContent("What is in this image?"),
                        new ImageContent("https://example.com/image.png")
                    }
                }
            }
        };

        // Serialize
        string json = JsonSerializer.Serialize(request, options);
        Console.WriteLine(json);
    }
}
