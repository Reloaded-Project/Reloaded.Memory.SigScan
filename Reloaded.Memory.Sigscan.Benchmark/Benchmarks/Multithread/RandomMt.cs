using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Reloaded.Memory.Sigscan.Benchmark.Benchmarks.LargeArray;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.Multithread
{
    public class RandomMt : ScannerBenchmarkBase
    {
        [Params(1, 8, 128, 512)]
        public int NumItems;

        private static Dictionary<int, long> _numItemsToTotalSize = new();
        private static List<string> _patterns;
        
        public override void PerMethodSetup()
        {
            // Skip another method in class with same params.
            if (_patterns != null && _patterns.Count == NumItems)
                return;
            
            // Pick some random patterns.
            Console.WriteLine($"[{nameof(RandomMt)}] Creating Random Test Data for Item Count: {NumItems}");
            _patterns = BenchmarkUtils.CreateRandomPatterns(_data, NumItems, 12, out var totalBytes);
            _numItemsToTotalSize[NumItems] = totalBytes;
        }

        [Benchmark]
        public int Random_ST()
        {
            return _scanner.FindPattern(_patterns[0]).Offset;
        }

        [Benchmark]
        public int Random_MT_NoLB()
        {
            // Baseline
            var result = _scanner.FindPatterns(_patterns);
            return result[0].Offset;
        }

        [Benchmark]
        public int Random_MT_LB()
        {
            // Baseline
            var result = _scanner.FindPatterns(_patterns, true);
            return result[0].Offset;
        }

        /// <summary>
        /// Gets the total processed file size by a benchmark run.
        /// </summary>
        public static long GetFileSize(Summary summary, BenchmarkCase benchmarkCase)
        {
            var numItems = benchmarkCase.Parameters[nameof(NumItems)];
            return _numItemsToTotalSize[(int)numItems];
        }
    }
}
