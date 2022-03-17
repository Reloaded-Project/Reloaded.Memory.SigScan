using BenchmarkDotNet.Attributes;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.LargeArray
{
    public class OtherLargeArrayBenchmarks : LargeArrayBenchmarkBase
    {
        [Benchmark]
        public int MediumPatternEnd_Avx()
        {
            var result = _scannerFromFile.FindPattern_Avx2("D7 9F 43 63 68 43");
            return result.Offset;
        }

        [Benchmark]
        public int MediumPatternEnd_Sse()
        {
            var result = _scannerFromFile.FindPattern_Sse2("D7 9F 43 63 68 43");
            return result.Offset;
        }

        [Benchmark]
        public int MediumPatternEnd_Compiled()
        {
            var result = _scannerFromFile.FindPattern_Compiled("D7 9F 43 63 68 43");
            return result.Offset;
        }

        [Benchmark]
        public int MediumPatternEnd_Simple()
        {
            var result = _scannerFromFile.FindPattern_Simple("D7 9F 43 63 68 43");
            return result.Offset;
        }

        [Benchmark]
        public int MediumPatternAlignedEnd_Avx()
        {
            var result = _scannerFromFile.FindPattern_Avx2("D7 9F 43 63");
            return result.Offset;
        }

        [Benchmark]
        public int MediumPatternAlignedEnd_Sse()
        {
            var result = _scannerFromFile.FindPattern_Sse2("D7 9F 43 63");
            return result.Offset;
        }

        [Benchmark]
        public int MediumPatternAlignedEnd_Compiled()
        {
            var result = _scannerFromFile.FindPattern_Compiled("D7 9F 43 63");
            return result.Offset;
        }

        [Benchmark]
        public int MediumPatternAlignedEnd_Simple()
        {
            var result = _scannerFromFile.FindPattern_Simple("D7 9F 43 63");
            return result.Offset;
        }

        [Benchmark]
        public int LongPatternEnd_Avx()
        {
            var result = _scannerFromFile.FindPattern_Avx2("9F 43 63 68 43 4F 99 A7 15 48");
            return result.Offset;
        }

        [Benchmark]
        public int LongPatternEnd_Sse()
        {
            var result = _scannerFromFile.FindPattern_Sse2("9F 43 63 68 43 4F 99 A7 15 48");
            return result.Offset;
        }

        [Benchmark]
        public int LongPatternEnd_Compiled()
        {
            var result = _scannerFromFile.FindPattern_Compiled("9F 43 63 68 43 4F 99 A7 15 48");
            return result.Offset;
        }

        [Benchmark]
        public int LongPatternEnd_Simple()
        {
            var result = _scannerFromFile.FindPattern_Simple("9F 43 63 68 43 4F 99 A7 15 48");
            return result.Offset;
        }
    }
}
