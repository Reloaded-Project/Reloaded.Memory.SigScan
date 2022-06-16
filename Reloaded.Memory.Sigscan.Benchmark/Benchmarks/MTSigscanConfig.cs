using System;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Reloaded.Memory.Sigscan.Benchmark.Benchmarks.Multithread;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks
{
    /// <inheritdoc />
    internal class MTSigscanConfig : SigscanConfig
    {
        public MTSigscanConfig(Func<Summary, BenchmarkCase, long> getFileSize) : base(getFileSize)
        {
            AddFilter(new SimpleFilter(benchCase =>
            {
                var numItems = benchCase.Parameters[nameof(RandomMt.NumItems)];
                if (numItems == null)
                    return true;

                switch ((int)numItems)
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
