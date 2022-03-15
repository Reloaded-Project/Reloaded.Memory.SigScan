using BenchmarkDotNet.Attributes;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.LargeArray
{
    public class LongPatternWithMaskEnd : LargeArrayBenchmarkBase
    {
        [Benchmark]
        public int LongPatternWithMaskEnd_Avx()
        {
            var result = _scannerFromFile.CompiledFindPatternAvx2("9F 43 ?? ?? 43 4F 99 ?? ?? 48");
            return result.Offset;
        }

        [Benchmark(Baseline = true)]
        public int LongPatternWithMaskEnd_Compiled()
        {
            var result = _scannerFromFile.CompiledFindPattern("9F 43 ?? ?? 43 4F 99 ?? ?? 48");
            return result.Offset;
        }

        [Benchmark]
        public int LongPatternWithMaskEnd_Simple()
        {
            var result = _scannerFromFile.SimpleFindPattern("9F 43 ?? ?? 43 4F 99 ?? ?? 48");
            return result.Offset;
        }
    }
}
