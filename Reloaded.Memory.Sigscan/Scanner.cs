using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Reloaded.Memory.Sigscan.Instructions;
using Reloaded.Memory.Sigscan.Structs;
using Reloaded.Memory.Sources;

namespace Reloaded.Memory.Sigscan
{
    /// <summary>
    /// Provides an implementation of a simple signature scanner sitting ontop of Reloaded.Memory.
    /// </summary>
    public unsafe class Scanner
    {
        /// <summary>
        /// The region of data to be scanned for signatures.
        /// </summary>
        public byte[] Data => _data.ToArray();
        private Memory<byte>  _data;

        private GCHandle _gcHandle;
        private byte*     _dataPtr;

        /// <summary>
        /// Creates a signature scanner given the data in which patterns are to be found.
        /// </summary>
        /// <param name="data">The data to look for signatures inside.</param>
        public Scanner(byte[] data)
        {
            _data = data;
            _gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            _dataPtr  = (byte*) _gcHandle.AddrOfPinnedObject();
        }

        /// <summary>
        /// Creates a signature scanner given a process and a module (EXE/DLL)
        /// from which the signatures are to be found.
        /// </summary>
        /// <param name="process">The process from which</param>
        /// <param name="module">An individual module of the given process, which</param>
        public Scanner(Process process, ProcessModule module)
        {
            var externalProcess = new ExternalMemory(process);
            externalProcess.ReadRaw(module.BaseAddress, out var data, module.ModuleMemorySize);

            _data = data;
            _gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            _dataPtr = (byte*)_gcHandle.AddrOfPinnedObject();
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
        public PatternScanResult CompiledFindPattern(string pattern)
        {
            var instructionSet = PatternScanInstructionSet.FromStringPattern(pattern);
            int numberOfInstructions = instructionSet.NumberOfInstructions;
            int dataLength     = _data.Length;

            byte* dataBasePointer = _dataPtr;
            byte* currentDataPointer;
            int lastIndex = (dataLength - instructionSet.Length) + 1;

            // Note: All of this has to be manually inlined otherwise performance suffers, this is a bit ugly though :/
            fixed (GenericInstruction* instructions = instructionSet.Instructions)
            {
                for (int x = 0; x < lastIndex; x++)
                {
                    // Do while saves an initial bounds check (y < numberOfInstructions).
                    // This can be beneficial when there are not many instructions.
                    currentDataPointer = dataBasePointer + x;
                    int y = 0;
                    do
                    {
                        if (instructions[y].Instruction == Instruction.Check)
                        {
                            long currentValue = *(long*) currentDataPointer;
                            currentValue     &= instructions[y].Mask;

                            if (currentValue != instructions[y].LongValue)
                                goto loopExit;

                            currentDataPointer += instructions[y].Skip;
                        }
                        else
                        {
                            currentDataPointer += instructions[y].Skip;
                        }

                        y++;
                    }
                    while (y < numberOfInstructions);

                    return new PatternScanResult(x);
                    loopExit:;
                }
            }
            

            return new PatternScanResult(-1);
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
        /// <returns>A result indicating an offset (if found) of the pattern.</returns>
        public PatternScanResult SimpleFindPattern(string pattern)
        {
            var target      = new SimplePatternScanData(pattern);
            var patternData = target.Bytes;
            var patternMask = target.Mask;

            int lastIndex   = (_data.Span.Length - patternMask.Length) + 1;

            fixed (byte* patternDataPtr = patternData)
            {
                for (int x = 0; x < lastIndex; x++)
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
}
