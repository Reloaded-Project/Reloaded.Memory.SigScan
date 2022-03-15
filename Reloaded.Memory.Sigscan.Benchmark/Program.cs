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
            BenchmarkRunner.Run<LargeArrayBenchmarks>(LargeArrayBenchmarks.Config);
            BenchmarkRunner.Run<WorstCaseScenario>(WorstCaseScenario.Config);

            BenchmarkRunner.Run<ParsePattern>();
            BenchmarkRunner.Run<StringParsing>();
        }
    }
}
