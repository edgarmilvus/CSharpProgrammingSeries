
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

// Source File: solution_exercise_8.cs
// Description: Solution for Exercise 8
// ==========================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

// 1. Query Interceptor
public class VectorQueryInterceptor : IQueryInterceptor
{
    public Expression QueryCompilationContextFactoryCreating(QueryCompilationContextFactoryCreatingEvent data)
    {
        // We can modify the factory here if needed, but usually we intercept the expression tree
        return data.Expression;
    }

    public Expression OnExpressionCreated(Expression expression)
    {
        // 2. Query Visitor to identify and rewrite vector ops
        var visitor = new VectorRewritingVisitor();
        return visitor.Visit(expression);
    }
}

// 3. Query Visitor
public class VectorRewritingVisitor : ExpressionVisitor
{
    // Detects calls to our custom similarity method
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method.Name == "NearestNeighbors")
        {
            // Original: context.Documents.NearestNeighbors(queryVector, 10)
            // We rewrite this into a form EF Core can translate, or separate into two queries.
            
            // For this exercise, we rewrite it to a standard Where/OrderBy/Take
            // but injecting the specific SQL logic if possible.
            
            var queryArg = node.Arguments[0]; // The IQueryable
            var vectorArg = node.Arguments[1]; // The query vector
            var takeArg = node.Arguments[2];   // Top K

            // We can't easily inject raw SQL into an Expression Tree without a custom QueryProvider.
            // Instead, we transform it into a call to a static method that EF Core *can* translate (DbFunction).
            
            // However, a common pattern for unsupported functions is to split the query:
            // 1. Fetch candidates
            // 2. Calculate similarity in memory (Client-side)
            // 3. Filter and Order
            
            // To force client-side evaluation for the specific part:
            return Expression.Call(
                typeof(Enumerable), 
                nameof(Enumerable.ToList), 
                new[] { node.Type },
                node
            );
        }

        return base.VisitMethodCall(node);
    }
}

// 4. Caching Query Plans
public class CachedQueryProvider
{
    private static readonly ConcurrentDictionary<string, Func<IQueryable>> _cache = new();

    public IQueryable<T> GetCachedQuery<T>(IQueryable<T> baseQuery, string key)
    {
        if (_cache.TryGetValue(key, out var factory))
        {
            return (IQueryable<T>)factory();
        }
        
        // Compile and cache
        var compiled = baseQuery.Expression; 
        // Note: Full implementation requires compiling the expression tree
        _cache[key] = () => baseQuery; 
        return baseQuery;
    }
}

// Extension method for the exercise requirement
public static class QueryableExtensions
{
    public static IQueryable<Document> NearestNeighbors(this IQueryable<Document> source, float[] queryVector, int topK)
    {
        // This method is just a marker. The Visitor intercepts it.
        throw new NotImplementedException("This is a marker method for the interceptor.");
    }
}

// 5. Telemetry Interceptor
public class TelemetryInterceptor : DbCommandInterceptor
{
    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        // Track vector DB latency here if command is specific to vector DB
        Console.WriteLine($"Executing SQL: {command.CommandText}");
        return base.ReaderExecuting(command, eventData, result);
    }
}
