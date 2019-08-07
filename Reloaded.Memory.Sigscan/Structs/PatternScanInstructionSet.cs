using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
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

        private unsafe void Initialize(string stringPattern)
        {
            string[] entries = stringPattern.Split(' ');
            Length = entries.Length;

            // Ensure the array allocation size is sufficient such that dereferencing long at any index
            // could not possibly reference unallocated memory.
            byte[] bytesToCompare = new byte[Math.Max(entries.Length, sizeof(long) * 2)];;
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
            var bytesSpan = new Span<byte>(bytesToCompare, 0, bytesToCompare.Length);

            // Optimization for short-medium patterns with masks.
            // Check if our pattern is 1-8 bytes and contains any skips.
            if (entries.Length <= sizeof(long) && 
                CountTokensUntilSkip(entries, 0) != entries.Length)
            {
                GenerateMaskAndValue(entries, out ulong mask, out ulong value);
                AddInstruction(new GenericInstruction(Instruction.Check, value, mask, 0));
            }
            else
            {
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
                        ulong mask       = GenerateMask(x);
                        var valueToCheck = *(ulong*)Unsafe.AsPointer(ref bytesSpan[0]);
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
        private ulong GenerateMask(int numberOfBytes)
        {
            ulong mask = 0;
            for (int x = 0; x < numberOfBytes; x++)
            {
                mask = mask << 8;
                mask = mask | 0xFF;
            }

            return mask;
        }

        /// <summary>
        /// Generates a mask given a pattern between size 0-8.
        /// </summary>
        private void GenerateMaskAndValue(string[] entries, out ulong mask, out ulong value)
        {
            mask  = 0;
            value = 0;
            for (int x = 0; x < entries.Length; x++)
            {
                mask  = mask  << 8;
                value = value << 8;
                if (entries[x] != MaskIgnore)
                {
                    mask  = mask | 0xFF;
                    value = value | byte.Parse(entries[x], NumberStyles.HexNumber);
                }
            }

            // Reverse order of value.
            if (BitConverter.IsLittleEndian)
            {
                Endian.Reverse(ref value, out value);
                Endian.Reverse(ref mask, out mask);

                // Trim excess zeroes.
                int extraPadding = sizeof(long) - entries.Length;
                for (int x = 0; x < extraPadding; x++)
                {
                    mask  = mask >> 8;
                    value = value >> 8;
                }
            }
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
