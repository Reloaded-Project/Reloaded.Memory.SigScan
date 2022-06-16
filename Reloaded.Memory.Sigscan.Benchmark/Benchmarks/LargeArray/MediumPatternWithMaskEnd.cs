using BenchmarkDotNet.Attributes;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.LargeArray
{
    public class MediumPatternWithMaskEnd : ScannerBenchmarkBase
    {
        [Benchmark]
        public int MediumPatternWithMaskEnd_Avx()
        {
            var result = _scanner.FindPattern_Avx2("A0 4E ?? ?? 0E ED");
            return result.Offset;
        }

        [Benchmark]
        public int MediumPatternWithMaskEnd_Sse()
        {
            var result = _scanner.FindPattern_Sse2("A0 4E ?? ?? 0E ED");
            return result.Offset;
        }

        [Benchmark]
        public int MediumPatternWithMaskEnd_Compiled()
        {
            var result = _scanner.FindPattern_Compiled("A0 4E ?? ?? 0E ED");
            return result.Offset;
        }

        [Benchmark]
        public int MediumPatternWithMaskEnd_Simple()
        {
            var result = _scanner.FindPattern_Simple("A0 4E ?? ?? 0E ED");
            return result.Offset;
        }
    }
}
