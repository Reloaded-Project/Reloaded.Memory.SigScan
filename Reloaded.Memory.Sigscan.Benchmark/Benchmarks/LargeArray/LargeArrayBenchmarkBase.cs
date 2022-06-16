using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.LargeArray
{
    public class LargeArrayBenchmarkBase
    {
        public static IConfig Config(BenchmarkKind kind = BenchmarkKind.Default) => new SigscanConfig(kind, _dataFromFile.Length);

        protected static byte[] _dataFromFile = File.ReadAllBytes(Constants.File);
        protected static Scanner _scannerFromFile = new Scanner(_dataFromFile);

        [GlobalSetup]
        public void GlobalSetup() => PerMethodSetup();

        /// <summary>
        /// Sets up this benchmark.
        /// </summary>
        public virtual void PerMethodSetup() { }
    }
}
