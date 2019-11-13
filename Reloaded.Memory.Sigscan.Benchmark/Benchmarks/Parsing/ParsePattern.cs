using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Reloaded.Memory.Sigscan.Structs;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.Parsing
{
    [CoreJob]
    [MemoryDiagnoser]
    public class ParsePattern
    {
        // With Mask
        [Benchmark]
        public PatternScanInstructionSet MakeInstructionSetWithMask()
        {
            return new PatternScanInstructionSet("DA 69 ?? ?? FE B9");
        }

        [Benchmark]
        public SimplePatternScanData MakeScanDataWithMask()
        {
            return new SimplePatternScanData("DA 69 ?? ?? FE B9");
        }

        // Without Mask
        [Benchmark]
        public PatternScanInstructionSet MakeInstructionSetWithoutMask()
        {
            return new PatternScanInstructionSet("DA 69 DD AA FE B9");
        }

        [Benchmark]
        public SimplePatternScanData MakeScanDataWithoutMask()
        {
            return new SimplePatternScanData("DA 69 DD AA FE B9");
        }

        // No Mask + Long
        [Benchmark]
        public PatternScanInstructionSet MakeInstructionSetWithoutMaskLong()
        {
            return new PatternScanInstructionSet("DA 69 DD AA FE B9 BB CC DD EE FF");
        }

        [Benchmark]
        public SimplePatternScanData MakeScanDataWithoutMaskLong()
        {
            return new SimplePatternScanData("DA 69 DD AA FE B9 BB CC DD EE FF");
        }
    }
}
