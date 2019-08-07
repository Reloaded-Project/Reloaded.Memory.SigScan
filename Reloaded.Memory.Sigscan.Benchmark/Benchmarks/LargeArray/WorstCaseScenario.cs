using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using Reloaded.Memory.Sigscan.Benchmark.Columns;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.LargeArray
{
    [Config(typeof(Config))]
    public class WorstCaseScenario
    {
        private static byte[] _dataFromFile     = File.ReadAllBytes(Constants.WorstCaseFile);
        private static Scanner _scannerFromFile = new Scanner(_dataFromFile);

        private class Config : ManualConfig
        {
            public Config()
            {
                Add(Job.Core);
                Add(MarkdownExporter.GitHub);
                Add(new Speed(2.999992F));
            }
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
