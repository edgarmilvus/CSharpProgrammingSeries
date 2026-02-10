
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
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

// Data structures defined as requested
public record Document(string FilePath, string Content);
public record TextChunk(string Id, string Content, string SourceFile, long StartPosition);

public class DocumentIngestor
{
    // Regex to identify chunks that are mostly non-alphanumeric (simulating stop word/punctuation filter)
    private static readonly Regex NonAlphanumericRegex = new Regex(@"^[\W_]+$", RegexOptions.Compiled);

    public async Task IngestDirectoryAsync(string directoryPath, string outputPath)
    {
        // 1. Define the blocks
        var parseBlock = new TransformBlock<string, Document>(
            async filePath =>
            {
                try
                {
                    // Simulate I/O latency
                    string content = await File.ReadAllTextAsync(filePath);
                    return new Document(filePath, content);
                }
                catch (IOException ex) { Console.WriteLine($"IO Error on {filePath}: {ex.Message}"); return null; }
                catch (UnauthorizedAccessException ex) { Console.WriteLine($"Access Denied on {filePath}: {ex.Message}"); return null; }
            },
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = Environment.ProcessorCount, BoundedCapacity = 100 }
        );

        var chunkBlock = new TransformBlock<Document, List<TextChunk>>(
            doc =>
            {
                if (doc == null) return new List<TextChunk>();

                // Rough estimation: 4 chars per token. Target 512 tokens -> 2048 chars. Overlap 128 tokens -> 512 chars.
                int chunkSize = 2048;
                int overlap = 512;
                var chunks = new List<TextChunk>();

                for (int i = 0; i < doc.Content.Length; i += (chunkSize - overlap))
                {
                    if (i >= doc.Content.Length) break;

                    int length = Math.Min(chunkSize, doc.Content.Length - i);
                    string content = doc.Content.Substring(i, length);
                    string id = Guid.NewGuid().ToString();
                    chunks.Add(new TextChunk(id, content, doc.FilePath, i));
                }
                return chunks;
            },
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = Environment.ProcessorCount, BoundedCapacity = 100 }
        );

        // Interactive Challenge: Filter Block
        var filterBlock = new TransformBlock<List<TextChunk>, List<TextChunk>>(
            chunks =>
            {
                // Filter out chunks that are empty or entirely non-alphanumeric
                return chunks.Where(c => !string.IsNullOrWhiteSpace(c.Content) && 
                                         !NonAlphanumericRegex.IsMatch(c.Content)).ToList();
            },
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1, BoundedCapacity = 100 }
        );

        var persistBlock = new ActionBlock<List<TextChunk>>(
            async chunks =>
            {
                if (chunks.Count == 0) return;

                // Append to JSONL file
                using (var stream = new FileStream(outputPath, FileMode.Append, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(stream))
                {
                    foreach (var chunk in chunks)
                    {
                        var json = JsonSerializer.Serialize(chunk);
                        await writer.WriteLineAsync(json);
                    }
                }
                Console.WriteLine($"Persisted {chunks.Count} chunks.");
            },
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1, BoundedCapacity = 100 }
        );

        // 2. Link the blocks
        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
        
        parseBlock.LinkTo(chunkBlock, linkOptions);
        chunkBlock.LinkTo(filterBlock, linkOptions);
        filterBlock.LinkTo(persistBlock, linkOptions);

        // 3. Feed data
        var files = Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            // SendAsync respects bounded capacity and backpressure
            await parseBlock.SendAsync(file);
        }

        // Signal completion
        parseBlock.Complete();

        // Wait for the final block to finish
        await persistBlock.Completion;
    }
}
