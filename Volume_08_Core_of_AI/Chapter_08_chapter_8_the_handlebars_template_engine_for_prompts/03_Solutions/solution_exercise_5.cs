
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

# Source File: solution_exercise_5.cs
# Description: Solution for Exercise 5
# ==========================================

using HandlebarsDotNet;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FinancialReportGenerator
{
    // 1. Define Data Models
    public class QuarterlyMetrics
    {
        public double Revenue { get; set; }
        public double Expenses { get; set; }
        public double Growth { get; set; }
    }

    public class QuarterlyReport
    {
        public string Quarter { get; set; }
        public QuarterlyMetrics Metrics { get; set; }
    }

    public static async Task GenerateAnalysisAsync()
    {
        // 2. Handlebars Template with Complex Logic
        string template = """
            Analyze the following financial data for {{report.Quarter}}:
            
            Revenue: ${{report.Metrics.Revenue}}
            Expenses: ${{report.Metrics.Expenses}}
            Growth: {{report.Metrics.Growth}}%

            Analysis:
            {{#if (gt report.Metrics.Revenue report.Metrics.Expenses)}}
                {{#if (gt report.Metrics.Growth 5)}}
                    - Performance: STRONG. Revenue exceeds expenses significantly, and growth is high.
                {{else}}
                    - Performance: STABLE. Revenue exceeds expenses, but growth is modest.
                {{/if}}
            {{else}}
                - Performance: CRITICAL. Expenses exceed revenue. Immediate review required.
            {{/if}}
            """;

        // 3. Data Setup
        var report = new QuarterlyReport
        {
            Quarter = "Q3 2023",
            Metrics = new QuarterlyMetrics
            {
                Revenue = 1_500_000,
                Expenses = 1_200_000,
                Growth = 6.5
            }
        };

        // 4. Implementation Details
        // Registering 'gt' (greater than) and 'lt' (less than) helpers.
        var factory = new HandlebarsPromptTemplateFactory((config) =>
        {
            // Greater Than Helper
            config.BlockHelpers.Add("gt", (writer, options, context, arguments) =>
            {
                if (arguments.Length >= 2 && arguments[0] is IComparable val1 && arguments[1] is IComparable val2)
                {
                    if (val1.CompareTo(val2) > 0)
                    {
                        options.Template(writer, context);
                    }
                }
            });

            // Less Than Helper
            config.BlockHelpers.Add("lt", (writer, options, context, arguments) =>
            {
                if (arguments.Length >= 2 && arguments[0] is IComparable val1 && arguments[1] is IComparable val2)
                {
                    if (val1.CompareTo(val2) < 0)
                    {
                        options.Template(writer, context);
                    }
                }
            });
        });

        var config = new PromptTemplateConfig
        {
            Template = template,
            TemplateFormat = "handlebars"
        };

        var templateEngine = await factory.CreateAsync(config);
        
        var arguments = new KernelArguments
        {
            ["report"] = report
        };

        var result = await templateEngine.RenderAsync(null, arguments);
        Console.WriteLine(result);
    }
}
