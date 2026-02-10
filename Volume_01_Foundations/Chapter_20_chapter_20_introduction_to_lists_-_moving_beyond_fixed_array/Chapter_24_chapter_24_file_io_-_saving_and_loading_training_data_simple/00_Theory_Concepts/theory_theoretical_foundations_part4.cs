
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

// Source File: theory_theoretical_foundations_part4.cs
// Description: Theoretical Foundations
// ==========================================

using System;
using System.IO;
using System.Collections.Generic;

public class Program
{
    public static void Main()
    {
        List<string> trainingPrompts = new List<string>();
        trainingPrompts.Add("Prompt 1");
        trainingPrompts.Add("Prompt 2");
        trainingPrompts.Add("Prompt 3");

        string filePath = "dataset_stream.txt";

        // StreamWriter opens a connection to the file
        // The 'using' statement ensures the file is closed automatically
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (string prompt in trainingPrompts)
            {
                // Write one line at a time
                writer.WriteLine(prompt);
            }
        } // Connection closes here automatically, even if errors occur

        Console.WriteLine("Data streamed successfully.");
    }
}
