
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

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Reuse Customer/Order/OrderItem definitions from Exercise 1, 
// but ensure the DbContext configuration is correct.

public class OrderContextConfig : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer("YourConnectionStringHere");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Requirement 2: Configuration in OnModelCreating
        // This is the critical setting for orphan removal.
        // It tells EF Core that if an OrderItem is removed from Order.Items,
        // it should be deleted from the database.
        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .OnDelete(DeleteBehavior.Cascade); // Or DeleteBehavior.ClientCascade
    }
}

public class OrphanRemovalDemo
{
    public async Task RunDemoAsync()
    {
        // 1. Seed the database
        using (var context = new OrderContextConfig())
        {
            if (!await context.Orders.AnyAsync())
            {
                var order = new Order 
                { 
                    Id = 1, 
                    OrderNumber = "ORD-001", 
                    CustomerId = 1, // Assuming customer exists
                    Items = new List<OrderItem>
                    {
                        new OrderItem { Id = 1, ProductName = "Widget A", Quantity = 5, OrderId = 1 },
                        new OrderItem { Id = 2, ProductName = "Widget B", Quantity = 10, OrderId = 1 },
                        new OrderItem { Id = 3, ProductName = "Widget C", Quantity = 2, OrderId = 1 }
                    }
                };
                context.Orders.Add(order);
                await context.SaveChangesAsync();
            }
        }

        // 2. Fetch Order (Simulating Disconnected State)
        Order? orderToUpdate;
        using (var context = new OrderContextConfig())
        {
            orderToUpdate = await context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == 1);
        } // Context disposed -> Disconnected

        // 3. Modify the graph (Remove one item)
        if (orderToUpdate != null)
        {
            var itemToRemove = orderToUpdate.Items.FirstOrDefault();
            if (itemToRemove != null)
            {
                orderToUpdate.Items.Remove(itemToRemove);
                Console.WriteLine($"Removed {itemToRemove.ProductName} from collection.");
            }
        }

        // 4. Attach to a THIRD DbContext instance
        using (var context = new OrderContextConfig())
        {
            // Requirement 5: Use Update() or Entry().State = Modified
            // This marks the parent as Modified. 
            // With the Cascade config, EF Core detects the missing child 
            // and marks it for deletion.
            var entry = context.Update(orderToUpdate!);
            
            // Requirement 6: Visualize using TrackGraph
            Console.WriteLine("\n--- TrackGraph Visualization ---");
            context.ChangeTracker.TrackGraph(orderToUpdate, node =>
            {
                var entityType = node.Entry.Entity.GetType().Name;
                var state = node.Entry.State;
                Console.WriteLine($"Node: {entityType}, State: {state}");
            });

            // Verify the state of the orphaned entity
            var orphan = context.Entry(itemToRemove!).State;
            Console.WriteLine($"\nState of removed item: {orphan}"); 
            // Expected: Deleted (if configured correctly)

            await context.SaveChangesAsync();
            Console.WriteLine("Update complete. Orphan should be deleted.");
        }
    }
}
