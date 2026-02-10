
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

using System.Diagnostics;

public enum Precision { FP32, FP16, INT8 }

public record InferenceResult(
    double ExecutionTimeMs, 
    long MemoryUsageBytes, 
    double AccuracyScore, 
    Precision Precision
);

public class InferenceEngine
{
    // Simulated constants for a specific hardware (e.g., A100 GPU)
    private const double BaseTimeMs = 50.0;
    private const long BaseMemoryBytes = 1024 * 1024 * 100; // 100MB

    public InferenceResult RunInference(float[] input, Precision precision)
    {
        // 1. Simulate Computation based on Precision
        double speedFactor = 1.0;
        double memoryFactor = 1.0;
        double accuracyPenalty = 0.0;

        switch (precision)
        {
            case Precision.FP32:
                speedFactor = 1.0; // Baseline
                memoryFactor = 1.0;
                accuracyPenalty = 0.0; // 100%
                break;
            case Precision.FP16:
                speedFactor = 0.5; // 2x faster
                memoryFactor = 0.5; // Half memory
                accuracyPenalty = 0.02; // ~2% drop
                break;
            case Precision.INT8:
                speedFactor = 0.25; // 4x faster
                memoryFactor = 0.25; // Quarter memory
                accuracyPenalty = 0.05; // ~5% drop
                break;
        }

        // Simulate the delay (GPU compute)
        var sw = Stopwatch.StartNew();
        Thread.Sleep(TimeSpan.FromMilliseconds(BaseTimeMs * speedFactor)); 
        sw.Stop();

        // Simulate Energy Consumption (Proportional to time * power)
        // Not returned, but calculated for internal logic
        double energy = sw.ElapsedMilliseconds * (1.0 / speedFactor); 

        return new InferenceResult(
            sw.ElapsedMilliseconds,
            (long)(BaseMemoryBytes * memoryFactor),
            precision
        );
    }
}

public class PrecisionController
{
    private readonly InferenceEngine _engine;
    private Precision _currentPrecision = Precision.FP32;
    private int _highLoadCounter = 0;

    public PrecisionController(InferenceEngine engine)
    {
        _engine = engine;
    }

    // Determines optimal precision based on simulated system load (0.0 to 1.0)
    public Precision GetOptimalPrecision(double systemLoad)
    {
        // Hysteresis logic to prevent rapid switching
        if (systemLoad > 0.8)
        {
            _highLoadCounter++;
        }
        else
        {
            _highLoadCounter = 0;
        }

        // If under high load for a sustained period, switch down
        if (_highLoadCounter > 5 && _currentPrecision == Precision.FP32)
        {
            Console.WriteLine($"High Load detected ({systemLoad:P}). Switching to FP16 for throughput.");
            _currentPrecision = Precision.FP16;
        }
        else if (systemLoad > 0.95 && _currentPrecision == Precision.FP16)
        {
            Console.WriteLine($"Critical Load detected. Switching to INT8.");
            _currentPrecision = Precision.INT8;
        }
        else if (systemLoad < 0.3 && _currentPrecision != Precision.FP32)
        {
            Console.WriteLine($"Load normalized. Reverting to FP32 for max accuracy.");
            _currentPrecision = Precision.FP32;
            _highLoadCounter = 0;
        }

        return _currentPrecision;
    }

    public void RunBenchmark()
    {
        Console.WriteLine("--- Starting Precision Benchmark ---");
        var input = new float[1024]; // Dummy input
        var sw = Stopwatch.StartNew();

        // Benchmark FP32
        Console.WriteLine("Running 1000 inferences at FP32...");
        for (int i = 0; i < 1000; i++)
        {
            _engine.RunInference(input, Precision.FP32);
        }
        var fp32Time = sw.ElapsedMilliseconds;
        sw.Restart();

        // Benchmark FP16
        Console.WriteLine("Running 1000 inferences at FP16...");
        for (int i = 0; i < 1000; i++)
        {
            _engine.RunInference(input, Precision.FP16);
        }
        var fp16Time = sw.ElapsedMilliseconds;

        Console.WriteLine($"\nResults:");
        Console.WriteLine($"FP32 Total Time: {fp32Time}ms");
        Console.WriteLine($"FP16 Total Time: {fp16Time}ms");
        Console.WriteLine($"Speedup: {(double)fp32Time / fp16Time:F2}x");
    }
}
