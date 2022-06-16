using System;
using System.Collections.Generic;
using Reloaded.Memory.Sigscan.Definitions.Structs;

namespace Reloaded.Memory.Sigscan.Definitions;

/// <summary>
/// Represents an individual scanner that can be used to scan for byte patterns.
/// </summary>
public interface IScanner : IDisposable
{
    /// <summary>
    /// Attempts to find a given pattern inside the memory region this class was created with.
    /// The method used depends on the available hardware; will use vectorized instructions if available.
    /// </summary>
    /// <param name="pattern">
    ///     The pattern to look for inside the given region.
    ///     Example: "11 22 33 ?? 55".
    ///     Key: ?? represents a byte that should be ignored, anything else if a hex byte. i.e. 11 represents 0x11, 1F represents 0x1F
    /// </param>
    /// <returns>A result indicating an offset (if found) of the pattern.</returns>
    PatternScanResult FindPattern(string pattern);

    /// <summary>
    /// Finds multiple patterns within a given scan range, in multithreaded fashion.
    /// </summary>
    /// <param name="patterns">The patterns to scan.</param>
    /// <param name="loadBalance">True to use load balancing. Optimal with many patterns (64+) of variable length.</param>
    /// <returns>Results of the scan.</returns>
    PatternScanResult[] FindPatterns(IReadOnlyList<string> patterns, bool loadBalance = false);

    /// <summary>
    /// Finds multiple patterns within a given scan range, in multithreaded fashion.
    /// This implementation guards against scanning duplicates, at negligible speed expense.
    /// </summary>
    /// <param name="patterns">The patterns to scan.</param>
    /// <param name="loadBalance">True to use load balancing. Optimal with many patterns (64+) of variable length.</param>
    /// <returns>Results of the scan.</returns>
    PatternScanResult[] FindPatternsCached(IReadOnlyList<string> patterns, bool loadBalance = false);

#if SIMD_INTRINSICS
    /// <summary>
    /// Attempts to find a given pattern inside the memory region this class was created with.
    /// This method is based on a modified version of 'LazySIMD' - by uberhalit.
    /// </summary>
    /// <param name="pattern">
    ///     The pattern to look for inside the given region.
    ///     Example: "11 22 33 ?? 55".
    ///     Key: ?? represents a byte that should be ignored, anything else if a hex byte. i.e. 11 represents 0x11, 1F represents 0x1F
    /// </param>
    /// <returns>A result indicating an offset (if found) of the pattern.</returns>
    PatternScanResult FindPattern_Avx2(string pattern);

    /// <summary>
    /// [SSE2 Variant]
    /// Attempts to find a given pattern inside the memory region this class was created with.
    /// This method is based on a modified version of 'LazySIMD' - by uberhalit.
    /// </summary>
    /// <param name="pattern">
    ///     The pattern to look for inside the given region.
    ///     Example: "11 22 33 ?? 55".
    ///     Key: ?? represents a byte that should be ignored, anything else if a hex byte. i.e. 11 represents 0x11, 1F represents 0x1F
    /// </param>
    /// <returns>A result indicating an offset (if found) of the pattern.</returns>
    PatternScanResult FindPattern_Sse2(string pattern);
#endif

    /// <summary>
    /// Attempts to find a given pattern inside the memory region this class was created with.
    /// This method generates a list of instructions, which specify a set of bytes and mask to check against.
    /// It is fairly performant on 64-bit systems but not much faster than the simple implementation on 32-bit systems.
    /// </summary>
    /// <param name="pattern">
    ///     The pattern to look for inside the given region.
    ///     Example: "11 22 33 ?? 55".
    ///     Key: ?? represents a byte that should be ignored, anything else if a hex byte. i.e. 11 represents 0x11, 1F represents 0x1F
    /// </param>
    /// <returns>A result indicating an offset (if found) of the pattern.</returns>
    PatternScanResult FindPattern_Compiled(string pattern);

    /// <summary>
    /// Attempts to find a given pattern inside the memory region this class was created with.
    /// This method uses the simple search, which simply iterates over all bytes, reading max 1 byte at once.
    /// This method generally works better when the expected offset is smaller than 4096.
    /// </summary>
    /// <param name="pattern">
    ///     The pattern to look for inside the given region.
    ///     Example: "11 22 33 ?? 55".
    ///     Key: ?? represents a byte that should be ignored, anything else if a hex byte. i.e. 11 represents 0x11, 1F represents 0x1F
    /// </param>
    /// <returns>A result indicating an offset (if found) of the pattern.</returns>
    PatternScanResult FindPattern_Simple(string pattern);
}
