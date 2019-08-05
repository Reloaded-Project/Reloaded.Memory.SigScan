using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Reloaded.Memory.Sigscan.Benchmark
{
    [CoreJob]
    public class ScannerMediumArray
    {
        private static byte[]  _dataFromFile = File.ReadAllBytes("16KBRandom");
        private static Scanner _scannerFromFile = new Scanner(_dataFromFile);

        [Benchmark]
        public int CompiledFindBasicPatternMiddle()
        {
            var result = _scannerFromFile.CompiledFindPattern("DA 69 64 A8 FE B9");
            return result.Offset;
        }

        [Benchmark]
        public int CompiledFindPatternMiddleWithMask()
        {
            var result = _scannerFromFile.CompiledFindPattern("DA 69 ?? ?? FE B9");
            return result.Offset;
        }

        [Benchmark]
        public int CompiledFindPatternAtEnd()
        {
            var result = _scannerFromFile.CompiledFindPattern("41 B8 25 FA");
            return result.Offset;
        }

        /* Simple */

        [Benchmark]
        public int SimpleFindBasicPatternMiddle()
        {
            var result = _scannerFromFile.SimpleFindPattern("DA 69 64 A8 FE B9");
            return result.Offset;
        }

        [Benchmark]
        public int SimpleFindPatternMiddleWithMask()
        {
            var result = _scannerFromFile.SimpleFindPattern("DA 69 ?? ?? FE B9");
            return result.Offset;
        }

        [Benchmark]
        public int SimpleFindPatternAtEnd()
        {
            var result = _scannerFromFile.SimpleFindPattern("41 B8 25 FA");
            return result.Offset;
        }
    }
}
