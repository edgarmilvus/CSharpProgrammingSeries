
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

public class ModelConfig
{
    public string ModelName { get; set; }
    public ModelConfig(string name) { ModelName = name; }
}

public class DataSource
{
    public string ConnectionString { get; set; }
    public DataSource(string conn) { ConnectionString = conn; }
}

public class PredictionContext
{
    public Option<ModelConfig> Config { get; set; }
    public Option<DataSource> Source { get; set; }

    public PredictionContext(Option<ModelConfig> config, Option<DataSource> source)
    {
        Config = config;
        Source = source;
    }

    public Option<string> RunPrediction()
    {
        // Local flags to track validity state
        bool hasConfig = false;
        string configName = "";
        
        // Check Config
        Config.Match(
            ifSome: c => { hasConfig = true; configName = c.ModelName; },
            ifNone: () => { hasConfig = false; }
        );

        // Fail fast if Config is missing
        if (!hasConfig) return Option<string>.None();

        bool hasSource = false;
        string sourceConn = "";

        // Check Source
        Source.Match(
            ifSome: s => { hasSource = true; sourceConn = s.ConnectionString; },
            ifNone: () => { hasSource = false; }
        );

        // Fail fast if Source is missing
        if (!hasSource) return Option<string>.None();

        // Simulate processing only if both are valid
        string result = $"Prediction running with Model: {configName} and Data: {sourceConn}";
        return Option<string>.Some(result);
    }
}
