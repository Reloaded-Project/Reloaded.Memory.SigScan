using System;
using System.Collections.Generic;
using System.Text;

namespace Reloaded.Memory.Sigscan.Structs.Instructions
{
    public unsafe class CheckLongInstruction : IInstruction
    {
        private long _value;
        public CheckLongInstruction(long value)
        {
            _value = value;
        }

        public bool Execute(ref byte* dataPtr)
        {
            if (*(long*)dataPtr != _value)
                return false;

            dataPtr += sizeof(long);
            return true;
        }
    }
}
