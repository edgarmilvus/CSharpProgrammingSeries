
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

# Source File: project_advanced_application_script.cs
# Description: Advanced Application Script
# ==========================================

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SecureAIChatGateway
{
    // Real-World Context:
    // Imagine a company deploying a proprietary Large Language Model (LLM) internally for developers.
    // Access must be strictly controlled. We need two layers:
    // 1. User Authentication (JWT): For developers logging into the dashboard to test the API.
    // 2. Service Authentication (API Key): For automated CI/CD pipelines that invoke the AI model programmatically.
    // This console app simulates the "Gateway" logic that validates these credentials before allowing a chat request.

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- Secure AI Chat Gateway Simulation ---");

            // 1. SETUP: Initialize our "Database" of users and API keys.
            // In a real app, these would be stored securely in a database (hashed).
            string secretKey = "SuperSecretKeyForJWTSigning_DoNotShare";
            List<User> users = InitializeUsers();
            List<ApiKey> apiKeys = InitializeApiKeys();

            // 2. SCENARIO 1: User tries to access chat via JWT.
            Console.WriteLine("\n[Scenario 1] User Login (JWT)");
            string jwtToken = GenerateJWT("alice_dev", "Developer", secretKey);
            Console.WriteLine($"Generated JWT: {jwtToken}");
            
            bool jwtValid = ValidateJWT(jwtToken, secretKey, users);
            Console.WriteLine($"JWT Validation Result: {jwtValid}");

            // 3. SCENARIO 2: Automated Service tries to access chat via API Key.
            Console.WriteLine("\n[Scenario 2] Automated Service (API Key)");
            string serviceKey = "srv_99887766554433221100";
            Console.WriteLine($"Using API Key: {serviceKey}");

            bool apiKeyValid = ValidateApiKey(serviceKey, apiKeys);
            Console.WriteLine($"API Key Validation Result: {apiKeyValid}");

            // 4. SCENARIO 3: Malicious attempt (Expired/Invalid JWT).
            Console.WriteLine("\n[Scenario 3] Expired Token Attempt");
            // Simulating an expired token structure (shortened for simulation)
            string expiredToken = GenerateExpiredJWT("bob_intern", "Intern", secretKey);
            Console.WriteLine($"Generated Expired JWT: {expiredToken}");
            
            bool expiredValid = ValidateJWT(expiredToken, secretKey, users);
            Console.WriteLine($"Expired JWT Validation Result: {expiredValid}");
        }

        // --- DATA MODELS (Basic Structs/Classes) ---

        class User
        {
            public string Username { get; set; }
            public string Role { get; set; }
        }

        class ApiKey
        {
            public string Key { get; set; }
            public string ServiceName { get; set; }
            public bool IsActive { get; set; }
        }

        // --- INITIALIZATION LOGIC ---

        static List<User> InitializeUsers()
        {
            return new List<User>
            {
                new User { Username = "alice_dev", Role = "Developer" },
                new User { Username = "bob_intern", Role = "Intern" }
            };
        }

        static List<ApiKey> InitializeApiKeys()
        {
            return new List<ApiKey>
            {
                new ApiKey { Key = "srv_99887766554433221100", ServiceName = "CI/CD Pipeline", IsActive = true },
                new ApiKey { Key = "srv_11223344556677889900", ServiceName = "Legacy System", IsActive = false }
            };
        }

        // --- JWT IMPLEMENTATION (Simplified for Console) ---

        static string GenerateJWT(string username, string role, string secret)
        {
            // Header (Base64 encoded): { "alg": "HS256", "typ": "JWT" }
            string header = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));

            // Payload (Base64 encoded): { "sub": username, "role": role, "exp": current_time + 1_hour }
            // Note: In a real app, use DateTime.UtcNow for standardization.
            long expTicks = DateTime.Now.AddHours(1).Ticks;
            string payload = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{{\"sub\":\"{username}\",\"role\":\"{role}\",\"exp\":{expTicks}}}"));

            // Signature: HMACSHA256(Base64UrlEncode(header) + "." + Base64UrlEncode(payload), secret)
            string dataToSign = header + "." + payload;
            string signature = ComputeHmacSha256(dataToSign, secret);

            return $"{header}.{payload}.{signature}";
        }

        static string GenerateExpiredJWT(string username, string role, string secret)
        {
            // Simulating an expired token by setting time to the past
            string header = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));
            long expTicks = DateTime.Now.AddHours(-1).Ticks; // 1 hour in the past
            string payload = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{{\"sub\":\"{username}\",\"role\":\"{role}\",\"exp\":{expTicks}}}"));
            
            string dataToSign = header + "." + payload;
            string signature = ComputeHmacSha256(dataToSign, secret);

            return $"{header}.{payload}.{signature}";
        }

        static bool ValidateJWT(string token, string secret, List<User> users)
        {
            try
            {
                string[] parts = token.Split('.');
                if (parts.Length != 3) return false;

                string header = parts[0];
                string payload = parts[1];
                string signature = parts[2];

                // 1. Verify Signature
                string expectedSignature = ComputeHmacSha256($"{header}.{payload}", secret);
                if (signature != expectedSignature) return false;

                // 2. Decode Payload to check Expiration
                string payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                
                // Basic parsing without Regex/LINQ (using manual string search for simplicity in this exercise)
                // Finding "exp": value
                int expIndex = payloadJson.IndexOf("\"exp\":");
                if (expIndex == -1) return false;
                
                string expValueStr = "";
                int start = expIndex + 6; // length of "\"exp\":" 
                while(start < payloadJson.Length && char.IsDigit(payloadJson[start]))
                {
                    expValueStr += payloadJson[start];
                    start++;
                }

                long expTicks = long.Parse(expValueStr);
                if (DateTime.Now.Ticks > expTicks) return false;

                // 3. Check if User Exists
                int subIndex = payloadJson.IndexOf("\"sub\":\"");
                if (subIndex == -1) return false;
                
                int subStart = subIndex + 7;
                int subEnd = payloadJson.IndexOf("\"", subStart);
                string username = payloadJson.Substring(subStart, subEnd - subStart);

                bool userExists = false;
                foreach (var user in users)
                {
                    if (user.Username == username)
                    {
                        userExists = true;
                        break;
                    }
                }

                return userExists;
            }
            catch
            {
                return false;
            }
        }

        // --- API KEY IMPLEMENTATION ---

        static bool ValidateApiKey(string key, List<ApiKey> validKeys)
        {
            foreach (var apiKey in validKeys)
            {
                if (apiKey.Key == key && apiKey.IsActive)
                {
                    return true;
                }
            }
            return false;
        }

        // --- CRYPTO HELPER ---

        static string ComputeHmacSha256(string data, string secret)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hash);
            }
        }
    }
}
