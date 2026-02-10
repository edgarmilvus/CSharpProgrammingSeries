
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

// Source File: solution_exercise_2.cs
// Description: Solution for Exercise 2
// ==========================================

using Microsoft.KernelMemory.Pipeline;
using Microsoft.KernelMemory.MemoryStorage;
using System.Text.RegularExpressions;
using System.Threading.Channels;

namespace KernelMemory.Exercises;

public class SemanticChunkingPlugin : IAsyncPipelineStep
{
    private readonly ILogger<SemanticChunkingPlugin> _logger;
    private readonly SemanticChunkingOptions _options;

    public SemanticChunkingPlugin(SemanticChunkingOptions options, ILogger<SemanticChunkingPlugin> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<(string, DataContent)> InvokeAsync(
        DataPipeline pipeline, 
        DataPipelineStep step, 
        CancellationToken cancellationToken = default)
    {
        // 1. Extract raw text from the binary stream (assuming PDF extraction happened prior or we do it here)
        // In KM, usually a dedicated extractor step runs first. We assume text is in pipeline.GeneratedFiles.
        var textFile = pipeline.GeneratedFiles.FirstOrDefault(f => f.FileType == "txt");
        if (textFile == null) throw new InvalidOperationException("No text content found for chunking.");

        var fullText = await File.ReadAllTextAsync(textFile.ContentPath, cancellationToken);

        // 2. Semantic Chunking Logic
        var chunks = new List<string>();
        var currentChunk = new StringBuilder();
        var lastProcessedIndex = 0;

        // Regex for headings (e.g., "Section 1.2", "Introduction:")
        var headingRegex = new Regex(_options.HeadingPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        var lines = fullText.Split('\n');

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmedLine)) continue;

            bool isHeading = headingRegex.IsMatch(trimmedLine);
            
            // Check if adding this line exceeds token limit
            // Note: Token estimation is approximate. 1 token ~= 4 chars.
            if (currentChunk.Length + line.Length > _options.MaxTokens * 4 && currentChunk.Length > 0)
            {
                // Add Overlap Context
                if (_options.OverlapTokens > 0)
                {
                    string overlap = GetLastTokens(currentChunk.ToString(), _options.OverlapTokens);
                    chunks.Add(currentChunk.ToString());
                    
                    // Start new chunk with overlap
                    currentChunk.Clear();
                    currentChunk.AppendLine(overlap);
                }
                else
                {
                    chunks.Add(currentChunk.ToString());
                    currentChunk.Clear();
                }
            }

            // If it's a heading and we already have content, maybe start a new chunk
            if (isHeading && currentChunk.Length > 0 && _options.SplitOnHeadings)
            {
                 chunks.Add(currentChunk.ToString());
                 currentChunk.Clear();
            }

            currentChunk.AppendLine(line);
        }

        // Add remaining
        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString());
        }

        // 3. Create new DataContent for each chunk and push to pipeline
        // We use the Channel to ensure backpressure is handled if downstream steps are slow.
        var outputChannel = pipeline.DataIngestionChannel;
        
        int chunkIndex = 0;
        foreach (var chunk in chunks)
        {
            var chunkId = $"{pipeline.DocumentId}_chunk_{chunkIndex}";
            
            // Propagate metadata
            var chunkMetadata = new Dictionary<string, object>(pipeline.Metadata)
            {
                ["ChunkIndex"] = chunkIndex,
                ["SourceFile"] = textFile.Name
            };

            var chunkData = new DataContent(chunk)
            {
                Name = chunkId,
                FileType = "text",
                Metadata = chunkMetadata
            };

            // Write to channel (Async)
            await outputChannel.Writer.WriteAsync(chunkData, cancellationToken);
            chunkIndex++;
        }

        // Signal completion
        outputChannel.Writer.Complete();

        return (pipeline.DocumentId, new DataContent("Chunks generated"));
    }

    private string GetLastTokens(string text, int tokenCount)
    {
        // Approximate token extraction based on characters
        int charCount = tokenCount * 4;
        if (text.Length <= charCount) return text;
        return text.Substring(text.Length - charCount);
    }

    public class SemanticChunkingOptions
    {
        public int MaxTokens { get; set; } = 1000;
        public int OverlapTokens { get; set; } = 100;
        public string HeadingPattern { get; set; } = @"^\d+(\.\d+)*\s|^[A-Z][a-zA-Z\s]+:$";
        public bool SplitOnHeadings { get; set; } = true;
    }
}

// Registration Example (for context)
public static class PipelineExtensions
{
    public static KernelMemoryBuilder WithSemanticChunking(this KernelMemoryBuilder builder, SemanticChunkingPlugin.SemanticChunkingOptions options)
    {
        // KM allows custom steps injection
        builder.WithCustomPipelineStep(new SemanticChunkingPlugin(options, 
            builder.Services.BuildServiceProvider().GetRequiredService<ILogger<SemanticChunkingPlugin>>()));
        return builder;
    }
}
