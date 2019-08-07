using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using Reloaded.Memory.Sigscan.Benchmark.Columns;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.LargeArray
{
    [Config(typeof(Config))]
    public class LongPatternWithMaskEnd
    {
        private static byte[] _dataFromFile     = File.ReadAllBytes(Constants.File);
        private static Scanner _scannerFromFile = new Scanner(_dataFromFile);

        private class Config : ManualConfig
        {
            public Config()
            {
                Add(Job.Core);
                Add(MarkdownExporter.GitHub);
                Add(new Speed(3.145719F));
            }
        }

        [Benchmark]
        public int Compiled()
        {
            var result = _scannerFromFile.CompiledFindPattern("9F 43 ?? ?? 43 4F 99 ?? ?? 48");
            return result.Offset;
        }

        [Benchmark]
        public int Simple()
        {
            var result = _scannerFromFile.SimpleFindPattern("9F 43 ?? ?? 43 4F 99 ?? ?? 48");
            return result.Offset;
        }
    }
}
