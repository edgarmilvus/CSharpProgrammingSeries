
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

// 1. Abstract Base Class
public abstract class AIProcessor
{
    // Abstract method: Must be implemented by derived classes
    protected abstract void Execute();

    // Concrete method: Shared workflow logic
    public void RunPipeline()
    {
        Console.WriteLine("Starting...");
        
        // Polymorphic call to the concrete implementation
        Execute();
        
        Console.WriteLine("Finished.");
    }
}

// 2. Concrete Class: ImageClassifier
public class ImageClassifier : AIProcessor
{
    protected override void Execute()
    {
        Console.WriteLine("Classifying Image...");
    }
}

// 2. Concrete Class: VoiceDetector
public class VoiceDetector : AIProcessor
{
    protected override void Execute()
    {
        Console.WriteLine("Detecting Voice...");
    }
}

// Main Program
public class Program
{
    public static void Main()
    {
        // 3. Attempting to instantiate abstract class (Uncommenting below causes error)
        // AIProcessor processor = new AIProcessor(); // Error: Cannot create an instance of the abstract class

        // 4. Polymorphic Array
        AIProcessor[] pipeline = new AIProcessor[] 
        { 
            new ImageClassifier(), 
            new VoiceDetector() 
        };

        foreach (AIProcessor processor in pipeline)
        {
            processor.RunPipeline();
            Console.WriteLine("----------------");
        }
    }
}
