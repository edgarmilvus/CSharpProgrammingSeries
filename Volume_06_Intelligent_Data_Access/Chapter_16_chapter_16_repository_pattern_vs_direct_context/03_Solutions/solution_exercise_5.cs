
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

// Reusing DynamicOrderQueryService and Specifications from Exercise 2

public class OrderServiceTests
{
    [Fact]
    public async Task GetShippedOrders_ReturnsCorrectData()
    {
        // 1. Arrange: Create in-memory data
        var data = new List<Order>
        {
            new Order { Id = 1, Status = "Shipped", TotalAmount = 100 },
            new Order { Id = 2, Status = "Pending", TotalAmount = 200 },
            new Order { Id = 3, Status = "Shipped", TotalAmount = 300 }
        };

        // 2. Arrange: Mock DbSet
        var mockSet = new Mock<DbSet<Order>>();

        // Configure the mock to handle LINQ queries
        // We cast to IQueryable<T> to use the standard extension methods
        var queryable = data.AsQueryable();
        
        mockSet.As<IQueryable<Order>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockSet.As<IQueryable<Order>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<Order>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<Order>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

        // CRITICAL: Mock IAsyncEnumerable for async operations (e.g., ToListAsync)
        mockSet.As<IAsyncEnumerable<Order>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<Order>(queryable.GetEnumerator()));

        // 3. Arrange: Mock DbContext
        var mockContext = new Mock<AppDbContext>();
        mockContext.Setup(c => c.Orders).Returns(mockSet.Object);

        // 4. Act
        var service = new DynamicOrderQueryService(mockContext.Object);
        
        // We use the specification from Exercise 2
        var specs = new[] { new OrdersByStatusSpec("Shipped") };
        
        // ToListAsync requires the mock to support IAsyncEnumerable
        var result = await service.GetOrders(specs).ToListAsync();

        // 5. Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, o => Assert.Equal("Shipped", o.Status));
    }

    // Helper class to simulate async enumeration
    private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return ValueTask.FromResult(_inner.MoveNext());
        }
    }
}
