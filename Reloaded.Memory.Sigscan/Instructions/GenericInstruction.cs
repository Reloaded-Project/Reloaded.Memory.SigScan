using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Reloaded.Memory.Sigscan.Instructions;

/// <summary>
/// Represents a generic instruction to match an 8 byte masked value at a given address.
/// </summary>
public struct GenericInstruction
{
    /// <summary>
    /// The value to match.
    /// </summary>
    public nuint LongValue;

    /// <summary>
    /// The mask to apply before comparing with the value.
    /// </summary>
    public nuint Mask;

    /// <summary>
    /// Creates an instruction to match an 8 byte masked value at a given address.
    /// </summary>
    /// <param name="longValue">The value to be matched.</param>
    /// <param name="mask">The mask to match.</param>
    public GenericInstruction(nuint longValue, nuint mask)
    {
        LongValue   = longValue;
        Mask        = mask;
    }
}