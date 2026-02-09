
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

# Source File: solution_exercise_1.cs
# Description: Solution for Exercise 1
# ==========================================

using Microsoft.Data.SqlClient;
using System;
using System.Threading;
using System.Threading.Tasks;

public class UserProfileService
{
    private readonly string _connectionString;

    public UserProfileService(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Refactored to be fully asynchronous
    public async Task<string> GetUserProfile(int userId, CancellationToken cancellationToken)
    {
        // Use 'await using' to ensure the connection is disposed asynchronously
        await using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync(cancellationToken);
            
            var command = new SqlCommand("SELECT Preferences FROM Users WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", userId);

            // Use ExecuteReaderAsync for non-blocking I/O
            await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                // Use ReadAsync to fetch rows without blocking
                if (await reader.ReadAsync(cancellationToken))
                {
                    return reader["Preferences"].ToString();
                }
            }
        }
        return string.Empty;
    }

    // Refactored to propagate asynchronous behavior
    public async Task<string> GeneratePrompt(int userId, string basePrompt, CancellationToken cancellationToken)
    {
        // Await the asynchronous call
        string preferences = await GetUserProfile(userId, cancellationToken);
        return $"{basePrompt} User Preferences: {preferences}";
    }
}
