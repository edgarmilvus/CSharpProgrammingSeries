
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

# Source File: solution_exercise_9.cs
# Description: Solution for Exercise 9
# ==========================================

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public class SingleThreadSynchronizationContext : SynchronizationContext
{
    private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> _queue;
    private readonly Thread _thread;
    private readonly int _threadId;

    public SingleThreadSynchronizationContext()
    {
        _queue = new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();
        _thread = new Thread(() =>
        {
            // Set the context for this thread
            SynchronizationContext.SetSynchronizationContext(this);
            
            // Message loop
            foreach (var item in _queue.GetConsumingEnumerable())
            {
                item.Key(item.Value);
            }
        });
        _thread.Start();
        _threadId = _thread.ManagedThreadId;
        Console.WriteLine($"[Custom Context] Started dedicated thread ID: {_threadId}");
    }

    public override void Post(SendOrPostCallback d, object state)
    {
        _queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
    }

    public override void Send(SendOrPostCallback d, object state)
    {
        if (Thread.CurrentThread.ManagedThreadId == _threadId)
        {
            d(state);
        }
        else
        {
            var mre = new ManualResetEventSlim(false);
            Post(_ =>
            {
                try { d(state); }
                finally { mre.Set(); }
            }, null);
            mre.Wait();
        }
    }

    public void Complete()
    {
        _queue.CompleteAdding();
        _thread.Join();
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        // Install our custom context
        var context = new SingleThreadSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(context);

        Console.WriteLine($"Main Thread ID: {Thread.CurrentThread.ManagedThreadId}");

        // Run an async operation
        await Task.Run(async () =>
        {
            Console.WriteLine($"[Background Task] Running on Thread ID: {Thread.CurrentThread.ManagedThreadId}");
            
            // Simulate work
            await Task.Delay(500);
            
            // Post a result back to the custom context
            // This mimics IProgress<T> or UI property update
            SynchronizationContext.Current.Post(_ => 
            {
                Console.WriteLine($"[Callback] Executed on Thread ID: {Thread.CurrentThread.ManagedThreadId}");
                Console.WriteLine($"[Callback] Is this the dedicated UI thread? {Thread.CurrentThread.ManagedThreadId == context._threadId}");
            }, null);
        });

        // Cleanup the message loop
        context.Complete();
    }
}
