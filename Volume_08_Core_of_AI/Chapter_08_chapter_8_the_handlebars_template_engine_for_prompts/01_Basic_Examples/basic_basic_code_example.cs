
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

// Source File: basic_basic_code_example.cs
// Description: Basic Code Example
// ==========================================

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using System.Console;

// 1. Define a simple plugin to simulate fetching real-time data.
public class OrderStatusPlugin
{
    [KernelFunction("get_order_details")]
    [Description("Retrieves the current status and details of a specific order.")]
    public string GetOrderDetails(
        [Description("The unique identifier for the order")] string orderId)
    {
        // In a real app, this would query a database or API.
        // Here we return a JSON string to simulate the data structure.
        var mockData = new
        {
            OrderId = orderId,
            Status = "Shipped",
            Carrier = "FedEx",
            TrackingNumber = "1234567890XYZ",
            EstimatedDelivery = "2023-10-25"
        };

        return JsonSerializer.Serialize(mockData);
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        // 2. Initialize the Kernel. 
        // Note: For this example to run without an API key, we use a null client 
        // and focus purely on the template rendering logic. 
        // In a real scenario, you would configure AzureOpenAI or OpenAI here.
        var builder = Kernel.CreateBuilder();
        
        // 3. Register the OrderStatusPlugin so the template can access the data.
        builder.Plugins.AddFromType<OrderStatusPlugin>();
        
        var kernel = builder.Build();

        // 4. Define the Handlebars template.
        // {{...}} denotes variables.
        // {{#with ...}} creates a context block for the object properties.
        // {{#each ...}} iterates over a collection (not used in this basic example).
        string handlebarsTemplate = """
            You are a helpful customer support assistant. 
            Here is the status for order {{orderId}}:

            Order ID: {{orderDetails.OrderId}}
            Status: {{orderDetails.Status}}
            Carrier: {{orderDetails.Carrier}}
            Tracking #: {{orderDetails.TrackingNumber}}
            Estimated Delivery: {{orderDetails.EstimatedDelivery}}

            Please assist the customer based on this information.
            """;

        // 5. Create the prompt template config.
        // We explicitly tell the Kernel to use Handlebars as the template engine.
        var promptConfig = new PromptTemplateConfig(handlebarsTemplate)
        {
            TemplateFormat = "handlebars"
        };

        // 6. Create the function from the template.
        var promptFunction = kernel.CreateFunctionFromPrompt(promptConfig);

        // 7. Prepare the input data for the template.
        // We fetch the raw JSON string from our plugin first.
        var plugin = new OrderStatusPlugin();
        string rawJson = plugin.GetOrderDetails("ORD-7782");
        
        // Parse it to an object so Handlebars can access properties via dot notation.
        var orderData = JsonSerializer.Deserialize<JsonElement>(rawJson);

        // 8. Invoke the function with the variables required by the template.
        // The 'orderId' and 'orderDetails' keys match the {{variables}} in the template.
        var result = await kernel.InvokeAsync(promptFunction, new KernelArguments
        {
            ["orderId"] = "ORD-7782",
            ["orderDetails"] = orderData
        });

        // 9. Output the rendered prompt.
        // This string is what would actually be sent to the LLM.
        WriteLine("--- Rendered Prompt ---");
        WriteLine(result.ToString());
    }
}
