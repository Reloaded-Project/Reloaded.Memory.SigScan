using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Reloaded.Memory.Sigscan.Instructions
{
    public struct GenericInstruction
    {
        public Instruction Instruction;
        public int   Skip;
        public long  LongValue;
        public long  Mask;

        public GenericInstruction(Instruction instruction, long longValue, long mask, int skip)
        {
            Instruction = instruction;
            LongValue   = longValue;
            Mask        = mask;
            Skip        = skip;
        }
    }
}
