
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

# Source File: basic_basic_code_example.cs
# Description: Basic Code Example
# ==========================================

using System;

// 1. Class Definition
// This defines the blueprint for a User object.
public class User
{
    // 2. Fields (Private)
    // These variables store the data. They are private to prevent 
    // direct modification from outside the class.
    private string _username;
    private string _email;

    // 3. Properties (Public)
    // These allow controlled access to the fields.
    // We use 'get' so other parts of the program can read the values.
    // Notice there is NO 'set' here. We cannot change these after creation.
    public string Username 
    { 
        get { return _username; } 
    }

    public string Email 
    { 
        get { return _email; } 
    }

    // 4. The Constructor
    // This special method has the same name as the class and no return type.
    // It accepts parameters for the mandatory fields.
    public User(string username, string email)
    {
        // 5. Assignment
        // We assign the parameters to the private fields.
        // This ensures the object is created in a valid state.
        _username = username;
        _email = email;
    }

    // 6. Helper Method
    // A simple method to display the user's info.
    public void DisplayInfo()
    {
        Console.WriteLine($"User: {Username}, Email: {Email}");
    }
}

// 7. Main Program
public class Program
{
    public static void Main()
    {
        // 8. Creating an Instance
        // We use the 'new' keyword followed by the constructor arguments.
        // This guarantees that _username and _email are set immediately.
        User user1 = new User("AliceDev", "alice@example.com");

        // 9. Accessing Properties
        // We can read the values, but we cannot change them directly
        // because there is no 'set' accessor.
        Console.WriteLine($"Retrieved Username: {user1.Username}");

        // 10. Using the Method
        user1.DisplayInfo();

        // 11. Attempting to create an invalid user (This would cause an error)
        // If we tried: User user2 = new User(); 
        // The compiler would stop us because the constructor requires arguments.
    }
}
