using BenchmarkDotNet.Attributes;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.LargeArray
{
    public class ShortPatternEnd : LargeArrayBenchmarkBase
    {
        [Benchmark]
        public int ShortPatternEnd_Avx()
        {
            var result = _scannerFromFile.CompiledFindPatternAvx2("0F D7 9F");
            return result.Offset;
        }

        [Benchmark]
        public int ShortPatternEnd_Sse()
        {
            var result = _scannerFromFile.CompiledFindPatternSse2("0F D7 9F");
            return result.Offset;
        }

        [Benchmark]
        public int ShortPatternEnd_Compiled()
        {
            var result = _scannerFromFile.CompiledFindPattern("0F D7 9F");
            return result.Offset;
        }

        [Benchmark]
        public int ShortPatternEnd_Simple()
        {
            var result = _scannerFromFile.SimpleFindPattern("0F D7 9F");
            return result.Offset;
        }
    }
}
