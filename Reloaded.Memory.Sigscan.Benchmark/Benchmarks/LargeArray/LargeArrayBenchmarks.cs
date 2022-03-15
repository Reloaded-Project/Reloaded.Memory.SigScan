using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.LargeArray
{
    public class LargeArrayBenchmarks
    {
        public static IConfig Config = new SigscanConfig(3.145719F);

        private static byte[] _dataFromFile     = File.ReadAllBytes(Constants.File);
        private static Scanner _scannerFromFile = new Scanner(_dataFromFile);

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

        [Benchmark]
        public int MediumPatternEnd_Compiled()
        {
            var result = _scannerFromFile.CompiledFindPattern("D7 9F 43 63 68 43");
            return result.Offset;
        }

        [Benchmark]
        public int MediumPatternEnd_Simple()
        {
            var result = _scannerFromFile.SimpleFindPattern("D7 9F 43 63 68 43");
            return result.Offset;
        }

        [Benchmark]
        public int MediumPatternAlignedEnd_Compiled()
        {
            var result = _scannerFromFile.CompiledFindPattern("D7 9F 43 63");
            return result.Offset;
        }

        [Benchmark]
        public int MediumPatternAlignedEnd_Simple()
        {
            var result = _scannerFromFile.SimpleFindPattern("D7 9F 43 63");
            return result.Offset;
        }

        [Benchmark]
        public int LongPatternEnd_Compiled()
        {
            var result = _scannerFromFile.CompiledFindPattern("9F 43 63 68 43 4F 99 A7 15 48");
            return result.Offset;
        }

        [Benchmark]
        public int LongPatternEnd_Simple()
        {
            var result = _scannerFromFile.SimpleFindPattern("9F 43 63 68 43 4F 99 A7 15 48");
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
