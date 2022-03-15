using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;
using Reloaded.Memory.Sigscan.Benchmark.Columns;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks
{
    internal class SigscanConfig : ManualConfig
    {
        public SigscanConfig(float speedMegabytesOffset)
        {
            Add(DefaultConfig.Instance);

            AddJob(Job.ShortRun
                .WithPlatform(Platform.X86)
                .WithToolchain(CsProjCoreToolchain.From(NetCoreAppSettings.NetCoreApp50.WithCustomDotNetCliPath(@"C:\Program Files (x86)\dotnet\dotnet.exe")))
                .WithId(".NET 5 (x86)"));

            /*
            AddJob(Job.ShortRun
                .WithPlatform(Platform.X64)
                .WithToolchain(CsProjCoreToolchain.From(NetCoreAppSettings.NetCoreApp50.WithCustomDotNetCliPath(@"C:\Program Files\dotnet\dotnet.exe")))
                .WithId(".NET 5 (x64)"));
            */

            //AddJob(Job.Default.WithRuntime(CoreRuntime.Core31));
            AddExporter(MarkdownExporter.GitHub);
            AddColumn(BaselineRatioColumn.RatioMean);
            AddColumn(new Speed(speedMegabytesOffset));
            AddLogger(new ConsoleLogger());
            WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest));
        }
    }
}
