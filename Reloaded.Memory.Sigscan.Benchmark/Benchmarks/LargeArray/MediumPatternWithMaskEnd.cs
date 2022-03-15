using BenchmarkDotNet.Attributes;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.LargeArray
{
    public class MediumPatternWithMaskEnd : LargeArrayBenchmarkBase
    {
        [Benchmark]
        public int MediumPatternWithMaskEnd_Avx()
        {
            var result = _scannerFromFile.CompiledFindPatternAvx2("A0 4E ?? ?? 0E ED");
            return result.Offset;
        }

        [Benchmark]
        public int MediumPatternWithMaskEnd_Compiled()
        {
            var result = _scannerFromFile.CompiledFindPattern("A0 4E ?? ?? 0E ED");
            return result.Offset;
        }

        [Benchmark]
        public int MediumPatternWithMaskEnd_Simple()
        {
            var result = _scannerFromFile.SimpleFindPattern("A0 4E ?? ?? 0E ED");
            return result.Offset;
        }
    }
}
