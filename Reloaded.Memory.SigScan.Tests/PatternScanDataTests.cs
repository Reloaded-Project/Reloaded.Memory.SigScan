using System;
using Reloaded.Memory.Sigscan.Structs;
using Reloaded.Memory.Sigscan.Utility;
using Xunit;

namespace Reloaded.Memory.SigScan.Tests
{
    public class PatternScanDataTests
    {
        // InstructionSet no longer tested here, in case of future optimizations.
        // Integration tests will reveal if it works correctly.

        [Fact]
        public void InstantiateSimple()
        {
            string pattern = "11 22 33 ?? 55";
            var target = new SimplePatternScanData(pattern);

            Assert.Equal(new byte[] { 0x01, 0x01, 0x01, 0x00, 0x01 }, target.Mask);
            Assert.Equal(new byte[] { 0x11, 0x22, 0x33, 0x55 }, target.Bytes);
        }

        [Fact]
        public void StringSplitterNormal()
        {
            string pattern = "11 22 33 ?? 55";
            string[] splitSegments = { "11", "22", "33", "??", "55" };

            var splitEnumerator = new SpanSplitEnumerator<char>(pattern, ' ');
            for (int x = 0; x < splitSegments.Length; x++)
            {
                Assert.True(splitEnumerator.MoveNext());
                Assert.Equal(splitSegments[x], splitEnumerator.Current.ToString());
            }

            Assert.False(splitEnumerator.MoveNext());
        }

        [Fact]
        public void StringSplitterSingle()
        {
            string pattern = "11";

            var splitEnumerator = new SpanSplitEnumerator<char>(pattern, ' ');

            Assert.True(splitEnumerator.MoveNext());
            Assert.Equal("11", splitEnumerator.Current.ToString());
            Assert.False(splitEnumerator.MoveNext());
        }

        [Fact]
        public void StringSplitterEmpty()
        {
            string pattern = "";
            var splitEnumerator = new SpanSplitEnumerator<char>(pattern, ' ');

            Assert.True(splitEnumerator.MoveNext());
            Assert.Equal("", splitEnumerator.Current.ToString());
            Assert.False(splitEnumerator.MoveNext());
        }
    }
}
