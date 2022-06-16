namespace Reloaded.Memory.Sigscan.Benchmark.Benchmarks
{
    public static class BenchmarkUtils
    {
        // ReSharper disable once InconsistentNaming
        public static double BytesToMB(long bytes)
        {
            return bytes / 1000.0  // to kb
                         / 1000.0; // to mb
        }

    }
}
