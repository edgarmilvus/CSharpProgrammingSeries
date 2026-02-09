
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

using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

// 1. Define the Entity
// This class represents a product in our warehouse.
// It uses the [Key] attribute to explicitly define the Primary Key.
public class Product
{
    [Key]
    public int ProductId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }

    public override string ToString()
        => $"Product ID: {ProductId}, Name: {Name}, Price: ${Price}";
}

// 2. Define the DbContext
// This class manages the connection to the database and tracks changes.
// For this "Hello World" example, we use an InMemory database so 
// you can run this code without installing SQL Server.
public class WarehouseContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Using an in-memory database for demonstration purposes.
        // In production, you would use UseSqlServer, UseSqlite, etc.
        optionsBuilder.UseInMemoryDatabase("WarehouseDb");
    }
}

class Program
{
    static void Main(string[] args)
    {
        // 3. Instantiate the DbContext
        // The 'using' statement ensures the context is disposed of correctly 
        // when we are done, releasing resources.
        using (var context = new WarehouseContext())
        {
            Console.WriteLine("--- Step 1: Creating a new Product entity ---");
            
            // 4. Create a new instance of the Product
            // At this exact moment, this object is a standard C# object.
            // EF Core does not yet know about it.
            var newProduct = new Product 
            { 
                Name = "Wireless Mouse", 
                Price = 29.99m 
            };

            Console.WriteLine($"State before tracking: {context.Entry(newProduct).State}");

            // 5. Add the entity to the DbSet
            // This is the trigger. We are passing the object into the context's control.
            context.Products.Add(newProduct);

            // 6. Inspect the Change Tracker
            // Let's verify that EF Core has picked up this object.
            var entry = context.Entry(newProduct);
            Console.WriteLine($"State after Add: {entry.State}");
            
            // You can also inspect the original values (which are null here since it's new)
            Console.WriteLine($"Original Name: {entry.OriginalValues.GetValue<string>("Name")}");

            Console.WriteLine("\n--- Step 2: Saving changes ---");
            
            // 7. Commit to the Database
            // This generates the INSERT statement and executes it.
            // Note: In a real database, the ID would be generated here.
            context.SaveChanges();
            
            Console.WriteLine($"State after SaveChanges: {entry.State}");
            Console.WriteLine($"Generated ID: {newProduct.ProductId}");

            Console.WriteLine("\n--- Step 3: Modifying the entity ---");
            
            // 8. Modify the entity
            // We change a property. The context detects this change.
            newProduct.Price = 24.99m;
            
            Console.WriteLine($"State after modification: {entry.State}");
            Console.WriteLine($"Current Price: {entry.CurrentValues.GetValue<decimal>("Price")}");
            Console.WriteLine($"Original Price: {entry.OriginalValues.GetValue<decimal>("Price")}");

            // 9. Save again
            context.SaveChanges();
            Console.WriteLine($"State after second SaveChanges: {entry.State}");
        }
    }
}
