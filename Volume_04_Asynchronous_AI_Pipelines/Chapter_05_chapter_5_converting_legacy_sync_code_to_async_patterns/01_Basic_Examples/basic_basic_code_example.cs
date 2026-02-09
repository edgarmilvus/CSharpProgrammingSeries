
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;
using System.Data;
using System.Data.SqlClient; // Legacy ADO.NET provider
using System.Threading.Tasks;

namespace AsyncConversionExample
{
    public class UserProfileService
    {
        private readonly string _connectionString;

        public UserProfileService(string connectionString)
        {
            _connectionString = connectionString;
        }

        // ---------------------------------------------------------
        // 1. LEGACY SYNC METHOD (The "Before" State)
        // ---------------------------------------------------------
        // This method blocks the thread while waiting for the database.
        // Do NOT use this in high-throughput AI applications.
        public string GetUserPreference_Sync(int userId)
        {
            string preference = "Default";
            
            // Blocking call: Opens connection and waits
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open(); 

                // Blocking call: Executes query and waits
                using (var command = new SqlCommand("SELECT Preference FROM Users WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", userId);

                    // Blocking call: Fetches data and waits
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            preference = reader["Preference"].ToString();
                        }
                    }
                }
            }
            return preference;
        }

        // ---------------------------------------------------------
        // 2. REFACTORED ASYNC METHOD (The "After" State)
        // ---------------------------------------------------------
        // This method releases the thread while waiting for I/O.
        // This is the target pattern for the conversion.
        public async Task<string> GetUserPreference_Async(int userId)
        {
            string preference = "Default";

            // Use 'await using' to ensure the connection is disposed asynchronously
            await using (var connection = new SqlConnection(_connectionString))
            {
                // Asynchronously opens the connection without blocking the thread
                await connection.OpenAsync();

                await using (var command = new SqlCommand("SELECT Preference FROM Users WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", userId);

                    // Asynchronously executes the reader
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Asynchronously reads the first row
                        if (await reader.ReadAsync())
                        {
                            preference = reader["Preference"].ToString();
                        }
                    }
                }
            }
            return preference;
        }
    }

    // ---------------------------------------------------------
    // 3. MAIN PROGRAM (Simulation)
    // ---------------------------------------------------------
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Async Conversion Demo...");

            // Note: This connection string won't actually connect in this demo,
            // but the code structure is valid for a real environment.
            string mockConnectionString = "Server=myServer;Database=myDB;Trusted_Connection=True;";
            var service = new UserProfileService(mockConnectionString);

            try
            {
                // Simulating an AI application needing user data
                Console.WriteLine("Fetching user preference asynchronously...");
                
                // The 'await' keyword here yields control back to the caller 
                // while the database operation is in progress.
                string preference = await service.GetUserPreference_Async(userId: 42);
                
                Console.WriteLine($"User Preference retrieved: {preference}");
            }
            catch (Exception ex)
            {
                // In a real app, this would be logged or handled by an AI error-correction loop
                Console.WriteLine($"Operation failed: {ex.Message}");
            }

            Console.WriteLine("Demo finished.");
        }
    }
}
