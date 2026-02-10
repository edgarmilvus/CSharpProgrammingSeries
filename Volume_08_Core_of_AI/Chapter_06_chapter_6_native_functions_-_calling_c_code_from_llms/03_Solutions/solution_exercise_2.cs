
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
using System.ComponentModel;

namespace UserProfilePlugin
{
    // 1. Define the domain models and interface
    public record UserProfile(int Id, string Name, string SubscriptionLevel);

    public interface IUserService
    {
        Task<UserProfile?> GetUserAsync(int userId);
    }

    // 2. Concrete implementation simulating a database
    public class MockUserService : IUserService
    {
        private static readonly Dictionary<int, UserProfile> _users = new()
        {
            { 1, new UserProfile(1, "Alice", "Premium") },
            { 2, new UserProfile(2, "Bob", "Standard") }
        };

        public Task<UserProfile?> GetUserAsync(int userId)
        {
            // Simulate async DB call
            _users.TryGetValue(userId, out var user);
            return Task.FromResult(user);
        }
    }

    public class UserProfilePlugin
    {
        // 3. & 4. The function accepts the interface as a parameter.
        // The Kernel's DI container resolves 'IUserService' automatically.
        [KernelFunction, Description("Retrieves a user profile by ID.")]
        public async Task<UserProfile?> GetUserProfileAsync(
            [Description("The ID of the user")] int userId,
            IUserService userService)
        {
            return await userService.GetUserAsync(userId);
        }
    }

    // 5. Architectural Nuance: Configuration Example
    // This is how the Kernel instance would be configured in your application startup:
    /*
        var builder = Kernel.CreateBuilder();
        
        // Register the service with the .NET DI container
        builder.Services.AddSingleton<IUserService, MockUserService>();
        
        // Add the plugin (the plugin class doesn't need to register the service, 
        // the Kernel does)
        builder.Plugins.AddFromType<UserProfilePlugin>();
        
        var kernel = builder.Build();
    */
}
