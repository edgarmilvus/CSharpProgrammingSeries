
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        // 1. Simulate a stream of incoming user events (IDs and Names).
        //    In a real scenario, this might come from a log file or API.
        var userEvents = new List<(int Id, string Name)>
        {
            (101, "Alice"),
            (102, "Bob"),
            (101, "Alice"), // Duplicate ID
            (103, "Charlie"),
            (102, "Bob"),   // Duplicate ID
            (104, "David")
        };

        // 2. Create a HashSet to track unique IDs efficiently.
        //    HashSet<T> uses hashing to achieve O(1) average time complexity for lookups and insertions.
        var uniqueUserIds = new HashSet<int>();

        // 3. Create a Dictionary to map IDs to Names for quick retrieval.
        //    Dictionary<K,V> also uses hashing to map keys to values.
        var userDictionary = new Dictionary<int, string>();

        // 4. Process the stream manually (no LINQ).
        //    We iterate through the list and check for duplicates using the HashSet.
        foreach (var user in userEvents)
        {
            // TryAdd is a safe way to add to a HashSet; it returns false if the item already exists.
            if (uniqueUserIds.Add(user.Id))
            {
                // If the ID was unique, add it to the dictionary.
                userDictionary.Add(user.Id, user.Name);
                Console.WriteLine($"Added: ID={user.Id}, Name={user.Name}");
            }
            else
            {
                Console.WriteLine($"Skipped Duplicate: ID={user.Id}");
            }
        }

        // 5. Demonstrate efficient lookup.
        //    This is the "Tokenizer" or "Graph Node" scenario: finding data instantly.
        Console.WriteLine("\n--- Lookup Test ---");
        int searchId = 102;
        if (userDictionary.TryGetValue(searchId, out string name))
        {
            Console.WriteLine($"Found ID {searchId}: {name}");
        }
        else
        {
            Console.WriteLine($"ID {searchId} not found.");
        }
    }
}
