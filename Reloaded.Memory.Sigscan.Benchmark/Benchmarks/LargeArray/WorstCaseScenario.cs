using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.LargeArray
{
    public class WorstCaseScenario
    {
        public static IConfig Config = new SigscanConfig(5.999999f);

        private static byte[] _dataFromFile     = File.ReadAllBytes(Constants.WorstCaseFile);
        private static Scanner _scannerFromFile = new Scanner(_dataFromFile);

        [Benchmark]
        public int Avx()
        {
            var result = _scannerFromFile.CompiledFindPatternAvx2("0A 0A 0A 0A 0A 0A 0A 0A 0B");
            return result.Offset;
        }

        [Benchmark]
        public int Compiled()
        {
            var result = _scannerFromFile.CompiledFindPattern("0A 0A 0A 0A 0A 0A 0A 0A 0B");
            return result.Offset;
        }

        [Benchmark]
        public int Simple()
        {
            var result = _scannerFromFile.SimpleFindPattern("0A 0A 0A 0A 0A 0A 0A 0A 0B");
            return result.Offset;
        }
    }
}
