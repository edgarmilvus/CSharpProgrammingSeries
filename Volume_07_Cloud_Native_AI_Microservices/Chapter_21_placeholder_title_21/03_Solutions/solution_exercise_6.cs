
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

// Source File: solution_exercise_6.cs
// Description: Solution for Exercise 6
// ==========================================

// Gateway/Program.cs (Minimal API)
using Shared.Contracts;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// RabbitMQ Connection Factory
var factory = new ConnectionFactory { HostName = "localhost" };

var app = builder.Build();

app.MapPost("/submit", async (IFormFile file) =>
{
    if (file == null || file.Length == 0) return Results.BadRequest();

    using var ms = new MemoryStream();
    await file.CopyToAsync(ms);
    var message = new InferenceRequestMessage(
        Guid.NewGuid(),
        ms.ToArray(),
        DateTimeOffset.UtcNow
    );

    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

    using var connection = factory.CreateConnection();
    using var channel = connection.CreateModel();
    
    channel.ExchangeDeclare(exchange: "inference.direct", type: ExchangeType.Direct);
    
    channel.BasicPublish(
        exchange: "inference.direct",
        routingKey: "inference.request",
        basicProperties: null,
        body: body);

    return Results.Accepted($"/results/{message.MessageId}");
});

app.Run();
