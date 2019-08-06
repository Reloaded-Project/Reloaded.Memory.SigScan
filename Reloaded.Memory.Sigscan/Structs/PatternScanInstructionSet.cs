using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class PatternScanInstructionSet
    {
        private static List<byte> _bytes       = new List<byte>(1024);
        private static object _buildLock       = new object();

        /// <summary>
        /// The sequence of bytes that will be compared during the scan.
        /// </summary>
        public byte[] Bytes;

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
        /// Creates a new pattern scan target given a string representation of a pattern.
        /// </summary>
        /// <param name="stringPattern">
        ///     The pattern to look for inside the given region.
        ///     Example: "11 22 33 ?? 55".
        ///     Key: ?? represents a byte that should be ignored, anything else if a hex byte. i.e. 11 represents 0x11, 1F represents 0x1F.
        /// </param>
        public PatternScanInstructionSet(string stringPattern)
        {
            string[] entries = stringPattern.Split(' ');
            Length = entries.Length;

            // Get all bytes.
            ExtractBytesToMatch(entries);

            // Make Instructions.
            MakeInstructions(entries);
        }

        private void ExtractBytesToMatch(string[] entries)
        {
            lock (_buildLock)
            {
                _bytes.Clear();
                foreach (var segment in entries)
                    if (segment != "??")
                        _bytes.Add(Convert.ToByte(segment, 16));

                Bytes = _bytes.ToArray();
            }
        }

        private void MakeInstructions(string[] skipsAndBytes)
        {
            var instructions = new List<GenericInstruction>();
            var bytesSpan = new Span<byte>(Bytes);

            int tokensProcessed = 0;
            while (tokensProcessed < skipsAndBytes.Length)
            {
                int bytes = CountTokensUntilSkip(skipsAndBytes, tokensProcessed);

                if (bytes == 0)
                {
                    // No bytes, encode skip.
                    int skip = CountTokensUntilMatch(skipsAndBytes, tokensProcessed);
                    EncodeSkip(instructions, skip);
                    tokensProcessed += skip;
                }
                else
                {
                    // Bytes, now find skip after and encode check!
                    int skip = CountTokensUntilMatch(skipsAndBytes, tokensProcessed + bytes);
                    EncodeCheck(instructions, bytes, skip, ref bytesSpan);
                    tokensProcessed += (bytes + skip);
                }
            }

            Instructions = instructions.ToArray();
        }

        private unsafe void EncodeSkip(List<GenericInstruction> instructions, int skip)
        {
            // Add skip instruction.
            instructions.Add(new GenericInstruction(Instruction.Skip, 0, skip));
        }

        private unsafe void EncodeCheck(List<GenericInstruction> instructions, int bytes, int skip, ref Span<byte> bytesSpan)
        {
            // Code now moved to Scanner, inlined as far as it goes.
            // Delegates, or any kind of function calls are too slow.
            // Generics cannot even check for Equality without `Equals`
            while (bytes > 0)
            {
                // Note: Longs disabled due to bias towards short/byte making them slower.
                // Encoding longs as int is faster.
                
                /*
                if (bytes >= sizeof(long) && IntPtr.Size == 8)
                {
                    var valueToCheck = *(long*)Unsafe.AsPointer(ref bytesSpan[0]);

                    if (bytes - sizeof(long) > 0)
                        instructions.Add(new GenericInstruction(Instruction.CheckLong, valueToCheck, 0));
                    else
                        instructions.Add(new GenericInstruction(Instruction.CheckLong, valueToCheck, skip));

                    bytesSpan = bytesSpan.Slice(sizeof(long));
                    bytes -= sizeof(long);
                }
                */
                
                if (bytes >= sizeof(int))
                {
                    var valueToCheck = *(int*)Unsafe.AsPointer(ref bytesSpan[0]);

                    if (bytes - sizeof(int) > 0)
                        instructions.Add(new GenericInstruction(Instruction.CheckInt, valueToCheck, 0));
                    else
                        instructions.Add(new GenericInstruction(Instruction.CheckInt, valueToCheck, skip));

                    bytesSpan = bytesSpan.Slice(sizeof(int));
                    bytes -= sizeof(int);
                }

                else if (bytes >= sizeof(short))
                {
                    var valueToCheck = *(short*)Unsafe.AsPointer(ref bytesSpan[0]);

                    if (bytes - sizeof(short) > 0)
                        instructions.Add(new GenericInstruction(Instruction.CheckShort, valueToCheck, 0));
                    else
                        instructions.Add(new GenericInstruction(Instruction.CheckShort, valueToCheck, skip));

                    bytesSpan = bytesSpan.Slice(sizeof(short));
                    bytes -= sizeof(short);
                }

                else if (bytes >= sizeof(byte))
                {
                    var valueToCheck = *(byte*)Unsafe.AsPointer(ref bytesSpan[0]);

                    if (bytes - sizeof(byte) > 0)
                        instructions.Add(new GenericInstruction(Instruction.CheckByte, valueToCheck, 0));
                    else
                        instructions.Add(new GenericInstruction(Instruction.CheckByte, valueToCheck, skip));

                    bytesSpan = bytesSpan.Slice(sizeof(byte));
                    bytes -= sizeof(byte);
                }
            }
        }

        /* Retrieves the amount of bytes until the next wildcard character starting with a given entry. */
        private int CountTokensUntilSkip(string[] entries, int startingTokenEntry)
        {
            return CountTokensWhile(entries, s => s != "??", startingTokenEntry);
        }

        private int CountTokensUntilMatch(string[] entries, int startingTokenEntry)
        {
            return CountTokensWhile(entries, s => s == "??", startingTokenEntry);
        }

        /// <summary>
        /// Retrieves the amount of tokens until a given token is encountered.
        /// </summary>
        private int CountTokensWhile(string[] entries, Func<string, bool> compareToken, int startingTokenEntry)
        {
            int tokens = 0;
            for (int x = startingTokenEntry; x < entries.Length; x++)
            {
                if (! compareToken(entries[x]))
                    break;

                tokens += 1;
            }

            return tokens;
        }
    }
}
