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
        public int   IntValue ;

        public GenericInstruction(Instruction instruction, long longValue, int skip)
        {
            Instruction = instruction;
            LongValue   = longValue;
            IntValue    = (int) longValue;
            Skip        = skip;
        }
    }
}
