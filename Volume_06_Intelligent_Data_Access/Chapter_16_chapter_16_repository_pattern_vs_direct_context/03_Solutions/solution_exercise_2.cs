
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

// --- Specification Infrastructure ---

public interface ISpecification<T>
{
    // Applies filtering, sorting, and including to the query
    IQueryable<T> Apply(IQueryable<T> query);
}

// Concrete Specification 1: Filtering
public class OrdersByStatusSpec : ISpecification<Order>
{
    private readonly string _status;

    public OrdersByStatusSpec(string status)
    {
        _status = status;
    }

    public IQueryable<Order> Apply(IQueryable<Order> query)
    {
        return query.Where(o => o.Status == _status);
    }
}

// Concrete Specification 2: Eager Loading
public class OrdersWithItemsSpec : ISpecification<Order>
{
    public IQueryable<Order> Apply(IQueryable<Order> query)
    {
        return query.Include(o => o.Items);
    }
}

// Extension method for cleaner syntax
public static class SpecificationExtensions
{
    public static IQueryable<T> ApplySpecification<T>(this IQueryable<T> input, ISpecification<T> spec)
    {
        return spec.Apply(input);
    }
}

// --- Service Implementation ---

public class DynamicOrderQueryService
{
    private readonly AppDbContext _context;

    public DynamicOrderQueryService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Order> GetOrders(IEnumerable<ISpecification<Order>> specs)
    {
        // Start with the base IQueryable (unexecuted)
        IQueryable<Order> query = _context.Orders;

        // Apply specifications sequentially
        if (specs != null)
        {
            foreach (var spec in specs)
            {
                query = query.ApplySpecification(spec);
            }
        }

        // Return the composed query (still IQueryable, not executed yet)
        return query;
    }
}
