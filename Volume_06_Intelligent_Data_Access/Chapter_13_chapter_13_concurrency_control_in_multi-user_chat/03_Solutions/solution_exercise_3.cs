
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Enum and DTO
public enum ResolutionStatus { Merged, Overwritten, Failed }

public class ConflictResolutionResult
{
    public ResolutionStatus Status { get; set; }
    public string FinalContent { get; set; }
    public List<Guid> ConflictingUserIds { get; set; }
}

public class ConflictResolverService
{
    private readonly ChatContext _context;

    public ConflictResolverService(ChatContext context)
    {
        _context = context;
    }

    public async Task<ConflictResolutionResult> ResolveMessageConflictAsync(
        Guid messageId, string attemptedUpdate, byte[] clientRowVersion)
    {
        // 1. Retrieve current database record
        var currentMessage = await _context.Messages.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (currentMessage == null)
        {
            return new ConflictResolutionResult 
            { 
                Status = ResolutionStatus.Failed, 
                FinalContent = null 
            };
        }

        // 2. Compare RowVersion (Optimistic Concurrency Check)
        // If versions match, no conflict occurred (rare in this context but good practice to check)
        if (currentMessage.RowVersion.SequenceEqual(clientRowVersion))
        {
            // Update normally (omitted for brevity, assuming conflict was detected prior to calling this)
        }

        // 3. Levenshtein Distance Logic
        int distance = CalculateLevenshteinDistance(currentMessage.Content, attemptedUpdate);
        const int Threshold = 5;

        if (distance <= Threshold)
        {
            // 4. Merge Strategy (Concatenate)
            // Example format: "User A: [Original] | User B: [Attempted]"
            string mergedContent = $"{currentMessage.Content} | {attemptedUpdate}";
            
            // Update the entity in the context to save the merge
            var messageToUpdate = new ChatMessage { Id = messageId }; // Attach lightweight entity
            _context.Attach(messageToUpdate);
            messageToUpdate.Content = mergedContent;
            messageToUpdate.ModifiedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            return new ConflictResolutionResult
            {
                Status = ResolutionStatus.Merged,
                FinalContent = mergedContent,
                ConflictingUserIds = new List<Guid> { currentMessage.UserId }
            };
        }
        else
        {
            // 5. LastWriteWins Strategy (Overwrite)
            var messageToUpdate = new ChatMessage { Id = messageId };
            _context.Attach(messageToUpdate);
            messageToUpdate.Content = attemptedUpdate;
            messageToUpdate.ModifiedAt = DateTime.UtcNow;
            
            // We must manually update the RowVersion context to match the client's 
            // to force the update to go through despite the initial conflict, 
            // or simply rely on the fact we are resolving the conflict explicitly.
            // However, standard EF behavior requires resolving the initial DbUpdateConcurrencyException first.
            // Here we assume we are applying the resolved state.
            
            await _context.SaveChangesAsync();

            return new ConflictResolutionResult
            {
                Status = ResolutionStatus.Overwritten,
                FinalContent = attemptedUpdate,
                ConflictingUserIds = new List<Guid> { currentMessage.UserId }
            };
        }
    }

    // 3. Manual Levenshtein Distance Implementation
    private int CalculateLevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source))
        {
            return string.IsNullOrEmpty(target) ? 0 : target.Length;
        }

        if (string.IsNullOrEmpty(target))
        {
            return source.Length;
        }

        var sourceLength = source.Length;
        var targetLength = target.Length;
        var distance = new int[sourceLength + 1, targetLength + 1];

        for (var i = 0; i <= sourceLength; distance[i, 0] = i++) { }
        for (var j = 0; j <= targetLength; distance[0, j] = j++) { }

        for (var i = 1; i <= sourceLength; i++)
        {
            for (var j = 1; j <= targetLength; j++)
            {
                var cost = target[j - 1] == source[i - 1] ? 0 : 1;
                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
        }

        return distance[sourceLength, targetLength];
    }
}
