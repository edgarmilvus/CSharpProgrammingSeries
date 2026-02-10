
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace FilePlugin
{
    // 6. Custom Return Type for error handling
    public record FileProcessingResult
    {
        public bool Success { get; init; }
        public int? WordCount { get; init; }
        public string ErrorMessage { get; init; } = string.Empty;
    }

    public class FileProcessorPlugin
    {
        [KernelFunction, Description("Counts the words in a text file.")]
        public FileProcessingResult CountWordsInFile(
            [Description("The path to the text file")] string filePath)
        {
            try
            {
                // 4. Success Case: Perform file I/O
                if (!File.Exists(filePath))
                {
                    // Handle logical errors within the try block to return structured results
                    return new FileProcessingResult
                    {
                        Success = false,
                        ErrorMessage = $"File not found at path: {filePath}"
                    };
                }

                string content = File.ReadAllText(filePath);
                int wordCount = content.Split(new[] { ' ', '\t', '\n', '\r' }, 
                                              StringSplitOptions.RemoveEmptyEntries).Length;

                return new FileProcessingResult
                {
                    Success = true,
                    WordCount = wordCount
                };
            }
            catch (Exception ex)
            {
                // 5. Failure Case: Catch unexpected exceptions
                return new FileProcessingResult
                {
                    Success = false,
                    ErrorMessage = $"An error occurred: {ex.Message}"
                };
            }
        }
    }

    /*
     * 7. Why this matters:
     * 
     * When interacting with LLMs, exception handling strategy is critical.
     * 
     * 1. Throwing Exceptions: If we throw an exception (e.g., FileNotFoundException),
     *    the Semantic Kernel pipeline interrupts. The LLM receives a raw error message
     *    or a stack trace. LLMs are notoriously bad at parsing stack traces; they 
     *    cannot reliably extract the specific error type or location to formulate 
     *    a recovery strategy (like asking the user for a new path).
     * 
     * 2. Returning Structured Results: By returning a FileProcessingResult object,
     *    we provide the LLM with a clean, semantic signal. The LLM can read the 
     *    'Success: false' and 'ErrorMessage' properties. It can then reason:
     *    "The file was not found. I should ask the user to verify the file path."
     *    
     * This pattern transforms a runtime crash into a conversational flow.
     */
}
