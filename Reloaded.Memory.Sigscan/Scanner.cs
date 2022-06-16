using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Reloaded.Memory.Sigscan.Structs;
using Reloaded.Memory.Sources;

#if SIMD_INTRINSICS
using System.Runtime.Intrinsics.X86;
#endif

namespace Reloaded.Memory.Sigscan;

/// <summary>
/// Provides an implementation of a simple signature scanner sitting ontop of Reloaded.Memory.
/// </summary>
public unsafe partial class Scanner : IDisposable
{
    private static Process _currentProcess = Process.GetCurrentProcess();

    private bool _disposedValue;
    private GCHandle? _gcHandle;
    private byte*     _dataPtr;
    private int    _dataLength;

    /// <summary>
    /// Creates a signature scanner given the data in which patterns are to be found.
    /// </summary>
    /// <param name="data">The data to look for signatures inside.</param>
    public Scanner(byte[] data)
    {
        _gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
        _dataPtr  = (byte*)_gcHandle.Value.AddrOfPinnedObject();
        _dataLength = data.Length;
    }

    /// <summary>
    /// Creates a signature scanner given a process and a module (EXE/DLL)
    /// from which the signatures are to be found.
    /// </summary>
    /// <param name="process">The process from which to scan patterns in. (Not Null)</param>
    /// <param name="module">An individual module of the given process, which denotes the start and end of memory region scanned.</param>
    public Scanner(Process process, ProcessModule module)
    {
        // Optimization
        if (process.Id == _currentProcess.Id)
        {
            _dataPtr    = (byte*) module.BaseAddress;
            _dataLength = module.ModuleMemorySize;
        }
        else
        {
            var externalProcess = new ExternalMemory(process);
            externalProcess.ReadRaw(module.BaseAddress, out var data, module.ModuleMemorySize);

            _gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            _dataPtr = (byte*)_gcHandle.Value.AddrOfPinnedObject();
            _dataLength = data.Length;
        }
    }

    /// <summary>
    /// Creates a signature scanner given the data in which patterns are to be found.
    /// </summary>
    /// <param name="data">The data to look for signatures inside.</param>
    /// <param name="length">The length of the data.</param>
    public Scanner(byte* data, int length)
    {
        _dataPtr = data;
        _dataLength = length;
    }

    /// <inheritdoc />
    ~Scanner()
    {
        Dispose(false);
    }

    // This code added to correctly implement the disposable pattern.
    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary/>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            // Note: We consider a handle to managed memory as an
            // unmanaged resource since it needs to be explicitly freed.
            _gcHandle?.Free();
            _disposedValue = true;
        }
    }

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
    public PatternScanResult FindPattern(string pattern)
    {
#if SIMD_INTRINSICS
        if (Avx2.IsSupported)
            return FindPatternAvx2(_dataPtr, _dataLength, pattern);

        if (Sse2.IsSupported)
            return FindPatternSse2(_dataPtr, _dataLength, pattern);
#endif

        return FindPatternCompiled(_dataPtr, _dataLength, pattern);
    }

    /// <summary>
    /// Finds multiple patterns within a given scan range, in multithreaded fashion.
    /// </summary>
    /// <param name="patterns">The patterns to scan.</param>
    /// <param name="loadBalance">True to use load balancing. Optimal with many patterns (64+) of variable length.</param>
    /// <returns>Results of the scan.</returns>
    public PatternScanResult[] FindPatterns(IReadOnlyList<string> patterns, bool loadBalance = false)
    {
        var results     = new PatternScanResult[patterns.Count];
        if (loadBalance)
        {
            Parallel.ForEach(Partitioner.Create(patterns.ToArray(), true), (item, _, index) =>
            {
                results[index] = FindPattern(item);
            });
        }
        else
        {
            Parallel.ForEach(Partitioner.Create(0, patterns.Count), tuple =>
            {
                for (int x = tuple.Item1; x < tuple.Item2; x++)
                    results[x] = FindPattern(patterns[x]);
            });
        }

        return results;
    }

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
    public PatternScanResult FindPattern_Avx2(string pattern) => FindPatternAvx2(_dataPtr, _dataLength, pattern);

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
    public PatternScanResult FindPattern_Sse2(string pattern) => FindPatternSse2(_dataPtr, _dataLength, pattern);
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
    public PatternScanResult FindPattern_Compiled(string pattern) => FindPatternCompiled(_dataPtr, _dataLength, pattern);
    
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
    public PatternScanResult FindPattern_Simple(string pattern) => FindPatternSimple(_dataPtr, _dataLength, pattern);
}