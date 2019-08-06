using System;
using BenchmarkDotNet.Running;

namespace Reloaded.Memory.Sigscan.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //BenchmarkRunner.Run<ScannerSmallArray>();
            //BenchmarkRunner.Run<ScannerMediumArray>();
            BenchmarkRunner.Run<ScannerLargeArray>();

            /*
            var lrgArr = new ScannerLargeArray();
            for (int x = 0; x < 10000; x++)
            {
                lrgArr.SimpleMediumPatternWithMaskMiddle();
            }
            */
        }
    }
}
