using System.Diagnostics;
using System.Runtime.InteropServices;
using Reloaded.Memory.Sigscan.Instructions;
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

        return FindPattern_Compiled(pattern);
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
    public PatternScanResult FindPattern_Compiled(string pattern) => FindPattern_Compiled(new CompiledScanPattern(pattern));

    /// <summary>
    /// Attempts to find a given pattern inside the memory region this class was created with.
    /// This method generally works better when the expected offset is bigger than 4096.
    /// </summary>
    /// <param name="pattern">
    ///     The compiled pattern to look for inside the given region.
    /// </param>
    /// <param name="startingIndex">The index to start searching at.</param>
    /// <returns>A result indicating an offset (if found) of the pattern.</returns>
    public PatternScanResult FindPattern_Compiled(CompiledScanPattern pattern, int startingIndex = 0)
    {
        int numberOfInstructions = pattern.NumberOfInstructions;
        byte* dataBasePointer = _dataPtr;
        byte* currentDataPointer;
        int lastIndex = _dataLength - Math.Max(pattern.Length, sizeof(nint));

        // Note: All of this has to be manually inlined otherwise performance suffers, this is a bit ugly though :/
        fixed (GenericInstruction* instructions = pattern.Instructions)
        {
            var firstInstruction = instructions[0];

            /*
                There is an optimization going on in here apart from manual inlining which is why
                this function is a tiny mess of more goto statements than it seems necessary.

                Basically, it is considerably faster to reference a variable on the stack, than it is on the heap.
            
                This is because the compiler can address the stack bound variable relative to the current stack pointer,
                as opposing to having to dereference a pointer and then take an offset from the result address.
                
                This ends up being considerably faster, which is important in a scenario where we are entirely CPU bound.
            */

            int x = startingIndex;
            while (x < lastIndex)
            {
                currentDataPointer = dataBasePointer + x;
                var compareValue = *(nuint*)currentDataPointer & firstInstruction.Mask;
                if (compareValue != firstInstruction.LongValue)
                    goto singleInstructionLoopExit;

                if (numberOfInstructions <= 1)
                    return new PatternScanResult(x);

                /* When NumberOfInstructions > 1 */
                currentDataPointer += sizeof(nuint);
                int y = 1;
                do
                {
                    compareValue = *(nuint*)currentDataPointer & instructions[y].Mask;
                    if (compareValue != instructions[y].LongValue)
                        goto singleInstructionLoopExit;

                    currentDataPointer += sizeof(nuint);
                    y++;
                }
                while (y < numberOfInstructions);

                return new PatternScanResult(x);

                singleInstructionLoopExit:;
                x++;
            }
                

            // Check last few bytes in cases pattern was not found and long overflows into possibly unallocated memory.
            return FindPattern_Simple(pattern.Pattern, lastIndex);

            // PS. This function is a prime example why the `goto` statement is frowned upon.
            // I have to use it here for performance though.
        }
    }

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
    /// <param name="startingIndex">The index to start searching at.</param>
    /// <returns>A result indicating an offset (if found) of the pattern.</returns>
    public PatternScanResult FindPattern_Simple(string pattern, int startingIndex = 0)
    {
        var target      = new SimplePatternScanData(pattern);
        var patternData = target.Bytes;
        var patternMask = target.Mask;

        int lastIndex   = (_dataLength - patternMask.Length) + 1;

        fixed (byte* patternDataPtr = patternData)
        {
            for (int x = startingIndex; x < lastIndex; x++)
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
                    if (_dataPtr[currentIndex] != patternDataPtr[patternDataOffset])
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