using System;
using System.Collections.Generic;
using System.Text;

namespace Reloaded.Memory.Sigscan.Structs.Instructions
{
    public unsafe class CheckByteInstruction : IInstruction
    {
        private byte _value;
        public CheckByteInstruction(byte value)
        {
            _value = value;
        }

        public bool Execute(ref byte* dataPtr)
        {
            if (*dataPtr != _value)
                return false;

            dataPtr += sizeof(byte);
            return true;
        }
    }
}
