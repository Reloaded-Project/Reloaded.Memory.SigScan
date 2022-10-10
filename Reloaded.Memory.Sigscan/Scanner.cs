using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Reloaded.Memory.Sigscan.Definitions;
using Reloaded.Memory.Sigscan.Definitions.Structs;
using Reloaded.Memory.Sigscan.Structs;
using Reloaded.Memory.Sources;

#if SIMD_INTRINSICS
using System.Runtime.Intrinsics.X86;
#endif

#if NET5_0_OR_GREATER
    [module: SkipLocalsInit]
#endif

namespace Reloaded.Memory.Sigscan;

/// <summary>
/// Provides an implementation of a simple signature scanner sitting ontop of Reloaded.Memory.
/// </summary>
public unsafe partial class Scanner : IScanner, IDisposable
{
    private static int _currentProcessId = Process.GetCurrentProcess().Id;

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
    /// Creates a signature scanner given a process.
    /// The scanner will be initialised to scan the main module of the process.
    /// </summary>
    /// <param name="process">The process from which to scan patterns in. (Not Null)</param>
    public Scanner(Process process) : this(process, process.MainModule) { }

    /// <summary>
    /// Creates a signature scanner given a process and a module (EXE/DLL)
    /// from which the signatures are to be found.
    /// </summary>
    /// <param name="process">The process from which to scan patterns in. (Not Null)</param>
    /// <param name="module">An individual module of the given process, which denotes the start and end of memory region scanned.</param>
    public Scanner(Process process, ProcessModule module)
    {
        // Optimization
        if (process.Id == _currentProcessId)
        {
            _dataPtr    = (byte*) module.BaseAddress;
            _dataLength = module.ModuleMemorySize;
        }
        else
        {
            var externalProcess = new ExternalMemory(process);
            externalProcess.ReadRaw((nuint)(nint)module.BaseAddress, out var data, module.ModuleMemorySize);

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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public PatternScanResult FindPattern(string pattern, int offset)
    {
#if SIMD_INTRINSICS
        if (Avx2.IsSupported)
            return FindPatternAvx2(_dataPtr + offset, _dataLength - offset, pattern).AddOffset(offset);

        if (Sse2.IsSupported)
            return FindPatternSse2(_dataPtr + offset, _dataLength - offset, pattern).AddOffset(offset);
#endif

        return FindPatternCompiled(_dataPtr + offset, _dataLength - offset, pattern).AddOffset(offset);
    }

    /// <inheritdoc/>
    public PatternScanResult[] FindPatterns(IReadOnlyList<string> patterns, bool loadBalance = false)
    {
        var results = new PatternScanResult[patterns.Count];
        if (patterns.Count == 0)
            return results;

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

    /// <inheritdoc/>
    public PatternScanResult[] FindPatternsCached(IReadOnlyList<string> patterns, bool loadBalance = false)
    {
        var results = new PatternScanResult[patterns.Count];
        if (patterns.Count == 0)
            return results;

        var completedPatternCache = new ConcurrentDictionary<string, PatternScanResult>(StringComparer.OrdinalIgnoreCase);

        if (loadBalance)
        {
            Parallel.ForEach(Partitioner.Create(patterns.ToArray(), true), (item, _, index) =>
            {
                if (completedPatternCache.TryGetValue(item, out var value))
                    results[index] = value;
                else
                    AddResult(item, (int)index);
            });
        }
        else
        {
            Parallel.ForEach(Partitioner.Create(0, patterns.Count), tuple =>
            {
                for (int x = tuple.Item1; x < tuple.Item2; x++)
                {
                    var pattern = patterns[x];
                    if (completedPatternCache.TryGetValue(pattern, out var value))
                        results[x] = value;
                    else
                        AddResult(pattern, (int)x);
                }
            });
        }

        return results;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AddResult(string pattern, int index)
        {
            var scanResult = FindPattern(pattern);
            results[index] = scanResult;
            completedPatternCache[pattern] = scanResult;
        }
    }

#if SIMD_INTRINSICS
    /// <inheritdoc/>
    public PatternScanResult FindPattern_Avx2(string pattern) => FindPatternAvx2(_dataPtr, _dataLength, pattern);

    /// <inheritdoc/>
    public PatternScanResult FindPattern_Sse2(string pattern) => FindPatternSse2(_dataPtr, _dataLength, pattern);
#endif

    /// <inheritdoc/>
    public PatternScanResult FindPattern_Compiled(string pattern) => FindPatternCompiled(_dataPtr, _dataLength, pattern);

    /// <inheritdoc/>
    public PatternScanResult FindPattern_Simple(string pattern) => FindPatternSimple(_dataPtr, _dataLength, pattern);
}