
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

// Source File: theory_theoretical_foundations_part2.cs
// Description: Theoretical Foundations
// ==========================================

using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace CloudNativeAgents.Observability
{
    public class AgentCommunicator
    {
        private readonly HttpClient _httpClient;

        public AgentCommunicator(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // The ActivitySource is part of the System.Diagnostics namespace
        // and is the modern way to handle tracing in .NET.
        private static readonly ActivitySource MyActivitySource = new("AgentSwarm");

        public async Task<string> QueryPeerAsync(string peerUrl, string query)
        {
            // Start a new activity (span) for this specific interaction.
            using var activity = MyActivitySource.StartActivity("QueryPeer");
            
            // The 'using' statement ensures the activity is disposed (ended) correctly,
            // capturing timing and status.
            
            activity?.SetTag("peer.url", peerUrl);
            activity?.SetTag("query.length", query.Length);

            // In a real scenario, the OpenTelemetry instrumentation for HttpClient
            // automatically injects the 'traceparent' header into the request.
            // This relies on the async context being preserved.
            var response = await _httpClient.GetAsync($"{peerUrl}?q={query}");
            
            // Error handling that updates the trace status
            if (!response.IsSuccessStatusCode)
            {
                activity?.SetStatus(ActivityStatusCode.Error);
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
