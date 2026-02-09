
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

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public record ShipmentStatus(string TrackingNumber, bool IsFound, string Details = null);

public class ShippingPlugin
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ShippingPlugin> _logger;
    
    // Circuit Breaker State
    private int _consecutiveFailures = 0;
    private DateTime? _circuitOpenedAt = null;
    private const int CircuitFailureThreshold = 5;
    private const int CircuitResetTimeoutSeconds = 30;

    public ShippingPlugin(HttpClient httpClient, ILogger<ShippingPlugin> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    [KernelFunction("GetShipmentStatus")]
    public async Task<ShipmentStatus> GetShipmentStatusAsync(string trackingNumber)
    {
        // 1. Input Validation
        if (string.IsNullOrWhiteSpace(trackingNumber) || trackingNumber.Length != 10 || !trackingNumber.All(char.IsLetterOrDigit))
        {
            throw new ArgumentException("Tracking number must be exactly 10 alphanumeric characters.");
        }

        // 2. Circuit Breaker Check
        if (_circuitOpenedAt.HasValue && (DateTime.UtcNow - _circuitOpenedAt.Value).TotalSeconds < CircuitResetTimeoutSeconds)
        {
            throw new HttpRequestException("Circuit Open: Service unavailable");
        }
        
        // Reset if timeout passed
        if (_circuitOpenedAt.HasValue && (DateTime.UtcNow - _circuitOpenedAt.Value).TotalSeconds >= CircuitResetTimeoutSeconds)
        {
            _circuitOpenedAt = null;
            _consecutiveFailures = 0;
            _logger.LogInformation("Circuit breaker attempting to close.");
        }

        // 3. Retry Logic (Exponential Backoff)
        int retries = 0;
        while (true)
        {
            try
            {
                _logger.LogInformation("Attempt {n} for tracking {id}", retries + 1, trackingNumber);

                var response = await _httpClient.GetAsync($"https://api.shipping.com/shipment/{trackingNumber}");
                
                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    _logger.LogWarning("API returned 503");
                    throw new HttpRequestException("503"); // Trigger retry catch
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("400", null, HttpStatusCode.BadRequest);
                }

                var content = await response.Content.ReadAsStringAsync();

                // 4. Empty Response Handling
                if (string.IsNullOrWhiteSpace(content) || content == "{}")
                {
                    _logger.LogWarning("Shipment not found (empty response)");
                    return new ShipmentStatus(trackingNumber, IsFound: false);
                }

                // Reset Circuit Breaker on success
                _consecutiveFailures = 0;
                _circuitOpenedAt = null;

                // Parse and return
                var json = JsonDocument.Parse(content);
                var details = json.RootElement.GetProperty("details").GetString();
                return new ShipmentStatus(trackingNumber, IsFound: true, Details: details);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.ServiceUnavailable || ex.Message == "503")
            {
                retries++;
                if (retries > 3)
                {
                    TrackFailure();
                    throw new HttpRequestException("Service unavailable after 3 retries.");
                }
                
                // Exponential Backoff: 1s, 2s, 4s
                var delay = Math.Pow(2, retries - 1) * 1000;
                await Task.Delay((int)delay);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentException("Invalid tracking number format provided to API.");
            }
            catch (Exception)
            {
                TrackFailure();
                throw;
            }
        }
    }

    private void TrackFailure()
    {
        _consecutiveFailures++;
        if (_consecutiveFailures >= CircuitFailureThreshold)
        {
            _circuitOpenedAt = DateTime.UtcNow;
            _logger.LogError("Circuit breaker opened due to consecutive failures.");
        }
    }
}

// Usage Example
/*
var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<ShippingPlugin>();
var client = new HttpClient();
var plugin = new ShippingPlugin(client, logger);
try {
    var status = await plugin.GetShipmentStatusAsync("1234567890"); // Invalid length -> Exception
} catch (Exception e) { Console.WriteLine(e.Message); }
*/
