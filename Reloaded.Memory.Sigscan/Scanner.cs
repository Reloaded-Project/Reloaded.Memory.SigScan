using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Reloaded.Memory.Sigscan.Instructions;
using Reloaded.Memory.Sigscan.Structs;
using Reloaded.Memory.Sources;

#if SIMD_INTRINSICS
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

namespace Reloaded.Memory.Sigscan;

/// <summary>
/// Provides an implementation of a simple signature scanner sitting ontop of Reloaded.Memory.
/// </summary>
public unsafe class Scanner : IDisposable
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

    /// <summary>
    /// Attempts to find a given pattern inside the memory region this class was created with.
    /// This method generates a list of instructions, which more efficiently determine at any array index if pattern is found.
    /// This method generally works better when the expected offset is bigger than 4096.
    /// </summary>
    /// <param name="pattern">
    ///     The pattern to look for inside the given region.
    ///     Example: "11 22 33 ?? 55".
    ///     Key: ?? represents a byte that should be ignored, anything else if a hex byte. i.e. 11 represents 0x11, 1F represents 0x1F
    /// </param>
    /// <returns>A result indicating an offset (if found) of the pattern.</returns>
    public PatternScanResult CompiledFindPattern(string pattern) => CompiledFindPattern(new CompiledScanPattern(pattern));

#if SIMD_INTRINSICS
    /// <summary>
    /// [AVX Variant]
    /// Attempts to find a given pattern inside the memory region this class was created with.
    /// This method generates a list of instructions, which more efficiently determine at any array index if pattern is found.
    /// This method generally works better when the expected offset is bigger than 4096.
    /// </summary>
    /// <param name="pattern">
    ///     The pattern to look for inside the given region.
    ///     Example: "11 22 33 ?? 55".
    ///     Key: ?? represents a byte that should be ignored, anything else if a hex byte. i.e. 11 represents 0x11, 1F represents 0x1F
    /// </param>
    /// <returns>A result indicating an offset (if found) of the pattern.</returns>
    public PatternScanResult CompiledFindPatternAvx2(string pattern) => CompiledFindPatternAvx2(new CompiledScanPattern(pattern));

    /// <summary>
    /// [AVX Variant]
    /// Attempts to find a given pattern inside the memory region this class was created with.
    /// This method generally works better when the expected offset is bigger than 4096.
    /// </summary>
    /// <param name="pattern">
    ///     The compiled pattern to look for inside the given region.
    /// </param>
    /// <param name="startingIndex">The index to start searching at.</param>
    /// <returns>A result indicating an offset (if found) of the pattern.</returns>
    public PatternScanResult CompiledFindPatternAvx2(CompiledScanPattern pattern, int startingIndex = 0)
    {
        int numberOfInstructions = pattern.NumberOfInstructions;
        byte* dataBasePointer = _dataPtr;
        byte* currentDataPointer;
        int lastIndex = _dataLength - Math.Max(pattern.Length, sizeof(Vector256<byte>)) + 1;

        // Note: All of this has to be manually inlined otherwise performance suffers, this is a bit ugly though :/
        fixed (GenericInstruction* instructions = pattern.Instructions)
        {
            var firstInstruction = instructions[0];

            /*
                For non-AVX optimisation details, see the other CompiledFindPattern.

                AVX Strategy: 
            
                    Search for 8 bytes at a time.
                    Bitshift left if not found up until `RegisterBytes - sizeof(nint)`.
                    If matches, investigate the rest.
            */

            // Interpret pattern & mask based off of whether AVX or not.
            int equalsMask = unchecked((int)(0b00000000_00000000_00000000_11111111));
            Vector256<byte> patternAvx;
            Vector256<byte> maskAvx;
            int requiredShiftCount = sizeof(Vector256<byte>) - sizeof(long); // Calculated at JIT time.

            // Note: Below check is taken out by the JIT if compiled in release; no perf impact.
            var valueAsLong = (long)firstInstruction.LongValue;
            var maskAsLong  = (long)firstInstruction.Mask;
            if (sizeof(nint) == 4 && numberOfInstructions > 1)
            {
                valueAsLong |= ((long) instructions[1].LongValue) << 32;
                maskAsLong  |= ((long) instructions[1].Mask) << 32;
            }

            patternAvx = Avx2.BroadcastScalarToVector256(&valueAsLong).AsByte();
            maskAvx    = Avx2.BroadcastScalarToVector256(&maskAsLong).AsByte();

            int x = startingIndex;
            while (x < lastIndex)
            {
                currentDataPointer = dataBasePointer + x;

                var dataAvx    = Avx2.LoadVector256(currentDataPointer);
                int shiftCount = 0;

                for (; shiftCount < requiredShiftCount; shiftCount++)
                {
                    // Mask the data first.
                    var maskedDataAvx = Avx2.And(dataAvx, maskAvx);

                    // And compare against base value.
                    var comparedValue = Avx2.CompareEqual(patternAvx, maskedDataAvx);
                    if ((Avx2.MoveMask(comparedValue) & equalsMask) != equalsMask)
                        goto noMatchFound;

                    // First 8 bytes match using AVX. Compare remaining (if needed).
                    // Fast exit if nothing else to compare.
                    if ((sizeof(nint) == 8 && numberOfInstructions <= 1) || (sizeof(nint) == 4 && numberOfInstructions <= 2))
                        return new PatternScanResult(x + shiftCount);
                    
                    /* When NumberOfInstructions > 1 */
                    currentDataPointer += sizeof(ulong) + shiftCount; // Not an error; we're doing 8 byte comparisons in AVX.
                    int y = sizeof(nint) == 8 ? 1 : 2;
                    do
                    {
                        var compareValue = *(nuint*)currentDataPointer & instructions[y].Mask;
                        if (compareValue != instructions[y].LongValue)
                            goto noMatchFound;

                        currentDataPointer += sizeof(nuint);
                        y++;
                    }
                    while (y < numberOfInstructions);

                    return new PatternScanResult(x + shiftCount);

                    noMatchFound:;
                    // Shift Left 1
                    // Reference: https://stackoverflow.com/a/25264853/11106111
                    // _mm256_alignr_epi8(_mm256_permute2x128_si256(A, A, _MM_SHUFFLE(2, 0, 0, 1)), A, N)
                    // I'm not experienced with SIMD so this is mostly black magic to me.
                    // Also weirdly, I needed shift right instead of shift left.

                    // _MM_SHUFFLE(2, 0, 0, 1)
                    const byte YXXZ = (2 << 6) | (0 << 4) | (0 << 2) | 1;
                    dataAvx = Avx2.AlignRight(Avx2.Permute2x128(dataAvx, dataAvx, YXXZ), dataAvx, 1);
                }
                
                // Go to next vector read location.
                x += sizeof(Vector256<byte>) - sizeof(long);
            }

            // Done.
            // Check last few bytes in cases pattern was not found and long overflows into possibly unallocated memory.
            return SimpleFindPattern(pattern.Pattern, lastIndex);

            // PS. This function is a prime example why the `goto` statement is frowned upon.
            // I have to use it here for performance though.
        }
    }
#endif

    /// <summary>
    /// Attempts to find a given pattern inside the memory region this class was created with.
    /// This method generally works better when the expected offset is bigger than 4096.
    /// </summary>
    /// <param name="pattern">
    ///     The compiled pattern to look for inside the given region.
    /// </param>
    /// <param name="startingIndex">The index to start searching at.</param>
    /// <returns>A result indicating an offset (if found) of the pattern.</returns>
    public PatternScanResult CompiledFindPattern(CompiledScanPattern pattern, int startingIndex = 0)
    {
        int numberOfInstructions = pattern.NumberOfInstructions;
        byte* dataBasePointer = _dataPtr;
        byte* currentDataPointer;
        int lastIndex = _dataLength - Math.Max(pattern.Length, sizeof(nint)) + 1;

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
            return SimpleFindPattern(pattern.Pattern, lastIndex);

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
    public PatternScanResult SimpleFindPattern(string pattern, int startingIndex = 0)
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

    // This code added to correctly implement the disposable pattern.
    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}