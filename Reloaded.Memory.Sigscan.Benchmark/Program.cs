using System;
using BenchmarkDotNet.Running;
using Reloaded.Memory.Sigscan.Benchmark.Benchmarks.LargeArray;
using Reloaded.Memory.Sigscan.Benchmark.Benchmarks.Parsing;
using Reloaded.Memory.Sigscan.Structs;

namespace Reloaded.Memory.Sigscan.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<ShortPatternEnd>();
            BenchmarkRunner.Run<MediumPatternEnd>();
            BenchmarkRunner.Run<MediumPatternWithMaskEnd>();
            BenchmarkRunner.Run<MediumPatternAlignedEnd>();
            BenchmarkRunner.Run<LongPatternEnd>();
            BenchmarkRunner.Run<LongPatternWithMaskEnd>();
            BenchmarkRunner.Run<ParsePattern>();
            BenchmarkRunner.Run<StringParsing>();
            BenchmarkRunner.Run<WorstCaseScenario>();
        }
    }
}
