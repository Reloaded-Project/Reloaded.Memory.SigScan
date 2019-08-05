using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Reloaded.Memory.Sigscan.Structs.Instructions;

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
        internal IInstruction[] Instructions;

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
            var instructions = new List<IInstruction>();
            var bytesSpan = new Span<byte>(Bytes);

            int tokensProcessed = 0;
            while (tokensProcessed < skipsAndBytes.Length)
            {
                int bytes = CountTokensUntilSkip(skipsAndBytes, tokensProcessed);

                if (bytes == 0)
                {
                    // No bytes, encode skip.
                    int skip = CountTokensUntilMatch(skipsAndBytes, tokensProcessed + bytes);
                    EncodeSkip(instructions, skip);
                    tokensProcessed += skip;
                }
                else
                {
                    // Bytes, encode check!
                    EncodeCheck(instructions, bytes, ref bytesSpan);
                    tokensProcessed += bytes;
                }
            }

            Instructions = instructions.ToArray();
        }

        private void EncodeSkip(List<IInstruction> instructions, int skip)
        {
            // Add skip instruction.
            instructions.Add(new SkipInstruction(skip));
        }

        private unsafe void EncodeCheck(List<IInstruction> instructions, int bytes, ref Span<byte> bytesSpan)
        {
            // Each instruction in pseudo-code is
            // if (value != span[0]) return false;
            // else { slice and return true; }

            while (bytes > 0)
            {
                // No generic type constraint exists here that can allow me to perform an efficient equality check with
                // primitives. Have to write all variations of code out myself.
                if (bytes >= sizeof(long) && IntPtr.Size == 8)
                {
                    var valueToCheck = *(long*)Unsafe.AsPointer(ref bytesSpan[0]);
                    instructions.Add(new CheckLongInstruction(valueToCheck));

                    bytesSpan = bytesSpan.Slice(sizeof(long));
                    bytes -= sizeof(long);
                }
                else if (bytes >= sizeof(int))
                {
                    var valueToCheck = *(int*)Unsafe.AsPointer(ref bytesSpan[0]);
                    instructions.Add(new CheckIntInstruction(valueToCheck));

                    bytesSpan = bytesSpan.Slice(sizeof(int));
                    bytes -= sizeof(int);
                }
                else if (bytes >= sizeof(short))
                {
                    var valueToCheck = *(short*)Unsafe.AsPointer(ref bytesSpan[0]);
                    instructions.Add(new CheckShortInstruction(valueToCheck));

                    bytesSpan = bytesSpan.Slice(sizeof(short));
                    bytes -= sizeof(short);
                }
                else if (bytes >= sizeof(byte))
                {
                    var valueToCheck = *(byte*)Unsafe.AsPointer(ref bytesSpan[0]);
                    instructions.Add(new CheckByteInstruction(valueToCheck));

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
