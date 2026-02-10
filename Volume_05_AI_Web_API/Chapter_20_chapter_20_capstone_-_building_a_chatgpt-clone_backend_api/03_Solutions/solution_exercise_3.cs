
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

// Source File: solution_exercise_3.cs
// Description: Solution for Exercise 3
// ==========================================

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

// 1. Claims Transformation
public class TenantClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Clone the identity to avoid modifying the original immutable principal
        var clone = principal.Clone();
        var newIdentity = (ClaimsIdentity)clone.Identity;

        // Check if the request has a TenantId header (simulated here via context)
        // In a real scenario, you'd inject IHttpContextAccessor
        // For this example, assume we can access the request headers via context
        // Note: IClaimsTransformation is called frequently, so logic must be efficient.
        
        // Mock extraction logic (implementation depends on IHttpContextAccessor injection)
        // If TenantId exists in JWT, we might just copy it. 
        // If it's in a header (as per requirement), we add it here.
        
        // Example: Adding a claim if it doesn't exist
        if (!newIdentity.HasClaim(c => c.Type == "TenantId"))
        {
            // In a real app, retrieve from HttpContext.Request.Headers["X-Tenant-ID"]
            // For this solution, we simulate finding a tenant ID.
            string? tenantId = "tenant-123"; // Placeholder for actual extraction logic
            
            if (!string.IsNullOrEmpty(tenantId))
            {
                newIdentity.AddClaim(new Claim("TenantId", tenantId));
            }
        }

        return Task.FromResult(clone);
    }
}

// 2. Policy Definition (In Program.cs)
public static class AuthPolicies
{
    public const string RequireProTier = "RequireProTier";
    public const string RequireTenantAccess = "RequireTenantAccess";

    public static void AddPolicies(AuthorizationOptions options)
    {
        // Policy: Require Pro Tier
        options.AddPolicy(RequireProTier, policy =>
            policy.RequireClaim("SubscriptionTier", "Pro"));

        // Policy: Require Tenant Access
        options.AddPolicy(RequireTenantAccess, policy =>
            policy.RequireClaim("TenantId"));
    }
}

// 3. Endpoint Protection (Controller Example)
[ApiController]
[Route("api/[controller]")]
[Authorize] // Base authentication
public class ChatController : ControllerBase
{
    [HttpPost("stream")]
    [Authorize(Policy = AuthPolicies.RequireTenantAccess)]
    public async IAsyncEnumerable<ChatResponseChunk> StreamChat([FromBody] ChatRequest request)
    {
        // Logic here
        yield return new ChatResponseChunk();
    }
}

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    [HttpPost("models")]
    [Authorize(Policy = AuthPolicies.RequireProTier)] // Combines with class-level auth
    [Authorize(Policy = AuthPolicies.RequireTenantAccess)]
    public IActionResult ManageModels()
    {
        return Ok();
    }
}
