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
        /// This method generally works better when the expected offset is bigger than 4K.
        /// </summary>
        /// <param name="pattern">
        ///     The pattern to look for inside the given region.
        ///     Example: "11 22 33 ?? 55".
        ///     Key: ?? represents a byte that should be ignored, anything else if a hex byte. i.e. 11 represents 0x11, 1F represents 0x1F
        /// </param>
        /// <returns>A result indicating an offset (if found) of the pattern.</returns>
        public unsafe PatternScanResult CompiledFindPattern(string pattern)
        {
            var instructionSet = new PatternScanInstructionSet(pattern);

            // Get instruction set and copy instructions to stack.
            int numberOfInstructions = instructionSet.Instructions.Length;
            GenericInstruction* instructions = stackalloc GenericInstruction[numberOfInstructions];
            for (int x = 0; x < instructionSet.Instructions.Length; x++)
                instructions[x] = instructionSet.Instructions[x];

            int dataLength     = _data.Length;

            byte* dataBasePointer = _dataPtr;
            byte* currentDataPointer;
            bool  found;
            int lastIndex = (dataLength - instructionSet.Length) + 1;

            for (int x = 0; x < lastIndex; x++)
            {
                currentDataPointer = dataBasePointer + x;
                found              = true;
                for (int y = 0; y < numberOfInstructions; y++)
                {
                    switch (instructions[y].Instruction)
                    {
                        case Instruction.Skip:
                            currentDataPointer += instructions[y].Skip;
                            break;
                        case Instruction.CheckByte:
                            if (*currentDataPointer != instructions[y].IntValue)
                            {
                                found = false;
                                goto loopExit;
                            }

                            currentDataPointer += sizeof(byte);
                            break;
                        case Instruction.CheckShort:
                            if (*(short*)currentDataPointer != instructions[y].IntValue)
                            {
                                found = false;
                                goto loopExit;
                            }

                            currentDataPointer += sizeof(short);
                            break;
                        case Instruction.CheckInt:
                            if (*(int*)currentDataPointer != instructions[y].IntValue)
                            {
                                found = false;
                                goto loopExit;
                            }

                            currentDataPointer += sizeof(int);
                            break;

                        case Instruction.CheckLong:
                            if (*(long*) currentDataPointer != instructions[y].LongValue)
                            {
                                found = false;
                                goto loopExit;
                            }

                            currentDataPointer += sizeof(long);
                            break;
                    }
                }

                loopExit:
                if (found)
                    return new PatternScanResult(x);
            }

            return new PatternScanResult(-1);
        }

        /// <summary>
        /// Attempts to find a given pattern inside the memory region this class was created with.
        /// This method uses the simple search, which simply iterates over all bytes, reading max 1 byte at once.
        /// This method generally works better when the expected offset is smaller than 4K.
        /// </summary>
        /// <param name="pattern">
        ///     The pattern to look for inside the given region.
        ///     Example: "11 22 33 ?? 55".
        ///     Key: ?? represents a byte that should be ignored, anything else if a hex byte. i.e. 11 represents 0x11, 1F represents 0x1F
        /// </param>
        /// <returns>A result indicating an offset (if found) of the pattern.</returns>
        public PatternScanResult SimpleFindPattern(string pattern)
        {
            var target = new SimplePatternScanData(pattern);
            var dataSpan = _data.Span;

            for (int x = 0; x < _data.Length; x++)
            {
                if (CheckPatternAtOffset(ref target, ref dataSpan, x))
                    return new PatternScanResult(x);
            }

            return new PatternScanResult(-1);
        }

        /* Checks */
        private bool CheckPatternAtOffset(ref SimplePatternScanData pattern, ref Span<byte> data, int dataOffset)
        {
            // This code is IO bound on my machine. (4790k, 2133MHz CL9 RAM)
            // As such using unsafe code yielded (no bounds checks) yielded no speed improvements.
            // Keeping safe code.
            var patternData = pattern.Bytes;
            var patternMask = pattern.Mask;
            int patternDataOffset = 0;

            if (dataOffset + patternMask.Length > data.Length)
                return false;

            for (int x = 0; x < patternMask.Length; x++)
            {
                // Some performance is saved by making the mask a non-string, since a string comparison is a bit more involved with e.g. null checks.
                if (patternMask[x] == 0x0)
                {
                    dataOffset += 1;
                    continue;
                }

                // Performance: No need to check if Mask is `x`. The only supported wildcard is '?'.
                if (data[dataOffset] != patternData[patternDataOffset])
                    return false;

                dataOffset += 1;
                patternDataOffset += 1;
            }

            return true;
        }
    }
}
