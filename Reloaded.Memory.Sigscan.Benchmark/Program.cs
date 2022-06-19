using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Reports;
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
            //BenchmarkRunner.Run<LongPatternWithMaskEndMt>(new MTSigscanConfig(LongPatternWithMaskEndMt.GetFileSize));
            //BenchmarkRunner.Run<RandomMt>(new MTSigscanConfig(RandomMt.GetFileSize));
            // BenchmarkRunner.Run<RandomMtBigSize>(new MTSigscanConfig(RandomMtBigSize.GetFileSize));
            // BenchmarkRunner.Run<CachedVsUncachedRandomMt>(new MTSigscanConfig(CachedVsUncachedRandomMt.GetFileSize));
            
            BenchmarkRunner.Run<LongPatternWithMaskEnd>(ScannerBenchmarkBase.GetConfig());
            // BenchmarkRunner.Run<MediumPatternWithMaskEnd>(ScannerBenchmarkBase.GetConfig());
            // BenchmarkRunner.Run<ShortPatternEnd>(ScannerBenchmarkBase.GetConfig());
            // BenchmarkRunner.Run<OtherScannerBenchmarks>(ScannerBenchmarkBase.GetConfig());
            // BenchmarkRunner.Run<WorstCaseScenario>(WorstCaseScenario.GetConfig());

            // BenchmarkRunner.Run<ParsePattern>();
            // BenchmarkRunner.Run<StringParsing>();
        }

    }
}
