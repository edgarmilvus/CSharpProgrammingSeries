
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

// Source File: project_advanced_application_script.cs
// Description: Advanced Application Script
// ==========================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace InventoryManagementSystem
{
    // ==========================================
    // 1. DATA MODELS (Domain Entities)
    // ==========================================
    // Represents a physical item in the warehouse.
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Category { get; set; }
    }

    // Represents a record of stock leaving the warehouse (Sale).
    public class SaleRecord
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public DateTime SaleDate { get; set; }
        public int QuantitySold { get; set; }
        public decimal UnitPrice { get; set; } // Snapshot of price at time of sale
    }

    // ==========================================
    // 2. MOCK DATABASE CONTEXT
    // ==========================================
    // Simulates Entity Framework Core's DbContext.
    // In a real scenario, this would connect to SQL Server/PostgreSQL.
    public class InventoryContext
    {
        // Simulating database tables using in-memory Lists
        public List<Product> Products { get; set; } = new List<Product>();
        public List<SaleRecord> Sales { get; set; } = new List<SaleRecord>();

        public InventoryContext()
        {
            SeedData();
        }

        // Seeding initial data to make the application functional
        private void SeedData()
        {
            Products.AddRange(new[]
            {
                new Product { Id = 1, Name = "Wireless Mouse", Price = 25.99m, StockQuantity = 150, Category = "Electronics" },
                new Product { Id = 2, Name = "Mechanical Keyboard", Price = 89.99m, StockQuantity = 45, Category = "Electronics" },
                new Product { Id = 3, Name = "USB-C Cable", Price = 12.50m, StockQuantity = 300, Category = "Accessories" },
                new Product { Id = 4, Name = "Monitor Stand", Price = 45.00m, StockQuantity = 20, Category = "Furniture" },
                new Product { Id = 5, Name = "Gaming Headset", Price = 59.99m, StockQuantity = 0, Category = "Electronics" } // Out of stock
            });

            Sales.AddRange(new[]
            {
                new SaleRecord { Id = 1, ProductId = 1, SaleDate = DateTime.Now.AddDays(-10), QuantitySold = 5, UnitPrice = 25.99m },
                new SaleRecord { Id = 2, ProductId = 2, SaleDate = DateTime.Now.AddDays(-8), QuantitySold = 2, UnitPrice = 89.99m },
                new SaleRecord { Id = 3, ProductId = 1, SaleDate = DateTime.Now.AddDays(-5), QuantitySold = 10, UnitPrice = 25.99m },
                new SaleRecord { Id = 4, ProductId = 3, SaleDate = DateTime.Now.AddDays(-2), QuantitySold = 50, UnitPrice = 12.50m },
                new SaleRecord { Id = 5, ProductId = 4, SaleDate = DateTime.Now.AddDays(-1), QuantitySold = 1, UnitPrice = 45.00m }
            });
        }
    }

    // ==========================================
    // 3. REPOSITORY LAYER (Data Access)
    // ==========================================
    // Handles complex querying logic.
    public class InventoryRepository
    {
        private readonly InventoryContext _context;

        public InventoryRepository(InventoryContext context)
        {
            _context = context;
        }

        // SCENARIO: Find high-value products (Price > $50) that are currently in stock.
        // Uses: Filtering (Where), Boolean Logic.
        public List<Product> GetHighValueInStockProducts()
        {
            // Simulating LINQ to SQL translation: 
            // SELECT * FROM Products WHERE Price > 50 AND StockQuantity > 0
            var query = _context.Products
                .Where(p => p.Price > 50.00m && p.StockQuantity > 0)
                .ToList();

            return query;
        }

        // SCENARIO: Generate a revenue report for the last 7 days.
        // Uses: Joining (Navigation Properties simulation), Aggregation (Sum), Date filtering.
        public void GenerateSalesReport()
        {
            Console.WriteLine("\n--- Recent Sales Report (Last 7 Days) ---");
            
            // Logic: We need to join Sales with Products to get the Product Name
            // Since we are using basic loops (simulating older EF or raw SQL logic), 
            // we iterate and match manually.
            
            DateTime cutoffDate = DateTime.Now.AddDays(-7);
            decimal totalRevenue = 0;

            foreach (var sale in _context.Sales)
            {
                if (sale.SaleDate >= cutoffDate)
                {
                    // Find the associated product using simple loop (Simulating navigation property)
                    Product relatedProduct = null;
                    foreach (var prod in _context.Products)
                    {
                        if (prod.Id == sale.ProductId)
                        {
                            relatedProduct = prod;
                            break;
                        }
                    }

                    if (relatedProduct != null)
                    {
                        decimal lineTotal = sale.QuantitySold * sale.UnitPrice;
                        totalRevenue += lineTotal;

                        Console.WriteLine($"Item: {relatedProduct.Name} | Qty: {sale.QuantitySold} | Revenue: ${lineTotal:F2}");
                    }
                }
            }

            Console.WriteLine($"\nTotal Revenue: ${totalRevenue:F2}");
        }

        // SCENARIO: Group products by Category to see inventory distribution.
        // Uses: Grouping logic (simulated via Dictionary).
        public void GroupInventoryByCategory()
        {
            Console.WriteLine("\n--- Inventory by Category ---");

            // In standard LINQ: _context.Products.GroupBy(p => p.Category)
            // Here we manually group using a Dictionary to demonstrate the logic.
            var categoryGroups = new Dictionary<string, List<Product>>();

            foreach (var product in _context.Products)
            {
                if (!categoryGroups.ContainsKey(product.Category))
                {
                    categoryGroups[product.Category] = new List<Product>();
                }
                categoryGroups[product.Category].Add(product);
            }

            foreach (var group in categoryGroups)
            {
                Console.WriteLine($"Category: {group.Key}");
                foreach (var product in group.Value)
                {
                    Console.WriteLine($"  - {product.Name} (Stock: {product.StockQuantity})");
                }
            }
        }

        // SCENARIO: Complex Search - Find products that match a partial name AND are low stock.
        // Uses: String manipulation, Conditional logic.
        public List<Product> SearchProducts(string searchTerm)
        {
            var results = new List<Product>();

            foreach (var p in _context.Products)
            {
                // Simulating SQL LIKE '%term%'
                bool matchesName = p.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0;
                bool isLowStock = p.StockQuantity < 50;

                if (matchesName && isLowStock)
                {
                    results.Add(p);
                }
            }

            return results;
        }
    }

    // ==========================================
    // 4. APPLICATION LOGIC (Main Program)
    // ==========================================
    class Program
    {
        static void Main(string[] args)
        {
            // Initialize the "Database"
            var context = new InventoryContext();
            var repo = new InventoryRepository(context);

            Console.WriteLine("=== INTELLIGENT DATA ACCESS SYSTEM ===\n");

            // 1. Execute Filtering Query
            var highValueItems = repo.GetHighValueInStockProducts();
            Console.WriteLine("High Value In-Stock Items:");
            foreach (var item in highValueItems)
            {
                Console.WriteLine($" - {item.Name}: ${item.Price}");
            }

            // 2. Execute Aggregation & Joining Logic
            repo.GenerateSalesReport();

            // 3. Execute Grouping Logic
            repo.GroupInventoryByCategory();

            // 4. Execute Search Logic
            Console.WriteLine("\n--- Search Results (Term: 'Mouse', Low Stock Only) ---");
            var searchResults = repo.SearchProducts("Mouse");
            foreach (var item in searchResults)
            {
                Console.WriteLine($"Found: {item.Name} (Stock: {item.StockQuantity})");
            }

            Console.WriteLine("\n=== END OF REPORT ===");
        }
    }
}
