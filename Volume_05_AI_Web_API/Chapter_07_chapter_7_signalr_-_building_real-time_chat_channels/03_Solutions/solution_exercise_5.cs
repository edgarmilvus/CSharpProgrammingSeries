
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

// 1. The Custom Attribute
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public class RequiresPremiumAttribute : AuthorizeAttribute
{
    // This acts as a marker for our authorization handler.
}

// 2. The Authorization Handler
public class PremiumHubAuthorization : IAuthorizeHubData
{
    public bool ShouldAuthorize(HubInvocationContext context)
    {
        // Check if the method (or its declaring class) has the [RequiresPremium] attribute
        var hasAttribute = context.HubMethod.GetCustomAttributes(inherit: false).OfType<RequiresPremiumAttribute>().Any()
                        || context.Hub.GetType().GetCustomAttributes(inherit: false).OfType<RequiresPremiumAttribute>().Any();

        // If the method doesn't require premium, allow access
        if (!hasAttribute) return true;

        // If it does require premium, check the user's claims
        var user = context.Context.User;
        if (user == null) return false;

        // Check for claim: subscription = premium
        var isPremium = user.HasClaim(c => 
            c.Type == "subscription" && 
            c.Value.Equals("premium", StringComparison.OrdinalIgnoreCase));

        return isPremium;
    }
}

// 3. The Hub Implementation
[Authorize] // Base authentication required
public class AdvancedHub : Hub
{
    // Regular method - accessible to all authenticated users
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", Context.User?.Identity?.Name, message);
    }

    // 4. Decorated method - accessible only to premium users
    [RequiresPremium]
    public async IAsyncEnumerable<string> GetStreamingResponse(string prompt)
    {
        // Simulate streaming logic
        var tokens = prompt.Split(' ');
        foreach (var token in tokens)
        {
            await Task.Delay(100);
            yield return token + " ";
        }
    }
}

// File: Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add Authentication (Assuming JWT or similar is configured)
builder.Services.AddAuthentication(/* ... */); 

// 5. Registration of the custom authorization policy
builder.Services.AddSignalR(options =>
{
    // Add the instance of our authorization handler to the pipeline
    options.AuthorizationData.Add(new PremiumHubAuthorization());
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<AdvancedHub>("/advancedHub");

app.Run();
