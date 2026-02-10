
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

// Source File: solution_exercise_5.cs
// Description: Solution for Exercise 5
// ==========================================

using Prometheus;
using Microsoft.AspNetCore.Mvc;

namespace InferenceService
{
    [ApiController]
    [Route("[controller]")]
    public class InferenceController : ControllerBase
    {
        private static readonly Gauge InferenceQueueLength = Metrics.CreateGauge("inference_queue_length", "Current number of requests waiting for inference.");
        private static readonly Gauge GpuMemoryUsed = Metrics.CreateGauge("gpu_memory_used_bytes", "GPU memory used in bytes.");

        [HttpPost("predict")]
        public async Task<IActionResult> Predict([FromBody] InferenceRequest request)
        {
            // Increment queue length when request arrives
            InferenceQueueLength.Inc();

            try
            {
                // Simulate heavy GPU work
                // In a real app, you would update GpuMemoryUsed based on actual GPU memory API calls (e.g., NVIDIA Management Library)
                // For this example, we simulate a spike
                GpuMemoryUsed.Set(4.5 * 1024 * 1024 * 1024); // 4.5 GB

                await Task.Delay(2000); // Simulate processing time

                return Ok(new { Result = "Prediction" });
            }
            finally
            {
                // Decrement queue length when processing finishes
                InferenceQueueLength.Dec();
                
                // Reset or update GPU metric if necessary
                // GpuMemoryUsed.Set(0); 
            }
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();

            // Enable Prometheus metrics endpoint
            builder.Services.AddMetricServer(options => 
            {
                options.Port = 9090; 
            });

            var app = builder.Build();
            app.MapControllers();
            
            // Expose metrics at /metrics
            app.MapMetrics();
            
            app.Run();
        }
    }
}
