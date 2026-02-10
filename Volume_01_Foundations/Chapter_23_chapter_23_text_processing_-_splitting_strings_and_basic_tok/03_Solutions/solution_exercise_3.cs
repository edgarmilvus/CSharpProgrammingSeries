
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System;

class Program
{
    static void Main()
    {
        string urlQuery = "?category=books&sort=price&filter=in-stock";
        
        // Remove the leading '?' if it exists to make processing cleaner
        // We check if it starts with "?" and use Substring to remove the first character
        if (urlQuery.StartsWith("?"))
        {
            urlQuery = urlQuery.Substring(1);
        }

        // 1. First Split: Break into individual parameters by '&'
        string[] parameters = urlQuery.Split('&');
        
        // 2. Iterate through parameters
        foreach (string param in parameters)
        {
            // 3. Second Split: Break key=value into ["key", "value"]
            string[] keyValue = param.Split('=');
            
            // Safety check: ensure we actually have a key and a value
            if (keyValue.Length == 2)
            {
                string key = keyValue[0];
                string value = keyValue[1];
                
                // 4. Check if this is the parameter we want
                if (key == "sort")
                {
                    Console.WriteLine($"Sort value found: {value}");
                }
            }
        }
    }
}
