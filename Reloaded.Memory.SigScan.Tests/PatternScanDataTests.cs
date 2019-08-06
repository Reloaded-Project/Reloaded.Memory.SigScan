using System;
using Reloaded.Memory.Sigscan.Structs;
using Xunit;

namespace Reloaded.Memory.SigScan.Tests
{
    public class PatternScanDataTests
    {
        [Fact]
        public void InstantiateInstructions()
        {
            string pattern = "11 22 33 ?? 55";
            var target = PatternScanInstructionSet.FromStringPattern(pattern);

            // Instructions:
            // Compare Short
            // Compare Byte
            // Skip
            // Compare Byte

            Assert.Equal(new byte[] { 0x11, 0x22, 0x33, 0x55 }, target.Bytes);
            Assert.Equal(5, target.Length);
        }

        [Fact]
        public void InstantiateSimple()
        {
            string pattern = "11 22 33 ?? 55";
            var target = new SimplePatternScanData(pattern);

            Assert.Equal(new byte[] { 0x01, 0x01, 0x01, 0x00, 0x01 }, target.Mask);
            Assert.Equal(new byte[] { 0x11, 0x22, 0x33, 0x55 }, target.Bytes);
        }
    }
}
