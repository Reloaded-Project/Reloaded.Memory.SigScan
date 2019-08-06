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
        public int CompiledMediumPatternMiddle()
        {
            var result = _scannerFromFile.CompiledFindPattern("DA 69 64 A8 FE B9");
            return result.Offset;
        }

        [Benchmark]
        public int CompiledMediumPatternMiddleWithMask()
        {
            var result = _scannerFromFile.CompiledFindPattern("DA 69 ?? ?? FE B9");
            return result.Offset;
        }

        [Benchmark]
        public int CompiledShortPatternAtEnd()
        {
            var result = _scannerFromFile.CompiledFindPattern("41 B8 25 FA");
            return result.Offset;
        }

        /* Simple */

        [Benchmark]
        public int SimpleMediumPatternMiddle()
        {
            var result = _scannerFromFile.SimpleFindPattern("DA 69 64 A8 FE B9");
            return result.Offset;
        }

        [Benchmark]
        public int SimpleMediumPatternMiddleWithMask()
        {
            var result = _scannerFromFile.SimpleFindPattern("DA 69 ?? ?? FE B9");
            return result.Offset;
        }

        [Benchmark]
        public int SimpleShortPatternAtEnd()
        {
            var result = _scannerFromFile.SimpleFindPattern("41 B8 25 FA");
            return result.Offset;
        }
    }
}
