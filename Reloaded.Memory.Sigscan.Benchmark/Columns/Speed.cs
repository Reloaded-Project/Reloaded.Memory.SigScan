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
        public double FileSizeMB;
        public BenchmarkKind Kind;

        public Speed(BenchmarkKind benchmarkKind, long fileSize)
        {
            FileSizeMB = BenchmarkUtils.BytesToMB(fileSize);
            Kind = benchmarkKind;
        }

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
        {
            var ourReport = summary.Reports.First(x => x.BenchmarkCase.Equals(benchmarkCase));
            var mean = ourReport.ResultStatistics.Mean;
            var meanSeconds = mean / 1000_000_000F; // ns to seconds

            // Suppport Multithreaded benchmark.
            if (Kind == BenchmarkKind.Multithreaded || Kind == BenchmarkKind.MultithreadedRandom)
            {
                var numItems = benchmarkCase.Parameters[nameof(LargeArrayMtBenchmarkBase.NumItems)];
                if (Kind == BenchmarkKind.Multithreaded) 
                    return $"{(double)FileSizeMB * (int)numItems / meanSeconds}";

                return $"{(double)RandomMt.NumItemsToTotalSizeMB[(int)numItems] / meanSeconds}";
            }
            
            // Convert to seconds.
            return $"{(double)FileSizeMB / meanSeconds}";
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
