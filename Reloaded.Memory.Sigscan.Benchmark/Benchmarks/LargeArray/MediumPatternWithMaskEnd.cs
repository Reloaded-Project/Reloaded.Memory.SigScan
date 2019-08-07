using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using Reloaded.Memory.Sigscan.Benchmark.Columns;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.LargeArray
{
    [Config(typeof(Config))]
    public class MediumPatternWithMaskEnd
    {
        private static byte[] _dataFromFile = File.ReadAllBytes(Constants.File);
        private static Scanner _scannerFromFile = new Scanner(_dataFromFile);

        private class Config : ManualConfig
        {
            public Config()
            {
                Add(Job.Core);
                Add(MarkdownExporter.GitHub);
                Add(new Speed(3.145405F));
            }
        }

        [Benchmark]
        public int Compiled()
        {
            var result = _scannerFromFile.CompiledFindPattern("A0 4E ?? ?? 0E ED");
            return result.Offset;
        }

        [Benchmark]
        public int Simple()
        {
            var result = _scannerFromFile.SimpleFindPattern("A0 4E ?? ?? 0E ED");
            return result.Offset;
        }
    }
}
