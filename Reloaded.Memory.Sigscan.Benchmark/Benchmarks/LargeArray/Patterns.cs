using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using Reloaded.Memory.Sigscan.Benchmark.Columns;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.LargeArray
{
    [Config(typeof(Config))]
    public class Patterns
    {
        private static byte[] _dataFromFile = File.ReadAllBytes(Constants.DataFile);
        private static Scanner _scannerFromFile = new Scanner(_dataFromFile);

        private class Config : ManualConfig
        {
            public Config()
            {
                Add(Job.Core);
                Add(MarkdownExporter.GitHub);
                Add(new Speed(_dataFromFile.Length / 1000.0f / 1000.0f));
            }
        }

        [Benchmark]
        public int Compiled()
        {
            var result = _scannerFromFile.CompiledFindAllPatterns("6A 44 2F 13 1D 31 D2 A8 08 14 E8 0A 79");
            return result.Count;
        }

        [Benchmark]
        public int Simple()
        {
            var result = _scannerFromFile.SimpleFindAllPatterns("6A 44 2F 13 1D 31 D2 A8 08 14 E8 0A 79");
            return result.Count;
        }
    }
}