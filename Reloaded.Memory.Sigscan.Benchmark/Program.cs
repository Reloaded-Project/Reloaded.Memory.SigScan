using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Running;
using Reloaded.Memory.Sigscan.Benchmark.Benchmarks;
using Reloaded.Memory.Sigscan.Benchmark.Benchmarks.LargeArray;
using Reloaded.Memory.Sigscan.Benchmark.Benchmarks.Multithread;
using Reloaded.Memory.Sigscan.Benchmark.Benchmarks.Parsing;
using Reloaded.Memory.Sigscan.Structs;

namespace Reloaded.Memory.Sigscan.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            // Multithread
            // BenchmarkRunner.Run<LongPatternWithMaskEndMt>(LargeArrayBenchmarkBase.Config(BenchmarkKind.Multithreaded));
            BenchmarkRunner.Run<RandomMt>(LargeArrayBenchmarkBase.Config(BenchmarkKind.MultithreadedRandom));

            //BenchmarkRunner.Run<LongPatternWithMaskEnd>(LargeArrayBenchmarkBase.Config());
            //BenchmarkRunner.Run<MediumPatternWithMaskEnd>(LargeArrayBenchmarkBase.Config());
            //BenchmarkRunner.Run<ShortPatternEnd>(LargeArrayBenchmarkBase.Config());
            //BenchmarkRunner.Run<OtherLargeArrayBenchmarks>(LargeArrayBenchmarkBase.Config());
            //BenchmarkRunner.Run<WorstCaseScenario>(WorstCaseScenario.Config());

            //BenchmarkRunner.Run<ParsePattern>();
            //BenchmarkRunner.Run<StringParsing>();
        }
    }
}
