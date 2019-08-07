using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Reloaded.Memory.Sigscan.Instructions;

namespace Reloaded.Memory.Sigscan.Structs
{
    /// <summary>
    /// [Internal & Test Use]
    /// Represents the pattern to be searched by the scanner.
    /// </summary>
    public ref struct PatternScanInstructionSet
    {
        private const  string MaskIgnore      = "??";

        /// <summary>
        /// The length of the original given pattern.
        /// </summary>
        public int Length;

        /// <summary>
        /// Contains the functions that will be executed in order to validate a given block of memory to equal
        /// the pattern this class was instantiated with.
        /// </summary>
        internal GenericInstruction[] Instructions;

        /// <summary>
        /// Contains the number of instructions in the <see cref="Instructions"/> object.
        /// </summary>
        internal int NumberOfInstructions;

        /// <summary>
        /// Creates a new pattern scan target given a string representation of a pattern.
        /// </summary>
        /// <param name="stringPattern">
        ///     The pattern to look for inside the given region.
        ///     Example: "11 22 33 ?? 55".
        ///     Key: ?? represents a byte that should be ignored, anything else if a hex byte. i.e. 11 represents 0x11, 1F represents 0x1F.
        /// </param>
        public static PatternScanInstructionSet FromStringPattern(string stringPattern)
        {
            var instructionSet = new PatternScanInstructionSet();
            instructionSet.Initialize(stringPattern);
            return instructionSet;
        }

        private void Initialize(string stringPattern)
        {
            string[] entries = stringPattern.Split(' ');
            Length = entries.Length;

            byte[] bytesToCompare = new byte[entries.Length];
            int arrayIndex = 0;
            foreach (var segment in entries)
            {
                if (!segment.Equals(MaskIgnore, StringComparison.Ordinal))
                {
                    bytesToCompare[arrayIndex] = byte.Parse(segment, NumberStyles.HexNumber);
                    arrayIndex += 1;
                }
            }

            // Get bytes to make instructions with.
            Instructions  = new GenericInstruction[Length];
            var bytesSpan = new Span<byte>(bytesToCompare, 0, arrayIndex);

            int tokensProcessed = 0;
            while (tokensProcessed < entries.Length)
            {
                int bytes = CountTokensUntilSkip(entries, tokensProcessed);

                if (bytes == 0)
                {
                    // No bytes, encode skip.
                    int skip = CountTokensUntilMatch(entries, tokensProcessed);
                    EncodeSkip(skip);
                    tokensProcessed += skip;
                }
                else
                {
                    // Bytes, now find skip after and encode check!
                    int skip = CountTokensUntilMatch(entries, tokensProcessed + bytes);
                    EncodeCheck(bytes, skip, ref bytesSpan);
                    tokensProcessed += (bytes + skip);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void AddInstruction(GenericInstruction instruction)
        {
            Instructions[NumberOfInstructions] = instruction;
            NumberOfInstructions++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void EncodeSkip(int skip)
        {
            // Add skip instruction.
            AddInstruction(new GenericInstruction(Instruction.Skip, 0, 0x0, skip));
        }

        private unsafe void EncodeCheck(int bytes, int skip, ref Span<byte> bytesSpan)
        {
            // Code now moved to Scanner, inlined as far as it goes.
            // Delegates, or any kind of function calls are too slow.
            // Generics cannot even check for Equality without `Equals`

            // This code can be simplified with a for loop
            // Note: Longs removed due to bias towards short/byte making them slower.
            // Encoding longs as int is faster.

            startOfLoop:
            while (bytes > 0)
            {
                for (int x = sizeof(long); x > 0; x--)
                {
                    if (bytes >= x)
                    {
                        long mask        = GenerateMask(x);
                        var valueToCheck = *(long*)Unsafe.AsPointer(ref bytesSpan[0]);
                        valueToCheck     = valueToCheck & mask;

                        if ((bytes - x) > 0)
                            AddInstruction(new GenericInstruction(Instruction.Check, valueToCheck, mask, x));
                        else
                            AddInstruction(new GenericInstruction(Instruction.Check, valueToCheck, mask, skip + x));

                        bytesSpan = bytesSpan.Slice(x);
                        bytes -= x;
                        goto startOfLoop;
                    }
                }
            }
        }

        /// <summary>
        /// Generates a mask that preserves a given amount of bytes.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private long GenerateMask(int bytes)
        {
            long mask = 0;
            for (int x = 0; x < bytes; x++)
            {
                mask = mask << 8;
                mask = mask | 0xFF;
            }

            return mask;
        }

        /* Retrieves the amount of bytes until the next wildcard character starting with a given entry. */

        // The code below has been inlined because it is frequently called to help aid performance.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CountTokensUntilSkip(string[] entries, int startingTokenEntry)
        {
            int tokens = 0;
            for (int x = startingTokenEntry; x < entries.Length; x++)
            {
                if (entries[x] == MaskIgnore)
                    break;

                tokens += 1;
            }

            return tokens;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CountTokensUntilMatch(string[] entries, int startingTokenEntry)
        {
            int tokens = 0;
            for (int x = startingTokenEntry; x < entries.Length; x++)
            {
                if (entries[x] != MaskIgnore)
                    break;

                tokens += 1;
            }

            return tokens;
        }
    }
}
