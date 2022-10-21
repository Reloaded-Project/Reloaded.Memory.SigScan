using System.Numerics;
using System.Runtime.CompilerServices;
using Reloaded.Memory.Sigscan.Definitions.Structs;
using Reloaded.Memory.Sigscan.Structs;

#if SIMD_INTRINSICS
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Reloaded.Memory.Sigscan;

/// <summary>
/// Modified version of: Pattern scan implementation 'LazySIMD' - by uberhalit
/// https://github.com/uberhalit
///
/// Changes made:
///     - Handles patterns smaller than register size at end of data.
///     - Handles 1 byte pattern with a fallback.
///     - Handles patterns starting with a null.
/// 
/// Uses SIMD instructions on SSE2-supporting processors, the longer the pattern the more efficient this should get.
/// Requires RyuJIT compiler for hardware acceleration which **should** be enabled by default on newer VS versions.
/// Ideally a pattern would be a multiple of (xmm0 register size) / 8 so all available space gets used in calculations.
///
/// Licensed under the MIT License: https://github.com/uberhalit/PatternScanBench/blob/master/LICENSE
/// </summary>
public unsafe partial class Scanner
{
    /// <summary>
    /// Length of an SSE2 register in bytes.
    /// </summary>
    private const int SseRegisterLength = 16;

    /// <summary>
    /// Returns address of pattern using 'LazySIMD' implementation by uberhalit. Can match 0.
    /// </summary>
    /// <param name="data">Address of the data to be scanned.</param>
    /// <param name="dataLength">Length of the data to be scanned.</param>
    /// <param name="pattern">
    ///     The pattern to look for inside the given region.
    ///     Example: "11 22 33 ?? 55".
    ///     Key: ?? represents a byte that should be ignored, anything else if a hex byte. i.e. 11 represents 0x11, 1F represents 0x1F
    /// </param>
    /// <returns>-1 if pattern is not found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static PatternScanResult FindPatternSse2(byte* data, int dataLength, string pattern)
    {
        var patternData = new SimdPatternScanData(pattern);
        if (patternData.Bytes.Length == 1) // For single byte search, fall back.
            return FindPatternSimple(data, dataLength, pattern);

        var matchTable       = BuildMatchIndexes(patternData);
        var patternVectors   = PadPatternToVector128Sse(patternData);
        int matchTableLength = matchTable.Length;

        var firstByteVec  = Vector128.Create(patternData.Bytes[patternData.LeadingIgnoreCount]);
        int searchLength = dataLength - Math.Max(patternData.Bytes.Length, SseRegisterLength);

        int leadingIgnoreCount = patternData.LeadingIgnoreCount;
        ref var pVec = ref patternVectors[0];
        var matchTablePtr = (short*)Unsafe.AsPointer(ref matchTable.GetPinnableReference());

        int vectorLength = patternVectors.Length;
        int simdJump = SseRegisterLength - 1;
        ref var pFirstByteVec = ref firstByteVec;
        var dataPtr    = data;
        var dataMaxPtr = dataPtr + searchLength;
        for (; dataPtr < dataMaxPtr; dataPtr++)
        {
            // Problem: If pattern starts with unknown, will never match.
            var rhs = Sse2.LoadVector128(dataPtr);
            var equal = Sse2.CompareEqual(pFirstByteVec, rhs);
            int findFirstByte = Sse2.MoveMask(equal);

            if (findFirstByte == 0)
            {
                dataPtr += simdJump;
                continue;
            }

            // Shift up until first byte found.
            dataPtr += (BitOperations.TrailingZeroCount((uint)findFirstByte) - leadingIgnoreCount);

            int iMatchTableIndex = 0;
            for (int i = 0; i < vectorLength; i++)
            {
                int registerByteOffset = i * SseRegisterLength;
                var nextByte = dataPtr + registerByteOffset + 1;
                var rhsNo2   = Sse2.LoadVector128(nextByte);
                var curPatternVector = Unsafe.Add(ref pVec, i);

                int compareResult = Sse2.MoveMask(Sse2.CompareEqual(curPatternVector, rhsNo2));

                for (; iMatchTableIndex < matchTableLength; iMatchTableIndex++)
                {
                    int matchIndex = matchTablePtr[iMatchTableIndex] - registerByteOffset;
                    if (matchIndex < SseRegisterLength)
                    {
                        if (((compareResult >> matchIndex) & 1) != 1)
                            goto notfound;

                        continue;
                    }

                    break;
                }
            }

            return new PatternScanResult((int)(dataPtr - data));

            notfound:;
        }

        // Check last few bytes in cases pattern was not found and long overflows into possibly unallocated memory.
        var position = (int)(dataPtr - data);
        return FindPatternSimple(data + position, dataLength - position, pattern).AddOffset(position);
    }

    /// <summary>
    /// Builds a table that indicates which positions in pattern should be matched,
    /// the first match is skipped and all indexes are shifted to the left by 1.
    /// </summary>
    /// <param name="scanPattern">Data of the pattern to be scanned.</param>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static Span<ushort> BuildMatchIndexes(in SimdPatternScanData scanPattern)
    {
        // ORIGINAL CODE
        int maskLength  = scanPattern.Mask.Length;
        ushort[] fullMatchTable = new ushort[maskLength];

        int matchCount = 0;
        for (ushort x = 1; x < maskLength; x++)
        {
            if (scanPattern.Mask[x] != 1) 
                continue;

            fullMatchTable[matchCount] = (ushort)(x - 1);
            matchCount++;
        }
        
        return new Span<ushort>(fullMatchTable).Slice(0, matchCount);
    }

    /// <summary>
    /// Generates byte-Vectors that are right-padded with 0 from a pattern. The first byte is skipped.
    /// </summary>
    /// <param name="cbPattern">The pattern in question.</param>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static Vector128<byte>[] PadPatternToVector128Sse(in SimdPatternScanData cbPattern)
    {
        int patternLen     = cbPattern.Mask.Length;
        int vectorCount    = (int) Math.Ceiling((patternLen - 1) / (float)SseRegisterLength);
        var patternVectors = new Vector128<byte>[vectorCount];

        ref byte pPattern = ref cbPattern.Bytes[1];
        patternLen--;
        for (int i = 0; i < vectorCount; i++)
        {
            if (i < vectorCount - 1)
            {
                patternVectors[i] = Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref pPattern, i * SseRegisterLength));
            }
            else
            {
                int o = i * SseRegisterLength;
                patternVectors[i] = Vector128.Create(
                    Unsafe.Add(ref pPattern, o + 0),
                    o + 1 < patternLen ? Unsafe.Add(ref pPattern, o + 1) : (byte)0,
                    o + 2 < patternLen ? Unsafe.Add(ref pPattern, o + 2) : (byte)0,
                    o + 3 < patternLen ? Unsafe.Add(ref pPattern, o + 3) : (byte)0,
                    o + 4 < patternLen ? Unsafe.Add(ref pPattern, o + 4) : (byte)0,
                    o + 5 < patternLen ? Unsafe.Add(ref pPattern, o + 5) : (byte)0,
                    o + 6 < patternLen ? Unsafe.Add(ref pPattern, o + 6) : (byte)0,
                    o + 7 < patternLen ? Unsafe.Add(ref pPattern, o + 7) : (byte)0,
                    o + 8 < patternLen ? Unsafe.Add(ref pPattern, o + 8) : (byte)0,
                    o + 9 < patternLen ? Unsafe.Add(ref pPattern, o + 9) : (byte)0,
                    o + 10 < patternLen ? Unsafe.Add(ref pPattern, o + 10) : (byte)0,
                    o + 11 < patternLen ? Unsafe.Add(ref pPattern, o + 11) : (byte)0,
                    o + 12 < patternLen ? Unsafe.Add(ref pPattern, o + 12) : (byte)0,
                    o + 13 < patternLen ? Unsafe.Add(ref pPattern, o + 13) : (byte)0,
                    o + 14 < patternLen ? Unsafe.Add(ref pPattern, o + 14) : (byte)0,
                    o + 15 < patternLen ? Unsafe.Add(ref pPattern, o + 15) : (byte)0
                );
            }
        }
        return patternVectors;
    }
}
#endif