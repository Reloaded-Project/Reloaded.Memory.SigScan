using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Reloaded.Memory.Sigscan.Benchmark.Columns;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.LargeArray
{
    [Config(typeof(Config))]
    public class MediumPatternAlignedEnd
    {
        private static byte[] _dataFromFile     = File.ReadAllBytes(Constants.File);
        private static Scanner _scannerFromFile = new Scanner(_dataFromFile);

        private class Config : ManualConfig
        {
            public Config()
            {
                Add(Job.Core);
                Add(new Speed(3.145718F));
            }
        }

        [Benchmark]
        public int Compiled()
        {
            var result = _scannerFromFile.CompiledFindPattern("D7 9F 43 63");
            return result.Offset;
        }

        [Benchmark]
        public int Simple()
        {
            var result = _scannerFromFile.SimpleFindPattern("D7 9F 43 63");
            return result.Offset;
        }
    }
}
