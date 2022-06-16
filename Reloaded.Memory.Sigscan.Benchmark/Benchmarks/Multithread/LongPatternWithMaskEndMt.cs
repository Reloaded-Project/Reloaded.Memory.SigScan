using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.Multithread
{
    public class LongPatternWithMaskEndMt : LargeArrayMtBenchmarkBase
    {
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
            return _scannerFromFile.FindPattern(_patterns[0]).Offset;
        }

        [Benchmark]
        public int LongPatternWithMaskEnd_MT_NoLB()
        {
            // Baseline
            var result = _scannerFromFile.FindPatterns(_patterns);
            return result[0].Offset;
        }

        [Benchmark]
        public int LongPatternWithMaskEnd_MT_LB()
        {
            var result = _scannerFromFile.FindPatterns(_patterns, true);
            return result[0].Offset;
        }
    }
}
