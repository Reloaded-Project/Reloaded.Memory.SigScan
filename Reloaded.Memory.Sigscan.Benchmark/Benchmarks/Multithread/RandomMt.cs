using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.Multithread
{
    public class RandomMt : LargeArrayMtBenchmarkBase
    {
        public static Dictionary<int, double> NumItemsToTotalSizeMB = new();

        private static List<string> _patterns;

        public override void PerMethodSetup()
        {
            // Skip another method in class with same params.
            if (_patterns != null && _patterns.Count == NumItems)
                return;
            
            // Pick some random patterns.
            Console.WriteLine($"Creating Random Test Data for Item Count: {NumItems}");
            _patterns = new List<string>(NumItems);
            var random = new Random(DateTime.Now.Millisecond * DateTime.Now.Second);
            long totalBytes = 0;
            const int patternSize = 12;

            // Pick out some random offsets
            for (int x = 0; x < NumItems; x++)
            {
                var offset = random.Next(patternSize, _dataFromFile.Length - patternSize);
                totalBytes += offset;
                Console.WriteLine($"Offset: {offset}");
                _patterns.Add(GeneratePattern(offset));
            }

            NumItemsToTotalSizeMB[NumItems] = BenchmarkUtils.BytesToMB(totalBytes);

            string GeneratePattern(int offset)
            {
                var span = _dataFromFile.AsSpan(offset, patternSize);
                return BitConverter.ToString(span.ToArray()).Replace('-', ' ');
            }
        }

        [Benchmark]
        public int Random_ST()
        {
            return _scannerFromFile.FindPattern(_patterns[0]).Offset;
        }

        [Benchmark]
        public int Random_MT_NoLB()
        {
            // Baseline
            var result = _scannerFromFile.FindPatterns(_patterns);
            return result[0].Offset;
        }

        [Benchmark]
        public int Random_MT_LB()
        {
            // Baseline
            var result = _scannerFromFile.FindPatterns(_patterns, true);
            return result[0].Offset;
        }
    }
}
