using System.Runtime.CompilerServices;
using Reloaded.Memory.Sigscan.Instructions;
using Reloaded.Memory.Sigscan.Structs;

namespace Reloaded.Memory.Sigscan;

public unsafe partial class Scanner
{
    /// <summary>
    /// Attempts to find a given pattern inside the memory region this class was created with.
    /// This method generally works better when the expected offset is bigger than 4096.
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
        int numberOfInstructions = pattern.NumberOfInstructions;
        byte* dataBasePointer = data;
        byte* currentDataPointer;
        int lastIndex = dataLength - Math.Max(pattern.Length, sizeof(nint));

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

            int x = 0;
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
            return FindPatternSimple(data + lastIndex, dataLength - lastIndex, pattern.Pattern).AddOffset(lastIndex);

            // PS. This function is a prime example why the `goto` statement is frowned upon.
            // I have to use it here for performance though.
        }
    }
}