using System;
using System.Collections.Generic;
using System.Globalization;
using Reloaded.Memory.Sigscan.Utility;

namespace Reloaded.Memory.Sigscan.Structs;

#if SIMD_INTRINSICS
/// <summary>
/// [Internal and Test Use]
/// Represents the pattern to be searched by the scanner.
/// </summary>
public ref struct SimdPatternScanData
{
    private static char[] _maskIgnore = { '?', '?' };
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
    /// Count of leading ?? symbols (at start of pattern).
    /// </summary>
    public int LeadingIgnoreCount;

    /// <summary>
    /// Creates a new pattern scan target given a string representation of a pattern.
    /// </summary>
    /// <param name="stringPattern">
    ///     The pattern to look for inside the given region.
    ///     Example: "11 22 33 ?? 55".
    ///     Key: ?? represents a byte that should be ignored, anything else if a hex byte. i.e. 11 represents 0x11, 1F represents 0x1F.
    /// </param>
    public SimdPatternScanData(string stringPattern)
    {
        LeadingIgnoreCount = 0;

        var enumerator = new SpanSplitEnumerator<char>(stringPattern, ' ');
        var questionMarkFlag = new ReadOnlySpan<char>(_maskIgnore);
        bool foundNonIgnore = false;

        lock (_buildLock)
        {
            _maskBuilder.Clear();
            _bytes.Clear();

            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Equals(questionMarkFlag, StringComparison.Ordinal))
                {
                    _maskBuilder.Add(0x0);
                    _bytes.Add(0x0);
                    if (foundNonIgnore == false)
                        LeadingIgnoreCount += 1;
                }
                else
                {
                    _bytes.Add(byte.Parse(enumerator.Current, NumberStyles.AllowHexSpecifier));
                    _maskBuilder.Add(0x1);
                    foundNonIgnore = true;
                }

            }

            Mask = _maskBuilder.ToArray();
            Bytes = _bytes.ToArray();
        }
    }
}
#endif