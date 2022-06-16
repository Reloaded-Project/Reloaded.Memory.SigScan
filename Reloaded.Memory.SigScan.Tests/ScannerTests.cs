using System.Diagnostics;
using Reloaded.Memory.Sigscan;
using Xunit;

namespace Reloaded.Memory.SigScan.Tests
{
    public class ScannerTests : ScannerTestBase
    {
        [Fact]
        public void InstantiateFromCurrentProcess()
        {
            // Test fails if function throws.
            var thisProcess = Process.GetCurrentProcess();
            var scanner     = new Scanner(thisProcess, thisProcess.MainModule);
        }

        [Fact]
        public void InstantiateFromCurrentProcess_NewApi()
        {
            // Test fails if function throws.
            var scanner = new Scanner(Process.GetCurrentProcess());
        }

        [Fact]
        public void FindBasicPattern()
        {
            var scanner = new Scanner(_data);
            var resultCompiled = scanner.FindPattern_Compiled("04 25 12 2B 86 E5 E3");
            var resultCompiledAvx = scanner.FindPattern_Avx2("04 25 12 2B 86 E5 E3");
            var resultCompiledSse = scanner.FindPattern_Sse2("04 25 12 2B 86 E5 E3");
            var resultSimple = scanner.FindPattern_Simple("04 25 12 2B 86 E5 E3");

            Assert.Equal(resultCompiledSse, resultCompiledAvx);
            Assert.Equal(resultCompiledSse, resultSimple);
            Assert.Equal(resultCompiled, resultSimple);
            Assert.True(resultCompiled.Found);
            Assert.Equal(9, resultCompiled.Offset);
        }

        [Fact]
        public void FindPatternWithMask()
        {
            var scanner = new Scanner(_data);
            var resultCompiled = scanner.FindPattern_Compiled("04 25 ?? ?? 86 E5 E3");
            var resultCompiledAvx = scanner.FindPattern_Avx2("04 25 ?? ?? 86 E5 E3");
            var resultCompiledSse = scanner.FindPattern_Sse2("04 25 ?? ?? 86 E5 E3");
            var resultSimple = scanner.FindPattern_Simple("04 25 ?? ?? 86 E5 E3");

            Assert.Equal(resultCompiledSse, resultCompiledAvx);
            Assert.Equal(resultCompiledSse, resultSimple);
            Assert.Equal(resultCompiled, resultSimple);
            Assert.True(resultCompiled.Found);
            Assert.Equal(9, resultCompiled.Offset);
        }

        [Fact]
        public void FindPatternWithLongAndMask()
        {
            var scanner = new Scanner(_data);
            var resultCompiled = scanner.FindPattern_Compiled("04 25 12 2B 86 E5 E3 21 AF A3 ?? ?? 71 D1");
            var resultCompiledAvx = scanner.FindPattern_Avx2("04 25 12 2B 86 E5 E3 21 AF A3 ?? ?? 71 D1");
            var resultCompiledSse = scanner.FindPattern_Sse2("04 25 12 2B 86 E5 E3 21 AF A3 ?? ?? 71 D1");
            var resultSimple = scanner.FindPattern_Simple("04 25 12 2B 86 E5 E3 21 AF A3 ?? ?? 71 D1");

            Assert.Equal(resultCompiledSse, resultCompiledAvx);
            Assert.Equal(resultCompiledSse, resultSimple);
            Assert.Equal(resultCompiled, resultSimple);
            Assert.True(resultSimple.Found);
            Assert.Equal(9, resultSimple.Offset);
        }

        [Fact]
        public void FindPatternStartingWithSkip()
        {
            var scanner = new Scanner(_data);
            var resultCompiled = scanner.FindPattern_Compiled("?? 25 ?? ?? 86 E5 E3 ?? AF A3 ??");
            var resultCompiledAvx = scanner.FindPattern_Avx2("?? 25 ?? ?? 86 E5 E3 ?? AF A3 ??");
            var resultCompiledSse = scanner.FindPattern_Sse2("?? 25 ?? ?? 86 E5 E3 ?? AF A3 ??");
            var resultSimple   = scanner.FindPattern_Simple("?? 25 ?? ?? 86 E5 E3 ?? AF A3 ??");

            Assert.Equal(resultCompiledSse, resultCompiledAvx);
            Assert.Equal(resultCompiledSse, resultSimple);
            Assert.Equal(resultCompiled, resultSimple);
            Assert.True(resultCompiled.Found);
            Assert.Equal(9, resultCompiled.Offset);
        }

        [Fact]
        public void FindPatternAtEndOfData()
        {
            var scanner = new Scanner(_data);
            var resultCompiled = scanner.FindPattern_Compiled("7A BB");
            var resultCompiledAvx = scanner.FindPattern_Avx2("7A BB");
            var resultCompiledSse = scanner.FindPattern_Sse2("7A BB");
            var resultSimple   = scanner.FindPattern_Simple("7A BB");

            Assert.Equal(resultCompiledSse, resultCompiledAvx);
            Assert.Equal(resultCompiledSse, resultSimple);
            Assert.Equal(resultCompiledSse, resultCompiled);
            Assert.True(resultCompiled.Found);
            Assert.Equal(254, resultCompiled.Offset);
        }

        [Fact]
        public void FindFirstByte()
        {
            var scanner = new Scanner(_data);
            var resultCompiled = scanner.FindPattern_Compiled("D3 B2 7A");
            var resultCompiledAvx = scanner.FindPattern_Avx2("D3 B2 7A");
            var resultCompiledSse = scanner.FindPattern_Sse2("D3 B2 7A");
            var resultSimple = scanner.FindPattern_Simple("D3 B2 7A");

            Assert.Equal(resultCompiledSse, resultCompiledAvx);
            Assert.Equal(resultCompiledSse, resultSimple);
            Assert.Equal(resultCompiled, resultSimple);
            Assert.True(resultCompiled.Found);
            Assert.Equal(0, resultCompiled.Offset);
        }

        [Fact]
        public void FindLastByte()
        {
            var scanner = new Scanner(_data);
            var resultCompiled = scanner.FindPattern_Compiled("BB");
            var resultCompiledAvx = scanner.FindPattern_Avx2("BB");
            var resultCompiledSse = scanner.FindPattern_Sse2("BB");
            var resultSimple = scanner.FindPattern_Simple("BB");

            Assert.Equal(resultCompiledSse, resultCompiledAvx);
            Assert.Equal(resultCompiledSse, resultSimple);
            Assert.Equal(resultCompiledSse, resultCompiled);
            Assert.True(resultCompiled.Found);
            Assert.Equal(255, resultCompiled.Offset);
        }

        [Fact]
        public void PatternNotFound()
        {
            var scanner = new Scanner(_data);
            var resultCompiled = scanner.FindPattern_Compiled("7A BB CC DD EE FF");
            var resultCompiledAvx = scanner.FindPattern_Avx2("7A BB CC DD EE FF");
            var resultCompiledSse = scanner.FindPattern_Sse2("7A BB CC DD EE FF");
            var resultSimple   = scanner.FindPattern_Simple("7A BB CC DD EE FF");

            Assert.Equal(resultCompiledSse, resultCompiledAvx);
            Assert.Equal(resultCompiledSse, resultSimple);
            Assert.Equal(resultCompiled, resultSimple);
            Assert.False(resultCompiled.Found);
            Assert.Equal(-1, resultCompiled.Offset);
        }
    }
}
