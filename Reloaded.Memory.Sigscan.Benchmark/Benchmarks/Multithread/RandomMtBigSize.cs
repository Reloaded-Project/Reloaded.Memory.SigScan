using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Reloaded.Memory.Sigscan.Benchmark.Benchmarks.LargeArray;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.Multithread
{
    public class RandomMtBigSize : ScannerBenchmarkBase
    {
        // 200MB, fuck Denuvo
        const long ArraySize = 200_000_000;

        [Params(1, 4, 16, 64)]
        public int NumItems;
        
        private static List<string> _patterns;
        private static Dictionary<int, long> _numItemsToTotalSize = new();

        public override void PerMethodSetup()
        {
            // Skip another method in class with same params.
            if (_patterns != null && _patterns.Count == NumItems)
                return;

            Console.WriteLine($"[{nameof(RandomMtBigSize)}] Creating Random Test Data for Item Count: {NumItems}");
            _data = BenchmarkUtils.CreateRandomArray((int)ArraySize);
            _scanner = new Scanner(_data);

            // Pick some random patterns.
            _patterns = BenchmarkUtils.CreateRandomPatterns(_data, NumItems, 12, out var totalBytes);
            _numItemsToTotalSize[NumItems] = totalBytes;
            GC.Collect();
        }

        [Benchmark]
        public int Random_ST()
        {
            var result = 0; 
            foreach (var pattern in CollectionsMarshal.AsSpan(_patterns))
                result = _scanner.FindPattern(pattern).Offset;

            return result;
        }

        [Benchmark]
        public int Random_ST_Compiled()
        {            
            var result = 0; 
            foreach (var pattern in CollectionsMarshal.AsSpan(_patterns))
                result = _scanner.FindPattern_Compiled(pattern).Offset;

            return result;
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
            return ArraySize * (int)numItems;
        }
    }
}
