
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

using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Grpc.Net.Client;

var handler = new HttpClientHandler();

// Load Client Certificate (Identity)
var clientCert = new X509Certificate2("client.pfx", "password");
handler.ClientCertificates.Add(clientCert);

// Custom Server Certificate Validation (Trust)
handler.RemoteCertificateValidationCallback = (sender, cert, chain, errors) =>
{
    if (errors != SslPolicyErrors.None) return false;
    
    // Verify against our specific CA
    var caCert = new X509Certificate2("ca.crt");
    var customChain = new X509Chain();
    customChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
    customChain.ChainPolicy.ExtraStore.Add(caCert);
    customChain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
    
    if (!customChain.Build(new X509Certificate2(cert))) return false;
    
    // Verify the root is our CA
    var root = customChain.ChainElements[customChain.ChainElements.Count - 1].Certificate;
    return root.Thumbprint == caCert.Thumbprint;
};

var channel = GrpcChannel.ForAddress("https://inference-service:5001", new GrpcChannelOptions { HttpHandler = handler });
var client = new InferenceService.InferenceServiceClient(channel);
