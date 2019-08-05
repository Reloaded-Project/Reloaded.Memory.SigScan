using System;
using System.Collections.Generic;
using System.Text;

namespace Reloaded.Memory.Sigscan.Structs.Instructions
{
    public unsafe interface IInstruction
    {
        /// <param name="dataPtr">Pointer to the data to check against for equality.</param>
        /// <returns>True if pattern checking should continue, else false.</returns>
        bool Execute(ref byte* dataPtr);
    }
}
