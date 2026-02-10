
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

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace AsyncTestingSolutions
{
    // 1. Define the interface for the LLM Streaming Client
    public interface ILlmStreamingClient
    {
        IAsyncEnumerable<string> GetCompletionAsync(string prompt);
    }

    // 2. Implement the CodeReviewService
    public class CodeReviewService
    {
        private readonly ILlmStreamingClient _llmClient;

        public CodeReviewService(ILlmStreamingClient llmClient)
        {
            _llmClient = llmClient;
        }

        public async IAsyncEnumerable<string> ReviewCodeAsync(string prompt)
        {
            await foreach (var line in _llmClient.GetCompletionAsync(prompt))
            {
                // 5. Filter out lines containing "password" or "secret"
                if (!line.Contains("password", StringComparison.OrdinalIgnoreCase) &&
                    !line.Contains("secret", StringComparison.OrdinalIgnoreCase))
                {
                    yield return line;
                }
            }
        }
    }

    public class DeterministicMockingTests
    {
        [Fact]
        public async Task ReviewCodeAsync_FiltersSensitiveData_ReturnsOnlySafeLines()
        {
            // 3. & 4. Setup Mock with deterministic data
            var mockClient = new Mock<ILlmStreamingClient>();
            
            // Define the predictable sequence
            var streamData = new List<string>
            {
                "Line 1",
                "Password found",
                "Line 3",
                "Secret key"
            };

            // Mock implementation using yield return to simulate async stream
            mockClient.Setup(c => c.GetCompletionAsync(It.IsAny<string>()))
                      .Returns(GenerateStream(streamData));

            var service = new CodeReviewService(mockClient.Object);

            // 5. Act & Assert
            var results = new List<string>();
            
            // 9. Ensure test completes quickly (implicit in async test)
            var startTime = DateTime.Now;

            await foreach (var line in service.ReviewCodeAsync("dummy prompt"))
            {
                results.Add(line);
            }

            var duration = DateTime.Now - startTime;
            Assert.True(duration.TotalMilliseconds < 100, "Test took too long");

            // Verify output
            Assert.Equal(2, results.Count);
            Assert.Contains("Line 1", results);
            Assert.Contains("Line 3", results);
            Assert.DoesNotContain("Password found", results);
            Assert.DoesNotContain("Secret key", results);
        }

        // Edge Case Tests
        [Fact]
        public async Task ReviewCodeAsync_EmptyStream_ReturnsEmpty()
        {
            var mockClient = new Mock<ILlmStreamingClient>();
            mockClient.Setup(c => c.GetCompletionAsync(It.IsAny<string>()))
                      .Returns(GenerateStream(new List<string>()));

            var service = new CodeReviewService(mockClient.Object);
            var results = await service.ReviewCodeAsync("prompt").ToListAsync();

            Assert.Empty(results);
        }

        [Fact]
        public async Task ReviewCodeAsync_AllFiltered_ReturnsEmpty()
        {
            var mockClient = new Mock<ILlmStreamingClient>();
            var filteredData = new List<string> { "Secret", "Password" };
            mockClient.Setup(c => c.GetCompletionAsync(It.IsAny<string>()))
                      .Returns(GenerateStream(filteredData));

            var service = new CodeReviewService(mockClient.Object);
            var results = await service.ReviewCodeAsync("prompt").ToListAsync();

            Assert.Empty(results);
        }

        // Helper method to simulate IAsyncEnumerable with yield return
        private async IAsyncEnumerable<string> GenerateStream(IEnumerable<string> data)
        {
            foreach (var item in data)
            {
                yield return item;
            }
        }
    }

    // Extension for ToListAsync to simplify assertions
    public static class AsyncEnumerableExtensions
    {
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
        {
            var list = new List<T>();
            await foreach (var item in source)
            {
                list.Add(item);
            }
            return list;
        }
    }
}
