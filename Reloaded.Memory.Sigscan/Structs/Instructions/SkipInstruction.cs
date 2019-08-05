using System;
using System.Collections.Generic;
using System.Text;

namespace Reloaded.Memory.Sigscan.Structs.Instructions
{
    internal unsafe class SkipInstruction : IInstruction
    {
        private int _skipAmount;
        public SkipInstruction(int skipAmount)
        {
            _skipAmount = skipAmount;
        }

        public bool Execute(ref byte* dataPtr)
        {
            dataPtr += _skipAmount;
            return true;
        }
    }
}
