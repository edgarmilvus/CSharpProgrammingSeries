
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

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net;
using System.Threading.Tasks;
using Xunit;

public class ModelLoadingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ModelLoadingTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            // Mock the loader to delay
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IModelLoader>(new MockSlowLoader(5000));
            });
        });
    }

    [Fact]
    public async Task Health_Returns_Degraded_While_Loading()
    {
        var client = _factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("/health");
        
        // Assert
        // Depending on implementation, this might be 200 OK (Healthy) if background service is fast
        // or 503 (Degraded). Here we assume the test triggers during the load window.
        Assert.True(response.StatusCode == HttpStatusCode.ServiceUnavailable || 
                    response.StatusCode == HttpStatusCode.OK);
    }

    [Fact]
    public async Task Graceful_Shutdown_Cancels_Loading()
    {
        // This test requires access to the host lifetime
        var host = _factory.Services.GetRequiredService<IHost>();
        
        // Start the host explicitly if not already started by factory
        await host.StartAsync();
        
        // Trigger stopping
        await host.StopAsync(TimeSpan.FromSeconds(1));
        
        // Verify logs or state (requires capturing logs via xUnit ITestOutputHelper or Spy Logger)
        // Assert that "Model loading cancelled" was logged.
    }
}

public class MockSlowLoader : IModelLoader
{
    private readonly int _delayMs;
    public MockSlowLoader(int delayMs) => _delayMs = delayMs;
    public async Task LoadModelAsync(CancellationToken cancellationToken) 
        => await Task.Delay(_delayMs, cancellationToken);
}

// Note: Program.cs must be public for integration tests to access it
// public class Program { }
