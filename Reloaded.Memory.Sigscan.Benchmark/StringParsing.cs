using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Reloaded.Memory.Sigscan.Utility;

namespace Reloaded.Memory.Sigscan.Benchmark
{
    [CoreJob]
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class StringParsing
    {
        private static string _pattern = "11 22 33 ?? 55";

        [Benchmark]
        public string[] StringSplit()
        {
            return _pattern.Split(' ');
        }

        [Benchmark]
        public SpanSplitEnumerator<char> SpanSplit()
        {
            var enumerator = new SpanSplitEnumerator<char>(_pattern, ' ');
            while (enumerator.MoveNext())
            { }

            return enumerator;
        }
    }
}
