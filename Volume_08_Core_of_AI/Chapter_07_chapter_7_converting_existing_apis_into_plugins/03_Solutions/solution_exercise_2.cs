
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

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

// Custom Exception
public class BankingApiException : Exception
{
    public BankingApiException(string message) : base(message) { }
}

// 1. Custom DelegatingHandler for Auth & Token Refresh
public class BankingAuthHandler : DelegatingHandler
{
    private string _accessToken;
    private readonly Func<Task<string>> _tokenRefreshFunc;
    private readonly string _clientId;

    public BankingAuthHandler(string initialToken, string clientId, Func<Task<string>> tokenRefreshFunc)
    {
        _accessToken = initialToken;
        _clientId = clientId;
        _tokenRefreshFunc = tokenRefreshFunc;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Attach headers
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
        request.Headers.Add("X-Client-ID", _clientId);

        var response = await base.SendAsync(request, cancellationToken);

        // 2. Interactive Challenge: Token Refresh Logic
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Refresh token
            var newToken = await _tokenRefreshFunc();
            if (!string.IsNullOrEmpty(newToken))
            {
                _accessToken = newToken;
                // Update request header and retry
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
                response = await base.SendAsync(request, cancellationToken);
            }
        }

        return response;
    }
}

// 3. Manual Plugin Construction
public class BankingPlugin
{
    private readonly Kernel _kernel;
    private readonly HttpClient _httpClient;

    public BankingPlugin(Kernel kernel, HttpClient httpClient)
    {
        _kernel = kernel;
        _httpClient = httpClient;
    }

    public void RegisterFunctions()
    {
        // We manually construct RestApiOperation to define the specific JSON structure required.
        // Note: In newer SK versions, this might be done via KernelPluginFactory or direct KernelFunction creation.
        
        // Balance Check Operation
        var balanceOp = new RestApiOperation(
            id: "GetAccountBalance",
            method: HttpMethod.Post,
            path: "/api/v1/query",
            description: "Gets account balance",
            parameters: new List<RestApiParameter>
            {
                new RestApiParameter("account_id", "string", true, false, "The account ID"),
                // 'action' is fixed, so we treat it as a constant or default value in the runner logic.
                // However, RestApiOperation typically maps inputs. We will handle the body construction in the execution.
            },
            requestBody: null // We will construct body dynamically
        );

        // History Check Operation
        var historyOp = new RestApiOperation(
            id: "GetTransactionHistory",
            method: HttpMethod.Post,
            path: "/api/v1/query",
            description: "Gets transaction history",
            parameters: new List<RestApiParameter>
            {
                new RestApiParameter("account_id", "string", true, false, "The account ID"),
                new RestApiParameter("limit", "integer", false, false, "Max records")
            },
            requestBody: null
        );

        // Register as Kernel Functions (Simulating the runner logic)
        _kernel.Plugins.Add(CreatePluginFromOps(balanceOp, historyOp));
    }

    private KernelPlugin CreatePluginFromOps(params RestApiOperation[] ops)
    {
        var plugin = new KernelPlugin("Banking");
        foreach (var op in ops)
        {
            // Map the RestApiOperation to a KernelFunction
            var func = KernelFunctionFactory.CreateFromMethod(
                async (KernelArguments args) =>
                {
                    // Construct JSON Body based on operation ID
                    var body = new Dictionary<string, object>();
                    
                    if (op.Id == "GetAccountBalance")
                    {
                        body["action"] = "balance";
                        body["account_id"] = args["account_id"];
                    }
                    else if (op.Id == "GetTransactionHistory")
                    {
                        body["action"] = "history";
                        body["account_id"] = args["account_id"];
                        if (args.ContainsKey("limit"))
                            body["limit"] = args["limit"];
                    }

                    var jsonBody = JsonSerializer.Serialize(body);
                    var content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");

                    var request = new HttpRequestMessage(op.Method, op.Path) { Content = content };
                    
                    // Send via the HttpClient (which has the AuthHandler attached)
                    var response = await _httpClient.SendAsync(request);
                    var responseString = await response.Content.ReadAsStringAsync();

                    // Response Parsing
                    var doc = JsonDocument.Parse(responseString);
                    if (doc.RootElement.TryGetProperty("status", out var status) && status.GetString() == "error")
                    {
                        throw new BankingApiException($"API Error: {doc.RootElement.GetProperty("message").GetString()}");
                    }

                    return responseString; // Return raw JSON or deserialize further
                },
                new KernelFunctionMetadata(op.Id)
                {
                    Parameters = op.Parameters.Select(p => new KernelParameterMetadata(p.Name) { ParameterType = typeof(string) }).ToList()
                }
            );
            plugin.AddFunction(func);
        }
        return plugin;
    }
}

// Usage Example
/*
var token = "initial_token";
var refreshFunc = async () => { await Task.Delay(100); return "new_token"; }; // Simulate network call
var handler = new BankingAuthHandler(token, "client_123", refreshFunc);
var client = new HttpClient(handler);
var kernel = new Kernel();
var plugin = new BankingPlugin(kernel, client);
plugin.RegisterFunctions();

// Execute
var result = await kernel.InvokeAsync<string>("Banking", "GetAccountBalance", new KernelArguments { ["account_id"] = "12345" });
*/
