
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

using System;
using System.Text;
using System.Text.Json;

namespace PrivacyAudit
{
    public class SensitiveData
    {
        public string SSN { get; set; }
        public string MedicalRecord { get; set; }
        public string FinancialData { get; set; }
    }

    public class PrivacyAuditor
    {
        private readonly StringBuilder _log = new StringBuilder();

        public void LogDataAccess(string dataSource, bool isExternal, long payloadSizeBytes = 0)
        {
            string type = isExternal ? "EXTERNAL (Network)" : "INTERNAL (Local)";
            string sizeInfo = payloadSizeBytes > 0 ? $" | Payload Size: {payloadSizeBytes} bytes" : "";
            _log.AppendLine($"[AUDIT] Source: {dataSource} | Type: {type}{sizeInfo}");
        }

        public void GenerateReport()
        {
            Console.WriteLine("\n--- Privacy Audit Report ---");
            Console.WriteLine(_log.ToString());
            Console.WriteLine("----------------------------");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var auditor = new PrivacyAuditor();

            // 1. User Input
            Console.WriteLine("Enter SSN:");
            string ssn = Console.ReadLine() ?? "123-45-6789";
            Console.WriteLine("Enter Medical Record Summary:");
            string med = Console.ReadLine() ?? "Patient has high blood pressure.";
            Console.WriteLine("Enter Financial Data:");
            string fin = Console.ReadLine() ?? "Balance: $50,000";

            var data = new SensitiveData
            {
                SSN = ssn,
                MedicalRecord = med,
                FinancialData = fin
            };

            // 2. Local Processing Simulation
            Console.WriteLine("\nProcessing Locally...");
            ProcessLocally(data, auditor);

            // 3. Cloud Processing Simulation
            Console.WriteLine("Processing in Cloud...");
            ProcessInCloud(data, auditor);

            // 4. Generate Report
            auditor.GenerateReport();
        }

        static void ProcessLocally(SensitiveData data, PrivacyAuditor auditor)
        {
            // Simulate local inference logic
            // No network calls are made.
            // Data never leaves the process memory space.
            auditor.LogDataAccess("LocalInferenceEngine", isExternal: false);
        }

        static void ProcessInCloud(SensitiveData data, PrivacyAuditor auditor)
        {
            // Serialize data
            string json = JsonSerializer.Serialize(data);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            
            // Simulate sending over network
            // In a real app, this would be HttpClient.PostAsync(...)
            auditor.LogDataAccess("RemoteAPI/InferenceEndpoint", isExternal: true, payloadSizeBytes: bytes.Length);
        }
    }
}
