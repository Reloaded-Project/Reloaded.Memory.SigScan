using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks
{
    public class ScannerBenchmarkBase
    {
        public static IConfig GetConfig() => new SigscanConfig((summary, bc) => _data.Length);

        internal static byte[] _data = File.ReadAllBytes(Constants.File);
        internal static Scanner _scanner = new Scanner(_data);

        [GlobalSetup]
        public void GlobalSetup() => PerMethodSetup();

        /// <summary>
        /// Sets up this benchmark.
        /// </summary>
        public virtual void PerMethodSetup() { }
    }
}
