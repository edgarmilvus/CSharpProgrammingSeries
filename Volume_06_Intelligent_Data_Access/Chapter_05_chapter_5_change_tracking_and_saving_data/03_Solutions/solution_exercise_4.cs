
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

using Microsoft.EntityFrameworkCore;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsFeatured { get; set; }
}

public class BulkUpdateService
{
    // 2. Method BulkUpdateFeaturedProducts
    public async Task BulkUpdateFeaturedProducts()
    {
        using var context = new ProductContext();
        
        // 2. Fetch products (AsNoTracking is crucial here to avoid overhead)
        // We only need the ID and Price to filter, but we fetch the whole entity
        // to simulate the scenario of having the data loaded.
        var products = await context.Products
            .Where(p => p.Price > 100)
            .AsNoTracking() // Detached immediately upon retrieval
            .ToListAsync();

        Console.WriteLine($"Processing {products.Count} products...");

        // 4. Use a single SaveChangesAsync call
        // We re-attach entities manually.
        
        // Optimization: If using EF Core 7+, ExecuteUpdate is better, 
        // but here we demonstrate manual tracking as requested.
        
        foreach (var product in products)
        {
            // 3. Detach (Already done via AsNoTracking, but explicitly doing it for clarity)
            // var entry = context.Entry(product); // This would be Detached
            
            // 3. Modify property
            product.IsFeatured = true;

            // 3. Re-attach and mark as Modified
            var entry = context.Attach(product);
            
            // 3. CRITICAL: Mark ONLY IsFeatured as modified
            entry.Property(p => p.IsFeatured).IsModified = true;
            
            // We explicitly ignore Name and Price changes even if they were modified
            // by setting IsModified = false (though default is false if not changed)
        }

        await context.SaveChangesAsync();
    }
}

public class ProductContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer("YourConnectionStringHere");
}
