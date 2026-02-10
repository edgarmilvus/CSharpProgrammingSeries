
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

// ---------------------------------------------------------
// 1. SETUP: Mocking EF Core Infrastructure
// ---------------------------------------------------------
// In a real app, these would come from 'Microsoft.EntityFrameworkCore'
// We simulate them here to make this code runnable without external dependencies.

using System.Collections.Generic;
using System.Linq;

namespace EFCoreArchitectures
{
    // Simulating EF Core's DbContext
    public class StoreContext : DbContext
    {
        public DbSet<Order> Orders { get; set; } = null!;

        // Simulating data in memory for this example
        public static StoreContext CreateSeedContext()
        {
            var context = new StoreContext();
            context.Orders.AddRange(
                new Order { Id = 1, CustomerName = "Alice", Status = OrderStatus.Pending },
                new Order { Id = 2, CustomerName = "Bob", Status = OrderStatus.Shipped },
                new Order { Id = 3, CustomerName = "Charlie", Status = OrderStatus.Pending }
            );
            return context;
        }
    }

    // Simulating DbSets
    public class DbSet<T> : List<T> where T : class { }

    // Simulating DbContext base
    public class DbContext { }

    // ---------------------------------------------------------
    // 2. DOMAIN MODEL
    // ---------------------------------------------------------
    public enum OrderStatus { Pending, Shipped, Cancelled }

    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
    }

    // ---------------------------------------------------------
    // 3. APPROACH A: REPOSITORY PATTERN (Traditional)
    // ---------------------------------------------------------
    // The Repository acts as a middleman, hiding the DbContext.
    public interface IOrderRepository
    {
        IEnumerable<Order> GetPendingOrders();
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly StoreContext _context;

        public OrderRepository(StoreContext context)
        {
            _context = context;
        }

        public IEnumerable<Order> GetPendingOrders()
        {
            // Logic is encapsulated here.
            // We query the DbSet, filter, and return results.
            return _context.Orders
                .Where(o => o.Status == OrderStatus.Pending)
                .ToList();
        }
    }

    // ---------------------------------------------------------
    // 4. APPROACH B: DIRECT CONTEXT (Modern/Pragmatic)
    // ---------------------------------------------------------
    // Instead of a repository, the Service accesses the DbContext directly.
    public class OrderService
    {
        private readonly StoreContext _context;

        public OrderService(StoreContext context)
        {
            _context = context;
        }

        public IEnumerable<Order> GetPendingOrdersDirectly()
        {
            // We access the DbSet directly.
            // This allows for flexible querying without creating new repository methods.
            return _context.Orders
                .Where(o => o.Status == OrderStatus.Pending)
                .ToList();
        }
    }

    // ---------------------------------------------------------
    // 5. EXECUTION
    // ---------------------------------------------------------
    public class Program
    {
        public static void Main()
        {
            // Initialize mock data
            var context = StoreContext.CreateSeedContext();

            // --- Using the Repository Pattern ---
            Console.WriteLine("--- Repository Pattern ---");
            IOrderRepository repo = new OrderRepository(context);
            var ordersFromRepo = repo.GetPendingOrders();
            foreach (var order in ordersFromRepo)
            {
                Console.WriteLine($"Repo: Order {order.Id} for {order.CustomerName}");
            }

            // --- Using Direct Context ---
            Console.WriteLine("\n--- Direct Context ---");
            var service = new OrderService(context);
            var ordersFromService = service.GetPendingOrdersDirectly();
            foreach (var order in ordersFromService)
            {
                Console.WriteLine($"Direct: Order {order.Id} for {order.CustomerName}");
            }
        }
    }
}
