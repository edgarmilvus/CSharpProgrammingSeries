
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

// File: Program.cs (Server Configuration)
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Hosting;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureKestrel(options =>
                {
                    // Listen on port 5001 for gRPC (HTTPS)
                    options.ListenAnyIP(5001, listenOptions =>
                    {
                        // Configure HTTPS with mTLS
                        listenOptions.UseHttps(httpsOptions =>
                        {
                            // 1. Load the Server Certificate (InferenceService)
                            // In production, load from a secure store (Azure Key Vault, etc.)
                            var serverCert = new X509Certificate2("server.pfx", "password");
                            httpsOptions.ServerCertificate = serverCert;

                            // 2. Configure Client Certificate Validation
                            httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                            
                            // 3. Validate the Client Certificate against a trusted CA
                            httpsOptions.ClientCertificateValidation = (certificate, chain, policy) =>
                            {
                                // Load the Root CA that signed the client cert
                                var rootCA = new X509Certificate2("rootCA.crt");

                                // Build a custom chain to validate against our specific Root CA
                                using var customChain = new X509Chain();
                                customChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck; // For self-signed demo
                                customChain.ChainPolicy.ExtraStore.Add(rootCA);
                                customChain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

                                bool isValid = customChain.Build(certificate);

                                // Verify that the root of the chain is our trusted Root CA
                                if (isValid && customChain.ChainElements.Count > 0)
                                {
                                    var root = customChain.ChainElements[customChain.ChainElements.Count - 1].Certificate;
                                    isValid = root.Thumbprint == rootCA.Thumbprint;
                                }

                                return isValid;
                            };
                        });
                    });
                });
                
                webBuilder.UseStartup<Startup>();
            });
}

// File: Middleware/ClientCertificateMiddleware.cs
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

public class ClientCertificateMiddleware
{
    private readonly RequestDelegate _next;

    public ClientCertificateMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Connection.ClientCertificate != null)
        {
            var cert = context.Connection.ClientCertificate;
            // Log the Subject Common Name (CN)
            var cn = cert.Subject.Split(',').FirstOrDefault(s => s.Trim().StartsWith("CN="))?.Split('=')[1];
            Console.WriteLine($"[Auth] Request received from Client CN: {cn}");
        }
        else
        {
            Console.WriteLine("[Auth] No client certificate provided.");
        }

        await _next(context);
    }
}
