using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Reloaded.Memory.Sigscan.Structs;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.Parsing
{
    [SimpleJob(RuntimeMoniker.CoreRt60)]
    [MemoryDiagnoser]
    public class ParsePattern
    {
        // With Mask
        [Benchmark]
        public CompiledScanPattern MakeInstructionSetWithMask()
        {
            return new CompiledScanPattern("DA 69 ?? ?? FE B9");
        }

        [Benchmark]
        public SimplePatternScanData MakeScanDataWithMask()
        {
            return new SimplePatternScanData("DA 69 ?? ?? FE B9");
        }

        // Without Mask
        [Benchmark]
        public CompiledScanPattern MakeInstructionSetWithoutMask()
        {
            return new CompiledScanPattern("DA 69 DD AA FE B9");
        }

        [Benchmark]
        public SimplePatternScanData MakeScanDataWithoutMask()
        {
            return new SimplePatternScanData("DA 69 DD AA FE B9");
        }

        // No Mask + Long
        [Benchmark]
        public CompiledScanPattern MakeInstructionSetWithoutMaskLong()
        {
            return new CompiledScanPattern("DA 69 DD AA FE B9 BB CC DD EE FF");
        }

        [Benchmark]
        public SimplePatternScanData MakeScanDataWithoutMaskLong()
        {
            return new SimplePatternScanData("DA 69 DD AA FE B9 BB CC DD EE FF");
        }
    }
}
