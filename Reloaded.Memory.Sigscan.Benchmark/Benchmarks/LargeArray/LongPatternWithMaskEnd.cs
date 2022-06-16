using BenchmarkDotNet.Attributes;
using Reloaded.Memory.Sigscan.Benchmark.Benchmarks;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.LargeArray
{
    public class LongPatternWithMaskEnd : ScannerBenchmarkBase
    {
        [Benchmark]
        public int LongPatternWithMaskEnd_Avx()
        {
            var result = _scanner.FindPattern_Avx2("9F 43 ?? ?? 43 4F 99 ?? ?? 48");
            return result.Offset;
        }

        [Benchmark]
        public int LongPatternWithMaskEnd_Sse()
        {
            var result = _scanner.FindPattern_Sse2("9F 43 ?? ?? 43 4F 99 ?? ?? 48");
            return result.Offset;
        }

        [Benchmark(Baseline = true)]
        public int LongPatternWithMaskEnd_Compiled()
        {
            var result = _scanner.FindPattern_Compiled("9F 43 ?? ?? 43 4F 99 ?? ?? 48");
            return result.Offset;
        }

        [Benchmark]
        public int LongPatternWithMaskEnd_Simple()
        {
            var result = _scanner.FindPattern_Simple("9F 43 ?? ?? 43 4F 99 ?? ?? 48");
            return result.Offset;
        }
    }
}
