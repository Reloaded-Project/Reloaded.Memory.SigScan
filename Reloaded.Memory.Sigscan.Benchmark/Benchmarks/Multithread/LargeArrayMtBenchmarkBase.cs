using BenchmarkDotNet.Attributes;
using Reloaded.Memory.Sigscan.Benchmark.Benchmarks.LargeArray;

namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks.Multithread
{
    public class LargeArrayMtBenchmarkBase : LargeArrayBenchmarkBase
    {
        // DO NOT RENAME.
        [Params(1, 8, 128, 512)]
        public int NumItems;
    }
}
