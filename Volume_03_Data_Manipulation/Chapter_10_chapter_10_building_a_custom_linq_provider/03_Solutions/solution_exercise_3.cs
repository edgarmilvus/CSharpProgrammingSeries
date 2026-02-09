
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

# Source File: solution_exercise_3.cs
# Description: Solution for Exercise 3
# ==========================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

// 1. The Extension Method Definition (The "Front End")
public static class VectorQueryableExtensions
{
    // This method has no implementation body. It exists only to be represented in the Expression Tree.
    public static IOrderedQueryable<T> OrderByDistanceTo<T>(this IQueryable<T> source, float[] targetVector)
    {
        return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(
            Expression.Call(
                typeof(VectorQueryableExtensions).GetMethod("OrderByDistanceTo", new[] { typeof(IQueryable<T>), typeof(float[]) }),
                source.Expression,
                Expression.Constant(targetVector)
            )
        );
    }
}

// 2. The Provider Modifications (The "Back End")
public class VectorQueryProvider : IQueryProvider
{
    // ... (Assume existing Execute<T> and CreateQuery methods) ...

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new VectorQueryable<TElement>(this, expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        // CRITICAL: We need to inspect the expression tree here.
        // In a real provider, this is a recursive Visitor pattern.
        
        if (expression is MethodCallExpression methodCall)
        {
            // Check if the method is our custom OrderByDistanceTo
            if (methodCall.Method.Name == "OrderByDistanceTo")
            {
                // 1. Extract the source collection (the first argument)
                var sourceExpression = methodCall.Arguments[0];
                // In a real scenario, we would recursively execute the source expression to get the data
                var sourceQuery = this.CreateQuery<float[]>(sourceExpression); // Assuming source is IQueryable<float[]>
                var data = sourceQuery.ToList(); // Immediate Execution to get data

                // 2. Extract the target vector (the second argument)
                var targetVector = (float[])((ConstantExpression)methodCall.Arguments[1]).Value;

                // 3. Perform the Sort (Using the logic from Exercise 2)
                var sorted = data
                    .Select(v => new { Vector = v, Score = VectorMath.CosineSimilarity(v, targetVector) })
                    .OrderByDescending(x => x.Score) // Highest similarity is closest distance
                    .Select(x => x.Vector)
                    .ToList();

                // 4. Cast back to TResult (usually List<T>)
                return (TResult)sorted;
            }
        }

        // Fallback for standard operations (like Where)
        throw new NotImplementedException("Only OrderByDistanceTo is implemented in this exercise snippet.");
    }
    
    // Boilerplate IQueryProvider methods omitted for brevity...
    public IQueryable CreateQuery(Expression expression) => throw new NotImplementedException();
    public object Execute(Expression expression) => throw new NotImplementedException();
}
