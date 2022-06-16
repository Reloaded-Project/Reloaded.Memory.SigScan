using System.Runtime.CompilerServices;
using Reloaded.Memory.Sigscan.Definitions.Structs;
using Reloaded.Memory.Sigscan.Structs;

namespace Reloaded.Memory.Sigscan;

public unsafe partial class Scanner
{
    /// <summary>
    /// Attempts to find a given pattern inside the memory region this class was created with.
    /// This method uses the simple search, which simply iterates over all bytes, reading max 1 byte at once.
    /// This method generally works better when the expected offset is smaller than 4096.
    /// </summary>
    /// <param name="data">Address of the data to be scanned.</param>
    /// <param name="dataLength">Length of the data to be scanned.</param>
    /// <param name="pattern">
    ///     The pattern to look for inside the given region.
    ///     Example: "11 22 33 ?? 55".
    ///     Key: ?? represents a byte that should be ignored, anything else if a hex byte. i.e. 11 represents 0x11, 1F represents 0x1F
    /// </param>
    /// <returns>A result indicating an offset (if found) of the pattern.</returns>
#if NET5_0_OR_GREATER
    [SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#endif
    public static PatternScanResult FindPatternSimple(byte* data, int dataLength, SimplePatternScanData pattern)
    {
        var patternData = pattern.Bytes;
        var patternMask = pattern.Mask;

        int lastIndex = (dataLength - patternMask.Length) + 1;

        fixed (byte* patternDataPtr = patternData)
        {
            for (int x = 0; x < lastIndex; x++)
            {
                int patternDataOffset = 0;
                int currentIndex = x;

                int y = 0;
                do
                {
                    // Some performance is saved by making the mask a non-string, since a string comparison is a bit more involved with e.g. null checks.
                    if (patternMask[y] == 0x0)
                    {
                        currentIndex += 1;
                        y++;
                        continue;
                    }

                    // Performance: No need to check if Mask is `x`. The only supported wildcard is '?'.
                    if (data[currentIndex] != patternDataPtr[patternDataOffset])
                        goto loopexit;

                    currentIndex += 1;
                    patternDataOffset += 1;
                    y++;
                }
                while (y < patternMask.Length);

                return new PatternScanResult(x);
                loopexit:;
            }

            return new PatternScanResult(-1);
        }
    }

}
