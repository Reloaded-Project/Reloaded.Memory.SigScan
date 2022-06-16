using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Reloaded.Memory.Sigscan.Benchmark.Benchmarks.LargeArray;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.Multithread
{
    public class LongPatternWithMaskEndMt : ScannerBenchmarkBase
    {
        [Params(1, 8, 128, 512)]
        public int NumItems;

        private List<string> _patterns;

        public override void PerMethodSetup()
        {
            _patterns = new List<string>(NumItems);
            for (int x = 0; x < NumItems; x++)
                _patterns.Add("9F 43 ?? ?? 43 4F 99 ?? ?? 48");
        }

        [Benchmark]
        public int LongPatternWithMaskEnd_ST()
        {
            return _scanner.FindPattern(_patterns[0]).Offset;
        }

        [Benchmark]
        public int LongPatternWithMaskEnd_MT_NoLB()
        {
            // Baseline
            var result = _scanner.FindPatterns(_patterns);
            return result[0].Offset;
        }

        [Benchmark]
        public int LongPatternWithMaskEnd_MT_LB()
        {
            var result = _scanner.FindPatterns(_patterns, true);
            return result[0].Offset;
        }

        /// <summary>
        /// Gets the total processed file size by a benchmark run.
        /// </summary>
        public static long GetFileSize(Summary summary, BenchmarkCase benchmarkCase)
        {
            var numItems = benchmarkCase.Parameters[nameof(NumItems)];
            return _data.Length * (int)numItems;
        }
    }
}
