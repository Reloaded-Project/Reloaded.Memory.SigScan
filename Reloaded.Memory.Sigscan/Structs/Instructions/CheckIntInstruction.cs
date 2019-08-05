using System;
using System.Collections.Generic;
using System.Text;

namespace Reloaded.Memory.Sigscan.Structs.Instructions
{
    public unsafe class CheckIntInstruction : IInstruction
    {
        private int _value;
        public CheckIntInstruction(int value)
        {
            _value = value;
        }

        public bool Execute(ref byte* dataPtr)
        {
            if (*(int*)dataPtr != _value)
                return false;

            dataPtr += sizeof(int);
            return true;
        }
    }
}
