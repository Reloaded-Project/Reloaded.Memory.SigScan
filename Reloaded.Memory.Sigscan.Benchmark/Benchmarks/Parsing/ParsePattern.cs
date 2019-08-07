using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Reloaded.Memory.Sigscan.Structs;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.Parsing
{
    [CoreJob]
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class ParsePattern
    {
        [Benchmark]
        public PatternScanInstructionSet MakeInstructionSet()
        {
            return PatternScanInstructionSet.FromStringPattern("DA 69 ?? ?? FE B9");
        }

        [Benchmark]
        public SimplePatternScanData MakeScanData()
        {
            return new SimplePatternScanData("DA 69 ?? ?? FE B9");
        }
    }
}
