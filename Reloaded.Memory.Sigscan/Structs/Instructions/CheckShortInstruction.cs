using System;
using System.Collections.Generic;
using System.Text;

namespace Reloaded.Memory.Sigscan.Structs.Instructions
{
    public unsafe class CheckShortInstruction : IInstruction
    {
        private short _value;
        public CheckShortInstruction(short value)
        {
            _value = value;
        }

        public bool Execute(ref byte* dataPtr)
        {
            if (*(short*)dataPtr != _value)
                return false;

            dataPtr += sizeof(short);
            return true;
        }
    }
}
