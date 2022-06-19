using Reloaded.Memory.Sigscan.Definitions.Structs;
using Reloaded.Memory.Sigscan.Structs;
using System.Runtime.CompilerServices;

namespace Reloaded.Memory.Sigscan;

public unsafe partial class Scanner
{
    /// <summary>
    /// Attempts to find a given pattern inside the memory region this class was created with.
    /// This method generally works better than a simple byte search when the expected offset is bigger than 4096.
    /// </summary>
    /// <param name="data">Address of the data to be scanned.</param>
    /// <param name="dataLength">Length of the data to be scanned.</param>
    /// <param name="pattern">
    ///     The compiled pattern to look for inside the given region.
    /// </param>
    /// <returns>A result indicating an offset (if found) of the pattern.</returns>
#if NET5_0_OR_GREATER
    [SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#endif
    public static PatternScanResult FindPatternCompiled(byte* data, int dataLength, CompiledScanPattern pattern)
    {
        const int numberOfUnrolls = 8;

        int numberOfInstructions = pattern.NumberOfInstructions;
        int lastIndex = dataLength - Math.Max(pattern.Length, sizeof(nint)) - numberOfUnrolls;

        if (lastIndex < 0)
            return FindPatternSimple(data, dataLength, new SimplePatternScanData(pattern.Pattern));

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

            var dataCurPointer = data;
            var dataMaxPointer = data + lastIndex;
            while (dataCurPointer < dataMaxPointer)
            {
                if ((*(nuint*)dataCurPointer & firstInstruction.Mask) != firstInstruction.LongValue)
                {
                    if ((*(nuint*)(dataCurPointer + 1) & firstInstruction.Mask) != firstInstruction.LongValue)
                    {
                        if ((*(nuint*)(dataCurPointer + 2) & firstInstruction.Mask) != firstInstruction.LongValue)
                        {
                            if ((*(nuint*)(dataCurPointer + 3) & firstInstruction.Mask) != firstInstruction.LongValue)
                            {
                                if ((*(nuint*)(dataCurPointer + 4) & firstInstruction.Mask) != firstInstruction.LongValue)
                                {
                                    if ((*(nuint*)(dataCurPointer + 5) & firstInstruction.Mask) != firstInstruction.LongValue)
                                    {
                                        if ((*(nuint*)(dataCurPointer + 6) & firstInstruction.Mask) != firstInstruction.LongValue)
                                        {
                                            if ((*(nuint*)(dataCurPointer + 7) & firstInstruction.Mask) != firstInstruction.LongValue)
                                            {
                                                dataCurPointer += 8;
                                                goto end;
                                            }
                                            else
                                            {
                                                dataCurPointer += 7;
                                            }
                                        }
                                        else
                                        {
                                            dataCurPointer += 6;
                                        }
                                    }
                                    else
                                    {
                                        dataCurPointer += 5;
                                    }
                                }
                                else
                                {
                                    dataCurPointer += 4;
                                }
                            }
                            else
                            {
                                dataCurPointer += 3;
                            }
                        }
                        else
                        {
                            dataCurPointer += 2;
                        }
                    }
                    else
                    {
                        dataCurPointer += 1;
                    }
                }

                if (numberOfInstructions <= 1 || TestRemainingMasks(numberOfInstructions, dataCurPointer, instructions))
                    return new PatternScanResult((int)(dataCurPointer - data));
                
                dataCurPointer += 1;
                end:;
            }

            // Check last few bytes in cases pattern was not found and long overflows into possibly unallocated memory.
            return FindPatternSimple(data + lastIndex, dataLength - lastIndex, pattern.Pattern).AddOffset(lastIndex);

            // PS. This function is a prime example why the `goto` statement is frowned upon.
            // I have to use it here for performance though.
        }
    }
    
#if NET5_0_OR_GREATER
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#endif
    private static bool TestRemainingMasks(int numberOfInstructions, byte* currentDataPointer, GenericInstruction* instructions)
    {
        /* When NumberOfInstructions > 1 */
        currentDataPointer += sizeof(nuint);

        int y = 1;
        do
        {
            var compareValue = *(nuint*)currentDataPointer & instructions[y].Mask;
            if (compareValue != instructions[y].LongValue)
                return false;

            currentDataPointer += sizeof(nuint);
            y++;
        } 
        while (y < numberOfInstructions);

        return true;
    }
}