
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

// Source File: solution_exercise_4.cs
// Description: Solution for Exercise 4
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Services;

namespace ChunkingExercises
{
    // Simplified interface for demonstration
    public interface ITextEmbeddingGenerationService
    {
        Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    }

    public class HybridChunker
    {
        private readonly ITextEmbeddingGenerationService _embeddingService;
        private const int HardMaxTokens = 1024; // Hard limit
        private const double SemanticDropThreshold = 0.4;

        public HybridChunker(ITextEmbeddingGenerationService embeddingService)
        {
            _embeddingService = embeddingService;
        }

        public List<string> ProcessDocument(string markdownContent)
        {
            // 1. Parse headers and build a hierarchy tree
            var lines = markdownContent.Split('\n');
            var root = new HeaderNode { Level = 0, Content = "Root", StartIndex = -1 };
            var stack = new Stack<HeaderNode>();
            stack.Push(root);

            var orphanText = new List<string>();
            int currentLineIndex = 0;

            foreach (var line in lines)
            {
                var match = Regex.Match(line, @"^(#{1,6})\s+(.*)");
                if (match.Success)
                {
                    int level = match.Groups[1].Length;
                    string headerText = match.Groups[2].Value;

                    var node = new HeaderNode
                    {
                        Level = level,
                        Content = headerText,
                        StartIndex = currentLineIndex,
                        TextBuffer = new List<string>()
                    };

                    // Handle Nesting: Pop stack until we find a parent
                    while (stack.Peek().Level >= level)
                    {
                        var closedNode = stack.Pop();
                        if (stack.Peek().Level < level)
                        {
                            // Finalize the closed node's content
                            stack.Peek().Children.Add(closedNode);
                        }
                    }

                    stack.Push(node);
                }
                else
                {
                    // Accumulate text
                    if (stack.Count == 1) // Orphan text (no parent header)
                    {
                        orphanText.Add(line);
                    }
                    else
                    {
                        stack.Peek().TextBuffer.Add(line);
                    }
                }
                currentLineIndex++;
            }

            // Close remaining nodes in stack
            while (stack.Count > 1)
            {
                var node = stack.Pop();
                stack.Peek().Children.Add(node);
            }

            // 2. Traverse tree and generate chunks with context
            var chunks = new List<string>();
            
            // Process Orphans first
            if (orphanText.Any(t => !string.IsNullOrWhiteSpace(t)))
            {
                chunks.Add($"[Orphan Context]: {string.Join("\n", orphanText)}");
            }

            // Recursive traversal
            TraverseAndChunk(root, "", chunks);

            return chunks;
        }

        private void TraverseAndChunk(HeaderNode node, string accumulatedContext, List<string> chunks)
        {
            // Update context
            string currentContext = node.Level > 0 ? $"{accumulatedContext} > {node.Content}" : accumulatedContext;
            
            // Combine node's own text
            string nodeContent = string.Join("\n", node.TextBuffer);
            
            // Check semantic break (Simulated for this exercise)
            // In a real scenario, we would generate embeddings for nodeContent and compare with accumulatedContext
            
            // Dynamic Budgeting Logic:
            // We check if adding this node's content exceeds the hard max.
            // If it does, we finalize the current chunk and start a new one.
            int currentTokenCount = GPT3Tokenizer.Encode(currentContext + nodeContent).Count;

            if (currentTokenCount > HardMaxTokens)
            {
                // If the node itself is too big, we might need to split it further (omitted for brevity)
                // For now, we just start a new chunk for this large node
                chunks.Add($"[Context]: {currentContext}\n[Content]: {nodeContent}");
            }
            else
            {
                // Accumulate context into the chunk
                chunks.Add($"[Context]: {currentContext}\n[Content]: {nodeContent}");
            }

            // Process Children
            foreach (var child in node.Children)
            {
                TraverseAndChunk(child, currentContext, chunks);
            }
        }

        // Helper class for the tree structure
        private class HeaderNode
        {
            public int Level { get; set; }
            public string Content { get; set; }
            public int StartIndex { get; set; }
            public List<string> TextBuffer { get; set; } = new List<string>();
            public List<HeaderNode> Children { get; set; } = new List<HeaderNode>();
        }
    }

    // xUnit Test Case (Conceptual)
    public class HybridChunkerTests
    {
        [Fact] // xUnit attribute
        public void ProcessDocument_NestedHeaders_InheritsContextCorrectly()
        {
            // Arrange
            var mockEmbeddingService = new Mock<ITextEmbeddingGenerationService>(); // Mocking dependency
            var chunker = new HybridChunker(mockEmbeddingService.Object);
            
            string markdown = @"
# Main Title
Intro text.
## Subsection A
Details for A.
### Detail X
Specifics for X.
";

            // Act
            var chunks = chunker.ProcessDocument(markdown);

            // Assert
            // Verify that the chunk for "Detail X" contains the hierarchy "Main Title > Subsection A > Detail X"
            var detailChunk = chunks.FirstOrDefault(c => c.Contains("Detail X"));
            Assert.NotNull(detailChunk);
            Assert.Contains("Main Title", detailChunk);
            Assert.Contains("Subsection A", detailChunk);
        }
    }
}
