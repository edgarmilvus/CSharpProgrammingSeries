
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class CustomerRepository
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _dbContext;

        public CustomerRepository(HttpClient httpClient, AppDbContext dbContext)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
        }

        // 1. Fixed: Added ConfigureAwait(false) to network I/O
        public async Task<string> GetCustomerDataAsync(int id)
        {
            var response = await _httpClient.GetAsync($"/api/customers/{id}")
                                           .ConfigureAwait(false);
            
            return await response.Content.ReadAsStringAsync()
                                 .ConfigureAwait(false);
        }

        // 2. Fixed: Added ConfigureAwait(false) to DB I/O
        public async Task UpdateCustomerStatusAsync(int id, string status)
        {
            var customer = await _dbContext.Customers.FindAsync(id)
                                           .ConfigureAwait(false);
            
            if (customer != null)
            {
                customer.Status = status;
                await _dbContext.SaveChangesAsync()
                               .ConfigureAwait(false);
            }
        }

        // 3. Optimized: Process batch concurrently rather than sequentially
        public async Task ProcessCustomerBatchAsync(IEnumerable<int> ids)
        {
            // Create a list of tasks (fan-out)
            var tasks = ids.Select(id => UpdateCustomerStatusAsync(id, "Processed"));
            
            // Await all to complete concurrently (fan-in)
            await Task.WhenAll(tasks);
        }
    }

    // Mocking EF Core Context for compilation
    public class AppDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
    }

    public class Customer 
    { 
        public int Id { get; set; } 
        public string Status { get; set; }
    }
}
