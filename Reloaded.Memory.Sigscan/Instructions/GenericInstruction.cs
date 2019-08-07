using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Reloaded.Memory.Sigscan.Instructions
{
    public struct GenericInstruction
    {
        public ulong LongValue;
        public ulong Mask;

        public GenericInstruction(ulong longValue, ulong mask)
        {
            LongValue   = longValue;
            Mask        = mask;
        }
    }
}
