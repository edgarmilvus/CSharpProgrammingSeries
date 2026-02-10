
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
using System.Threading.Tasks;

// A library component designed for reusability.
public class LibraryService
{
    // This method simulates fetching data asynchronously.
    public async Task<string> FetchDataAsync()
    {
        // Simulate a network delay (I/O bound operation).
        await Task.Delay(100);

        // CRITICAL: We use ConfigureAwait(false) here.
        // This tells the runtime: "After the await completes, 
        // do not resume on the original context (e.g., UI thread or ASP.NET request context).
        // Instead, resume on any available thread pool thread."
        await Task.Delay(100).ConfigureAwait(false);

        return "Data from Library";
    }
}

// The application entry point (e.g., a Console App, UI App, or Web App).
public class Program
{
    // Main method to demonstrate the usage.
    // Note: In a real UI or ASP.NET scenario, this would be an event handler or controller action.
    // For this console demo, we block the main thread to simulate a synchronous call context.
    public static void Main()
    {
        Console.WriteLine("Starting application...");

        // We call the async method synchronously to simulate a context where deadlocks can occur.
        // WARNING: .GetResult() blocks the calling thread until the task completes.
        // In a UI app, this would be the UI thread. In ASP.NET, the request thread.
        string result = new LibraryService().FetchDataAsync().GetResult();

        Console.WriteLine($"Result: {result}");
        Console.WriteLine("Application finished.");
    }
}
