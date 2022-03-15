using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using Reloaded.Memory.Sigscan.Benchmark.Columns;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks
{
    internal class SigscanConfig : ManualConfig
    {
        public SigscanConfig(float speedMegabytesOffset)
        {
            Add(DefaultConfig.Instance);
            AddJob(Job.Default.WithRuntime(CoreRuntime.Core60));
            AddJob(Job.Default.WithRuntime(CoreRuntime.Core31));
            AddExporter(MarkdownExporter.GitHub);
            AddColumn(BaselineRatioColumn.RatioMean);
            AddColumn(new Speed(speedMegabytesOffset));
            AddLogger(new ConsoleLogger());
            WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest));
        }
    }
}
