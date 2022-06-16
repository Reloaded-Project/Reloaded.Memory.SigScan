using System.Globalization;
using Reloaded.Memory.Sigscan.Utility;

namespace Reloaded.Memory.Sigscan.Structs;

/// <summary>
/// [Internal and Test Use]
/// Represents the pattern to be searched by the scanner.
/// </summary>
public ref struct SimplePatternScanData
{
    private static char[] _maskIgnore = { '?', '?' };
    private static List<byte> _bytes       = new List<byte>(1024);
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
    /// The original string from which this pattern was created.
    /// </summary>
    public string Pattern;

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
        Pattern = stringPattern;

#if SPAN_API
        var enumerator       = new SpanSplitEnumerator<char>(stringPattern, ' ');
#else
        var enumerator       = new SpanSplitEnumerator<char>(new ReadOnlySpan<char>(stringPattern.ToCharArray()), ' ');
#endif
        var questionMarkFlag = new ReadOnlySpan<char>(_maskIgnore);

        lock (_buildLock)
        {
            _maskBuilder.Clear();
            _bytes.Clear();

            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Equals(questionMarkFlag, StringComparison.Ordinal))
                {
                    _maskBuilder.Add(0x0);
                }
                else
                {
#if SPAN_API
                    _bytes.Add(byte.Parse(enumerator.Current, NumberStyles.AllowHexSpecifier));
#else
                    _bytes.Add(byte.Parse(enumerator.Current.ToString(), NumberStyles.AllowHexSpecifier));
#endif
                    _maskBuilder.Add(0x1);
                }

            }

            Mask   = _maskBuilder.ToArray();
            Bytes  = _bytes.ToArray();
        }
    }

    /// <summary>
    /// Implicitly converts a string to a scan pattern.
    /// </summary>
    public static implicit operator SimplePatternScanData(string pattern) => new(pattern);
}