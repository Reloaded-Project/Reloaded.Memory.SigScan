using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using Reloaded.Memory.Sigscan.Benchmark.Benchmarks.Multithread;
using Reloaded.Memory.Sigscan.Benchmark.Columns;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks
{
    internal class SigscanConfig : ManualConfig
    {
        public SigscanConfig(BenchmarkKind kind, long fileSize)
        {
            Add(DefaultConfig.Instance);

            AddJob(Job.Default
                .WithToolchain(InProcessEmitToolchain.Instance)
                .WithId(".NET (Current Process)"));

            //AddJob(Job.Default.WithRuntime(CoreRuntime.Core31));
            AddExporter(MarkdownExporter.GitHub);
            AddColumn(BaselineRatioColumn.RatioMean);
            AddColumn(new Speed(kind, fileSize));
            AddLogger(new ConsoleLogger());
            WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest));

            // Filter out singlethreaded where NumItems > 1
            if (kind == BenchmarkKind.Multithreaded || kind == BenchmarkKind.MultithreadedRandom)
            {
                AddFilter(new SimpleFilter(benchCase =>
                {
                    var numItems = (int) benchCase.Parameters[nameof(LargeArrayMtBenchmarkBase.NumItems)];
                    switch (numItems)
                    {
                        case 1 when benchCase.Descriptor.WorkloadMethod.Name.Contains("_MT"):
                        case > 1 when benchCase.Descriptor.WorkloadMethod.Name.Contains("_ST"):
                            return false;
                        default:
                            return true;
                    }
                }));
            }
        }
    }

    public enum BenchmarkKind
    {
        Default,
        Multithreaded,
        MultithreadedRandom
    }
}
