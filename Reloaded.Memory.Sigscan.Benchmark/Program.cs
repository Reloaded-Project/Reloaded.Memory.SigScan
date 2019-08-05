using System;
using BenchmarkDotNet.Running;

namespace Reloaded.Memory.Sigscan.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            BenchmarkRunner.Run<ScannerMediumArray>();
            /*
            var medArr = new ScannerMediumArray();
            for (int x = 0; x < 10000; x++)
            {
                medArr.CompiledFindBasicPatternMiddle();
            }
            */
        }
    }
}
