using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Reloaded.Memory.Sigscan.Benchmark.Columns
{
    public class Speed : IColumn
    {
        public float MegaBytesOffset;

        public Speed(float megaBytesOffset)
        {
            MegaBytesOffset = megaBytesOffset;
        }

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
        {
            var ourReport = summary.Reports.First(x => x.BenchmarkCase.Equals(benchmarkCase));
            var mean = ourReport.ResultStatistics.Mean;
            var meanSeconds = mean / 1000000000F;

            // Convert to seconds.
            return $"{MegaBytesOffset / meanSeconds}";
        }

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);
        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
        public bool IsAvailable(Summary summary) => true;

        public string Id { get; } = nameof(Speed);
        public string ColumnName { get; } = "Speed";
        public bool AlwaysShow { get; } = true;
        public ColumnCategory Category { get; } = ColumnCategory.Custom;
        public int PriorityInCategory { get; } = 0;
        public bool IsNumeric { get; } = false;
        public UnitType UnitType { get; } = UnitType.Dimensionless;
        public string Legend { get; } = "The speed of pattern checking in megabytes per second";
    }
}
