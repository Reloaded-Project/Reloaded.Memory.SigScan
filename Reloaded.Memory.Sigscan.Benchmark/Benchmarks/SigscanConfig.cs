using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using Reloaded.Memory.Sigscan.Benchmark.Columns;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks
{
    internal class SigscanConfig : ManualConfig
    {
        public SigscanConfig(float speedMegabytesOffset)
        {
            Add(DefaultConfig.Instance);

            AddJob(Job.ShortRun
                .WithToolchain(InProcessEmitToolchain.Instance)
                .WithId(".NET (Current Process)"));

            //AddJob(Job.Default.WithRuntime(CoreRuntime.Core31));
            AddExporter(MarkdownExporter.GitHub);
            AddColumn(BaselineRatioColumn.RatioMean);
            AddColumn(new Speed(speedMegabytesOffset));
            AddLogger(new ConsoleLogger());
            WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest));
        }
    }
}
