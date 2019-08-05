using System;
using System.Collections.Generic;
using System.Text;

namespace Reloaded.Memory.Sigscan.Structs
{
    /// <summary>
    /// [Internal & Test Use]
    /// Represents the pattern to be searched by the scanner.
    /// </summary>
    public struct SimplePatternScanData
    {
        private static List<byte> _bytes = new List<byte>(1024);
        private static List<byte> _maskBuilder = new List<byte>(1024);
        private static object _buildLock = new object();

        /// <summary>
        /// The pattern of bytes to check for.
        /// </summary>
        public byte[] Bytes;

        /// <summary>
        /// The mask string to compare against. `x` represents check while `?` ignores.
        /// Each `x` and `?` represent 1 byte.
        /// </summary>
        public byte[] Mask;

        /// <summary>
        /// Creates a new pattern scan target given a string representation of a pattern.
        /// </summary>
        /// <param name="stringPattern">
        ///     The pattern to look for inside the given region.
        ///     Example: "11 22 33 ?? 55".
        ///     Key: ?? represents a byte that should be ignored, anything else if a hex byte. i.e. 11 represents 0x11, 1F represents 0x1F.
        /// </param>
        public SimplePatternScanData(string stringPattern)
        {
            string[] segments = stringPattern.Split(' ');
            lock (_buildLock)
            {
                _maskBuilder.Clear();
                _bytes.Clear();

                foreach (var segment in segments)
                {
                    if (segment == "??")
                        _maskBuilder.Add(0x0);
                    else
                    {
                        _bytes.Add(Convert.ToByte(segment, 16));
                        _maskBuilder.Add(0x1);
                    }
                }

                Mask   = _maskBuilder.ToArray();
                Bytes  = _bytes.ToArray();
            }
        }
    }
}
