using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.LargeArray
{
    public class WorstCaseScenario
    {
        public static IConfig GetConfig() => new SigscanConfig((bc, bCase) => _dataFromFile.Length);

        private static byte[] _dataFromFile     = File.ReadAllBytes(Constants.WorstCaseFile);
        private static Scanner _scannerFromFile = new Scanner(_dataFromFile);

        [Benchmark]
        public int Avx()
        {
            var result = _scannerFromFile.FindPattern_Avx2("0A 0A 0A 0A 0A 0A 0A 0A 0B");
            return result.Offset;
        }

        [Benchmark]
        public int Sse()
        {
            var result = _scannerFromFile.FindPattern_Sse2("0A 0A 0A 0A 0A 0A 0A 0A 0B");
            return result.Offset;
        }

        [Benchmark]
        public int Compiled()
        {
            var result = _scannerFromFile.FindPattern_Compiled("0A 0A 0A 0A 0A 0A 0A 0A 0B");
            return result.Offset;
        }

        [Benchmark]
        public int Simple()
        {
            var result = _scannerFromFile.FindPattern_Simple("0A 0A 0A 0A 0A 0A 0A 0A 0B");
            return result.Offset;
        }
    }
}
