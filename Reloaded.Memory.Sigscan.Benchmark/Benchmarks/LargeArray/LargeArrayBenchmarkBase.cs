using System.IO;
using BenchmarkDotNet.Configs;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.LargeArray
{
    public class LargeArrayBenchmarkBase
    {
        public static IConfig Config = new SigscanConfig(3.145719F);

        protected static byte[] _dataFromFile = File.ReadAllBytes(Constants.File);
        protected static Scanner _scannerFromFile = new Scanner(_dataFromFile);
    }
}
