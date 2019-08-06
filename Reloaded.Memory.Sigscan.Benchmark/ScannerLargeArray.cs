using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Reloaded.Memory.Sigscan.Benchmark
{
    [CoreJob]
    public class ScannerLargeArray
    {
        private static byte[]  _dataFromFile = File.ReadAllBytes("1MBRandom");
        private static Scanner _scannerFromFile = new Scanner(_dataFromFile);

        
        [Benchmark]
        public int CompiledShortPatternMiddle()
        {
            var result = _scannerFromFile.CompiledFindPattern("84 2F F9");
            return result.Offset;
        }

        [Benchmark]
        public int CompiledMediumPatternWithMaskMiddle()
        {
            var result = _scannerFromFile.CompiledFindPattern("84 2F ?? ?? 6C 46");
            return result.Offset;
        }

        [Benchmark]
        public int CompiledShortPatternEnd()
        {
            var result = _scannerFromFile.CompiledFindPattern("0F D7 9F");
            return result.Offset;
        }

        [Benchmark]
        public int CompiledMediumPatternWithMaskEnd()
        {
            var result = _scannerFromFile.CompiledFindPattern("D7 9F ?? ?? 68 43");
            return result.Offset;
        }

        [Benchmark]
        public int CompiledMediumPatternEnd()
        {
            var result = _scannerFromFile.CompiledFindPattern("D7 9F 43 63 68 43");
            return result.Offset;
        }

        [Benchmark]
        public int CompiledLongPatternEnd()
        {
            var result = _scannerFromFile.CompiledFindPattern("9F 43 63 68 43 4F 99 A7 15 48");
            return result.Offset;
        }

        /* Simple */
        
        [Benchmark]
        public int SimpleShortPatternMiddle()
        {
            var result = _scannerFromFile.SimpleFindPattern("84 2F F9");
            return result.Offset;
        }

        [Benchmark]
        public int SimpleMediumPatternWithMaskMiddle()
        {
            var result = _scannerFromFile.SimpleFindPattern("84 2F ?? ?? 6C 46");
            return result.Offset;
        }

        [Benchmark]
        public int SimpleShortPatternEnd()
        {
            var result = _scannerFromFile.SimpleFindPattern("0F D7 9F");
            return result.Offset;
        }

        [Benchmark]
        public int SimpleMediumPatternEnd()
        {
            var result = _scannerFromFile.SimpleFindPattern("D7 9F 43 63 68 43");
            return result.Offset;
        }

        [Benchmark]
        public int SimpleMediumPatternWithMaskEnd()
        {
            var result = _scannerFromFile.SimpleFindPattern("D7 9F ?? ?? 68 43");
            return result.Offset;
        }

        [Benchmark]
        public int SimpleLongPatternEnd()
        {
            var result = _scannerFromFile.SimpleFindPattern("9F 43 63 68 43 4F 99 A7 15 48");
            return result.Offset;
        }
        
    }
}
