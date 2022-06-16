using System;
using System.Linq;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Reloaded.Memory.Sigscan.Benchmark.Benchmarks;
using Reloaded.Memory.Sigscan.Benchmark.Benchmarks.Multithread;

namespace Reloaded.Memory.Sigscan.Benchmark.Columns
{
    public class Speed : IColumn
    {
        public Func<Summary, BenchmarkCase, long> GetFileSizeBytes;

        public Speed(Func<Summary, BenchmarkCase, long> getFileSize)
        {
            GetFileSizeBytes = getFileSize;
        }

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
        {
            var ourReport = summary.Reports.First(x => x.BenchmarkCase.Equals(benchmarkCase));
            var mean = ourReport.ResultStatistics.Mean;
            var meanSeconds = mean / 1000_000_000F; // ns to seconds
            var sizeMb = BenchmarkUtils.BytesToMB(GetFileSizeBytes(summary, benchmarkCase));

            // Convert to MB/s.
            return $"{(sizeMb / meanSeconds):#####.00}";
        }

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);
        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
        public bool IsAvailable(Summary summary) => true;

        public string Id { get; } = nameof(Speed);
        public string ColumnName { get; } = "Speed (MB/s)";
        public bool AlwaysShow { get; } = true;
        public ColumnCategory Category { get; } = ColumnCategory.Custom;
        public int PriorityInCategory { get; } = 0;
        public bool IsNumeric { get; } = false;
        public UnitType UnitType { get; } = UnitType.Dimensionless;
        public string Legend { get; } = "The speed of pattern checking in megabytes per second";
    }
}
