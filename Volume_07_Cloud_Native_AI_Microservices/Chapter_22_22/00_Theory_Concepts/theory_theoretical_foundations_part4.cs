
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

# Source File: theory_theoretical_foundations_part4.cs
# Description: Theoretical Foundations
# ==========================================

using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Trace;

// C# code to instrument an AI agent for distributed tracing.
// This allows us to see the "lifecycle" of a request across the mesh.

public class InstrumentedAgent
{
    private static readonly ActivitySource MyActivitySource = new ActivitySource("AI.Agent");

    public string ProcessRequest(string prompt)
    {
        // Start a new activity (span) for this inference step.
        using var activity = MyActivitySource.StartActivity("InferenceStep");

        if (activity != null)
        {
            activity.SetTag("agent.type", "retriever");
            activity.SetTag("prompt.length", prompt.Length);
        }

        // Simulate processing
        var result = "Retrieved context for: " + prompt;

        // The service mesh (sidecar) will automatically capture the outbound HTTP call
        // if we use standard HttpClient, but we can add custom events here.
        activity?.AddEvent(new ActivityEvent("ContextRetrieved"));

        return result;
    }
}
