
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;

// REAL-WORLD CONTEXT:
// Imagine you are organizing a digital playlist. You have a fixed list of song titles
// stored in an array. You want to display each song title to the user one by one.
// Using a 'foreach' loop is the cleanest way to read through every item in the list.

class Program
{
    static void Main()
    {
        // 1. We declare an array of strings to hold our playlist.
        //    Arrays are fixed-size collections (Chapter 11).
        string[] playlist = { "Bohemian Rhapsody", "Stairway to Heaven", "Hotel California" };

        // 2. We use the 'foreach' loop to iterate over the array.
        //    This loop automatically visits every element from start to finish.
        //    'song' is a temporary variable that holds the current element.
        foreach (string song in playlist)
        {
            // 3. Inside the loop body, we print the current song to the console.
            Console.WriteLine($"Now playing: {song}");
        }
    }
}
