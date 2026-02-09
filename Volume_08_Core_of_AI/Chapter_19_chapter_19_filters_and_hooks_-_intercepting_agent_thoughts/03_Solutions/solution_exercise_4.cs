
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

# Source File: solution_exercise_4.cs
# Description: Solution for Exercise 4
# ==========================================

using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Abstraction for DateTime to allow unit testing
public interface ITimeProvider
{
    DateTime Now { get; }
}

public class RealTimeProvider : ITimeProvider
{
    public DateTime Now => DateTime.Now;
}

public class ContextEnricherFilter : IKernelFilter
{
    private readonly ITimeProvider _timeProvider;

    public ContextEnricherFilter(ITimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public Task InvokeAsync(KernelContext context, Func<KernelContext, Task> next)
    {
        // 1. Check time of day
        var now = _timeProvider.Now;
        bool isBusinessHours = now.Hour >= 9 && now.Hour < 17;

        // 2. Determine message
        string message = isBusinessHours 
            ? "User is in a business context. Prioritize formal tone." 
            : "User is off-hours. Keep responses concise.";

        // 3. Inject context safely (don't overwrite if exists)
        // We use "system_prompt" as the key
        if (!context.Arguments.ContainsKey("system_prompt"))
        {
            context.Arguments["system_prompt"] = message;
        }

        // 4. Proceed
        return next(context);
    }
}

// --- Graphviz DOT Visualization (Requirement 4) ---
/*
digraph Workflow {
    rankdir=LR;
    node [shape=box];

    User [shape=ellipse];
    Kernel [shape=cylinder];
    Planner [shape=component];

    User -> "ContextEnricherFilter" [label="Input"];
    "ContextEnricherFilter" -> "LoggingFilter" [label="Modified Context"];
    "LoggingFilter" -> Planner [label="Pass"];
    Planner -> Kernel [label="Function Invocation"];
    Kernel -> User [label="Response"];
}
*/
