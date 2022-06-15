using System.Globalization;
using System.Runtime.CompilerServices;
using Reloaded.Memory.Sigscan.Instructions;

namespace Reloaded.Memory.Sigscan.Structs;

/// <summary>
/// Represents the pattern to be searched by the scanner.
/// </summary>
public ref struct CompiledScanPattern
{
    private const string MaskIgnore = "??";

    /// <summary>
    /// The pattern the instruction set was created from.
    /// </summary>
    public readonly string Pattern;

    /// <summary>
    /// The length of the original given pattern.
    /// </summary>
    public readonly int Length;

    /// <summary>
    /// Contains the functions that will be executed in order to validate a given block of memory to equal
    /// the pattern this class was instantiated with.
    /// </summary>
    internal GenericInstruction[] Instructions;

    /// <summary>
    /// Contains the number of instructions in the <see cref="Instructions"/> object.
    /// </summary>
    internal int NumberOfInstructions;

    /// <summary>
    /// Creates a new pattern scan target given a string representation of a pattern.
    /// </summary>
    /// <param name="stringPattern">
    ///     The pattern to look for inside the given region.
    ///     Example: "11 22 33 ?? 55".
    ///     Key: ?? represents a byte that should be ignored, anything else if a hex byte. i.e. 11 represents 0x11, 1F represents 0x1F.
    /// </param>
    public unsafe CompiledScanPattern(string stringPattern)
    {
        Pattern = stringPattern;
        string[] entries = stringPattern.Split(' ');
        Length = entries.Length;

        // Ensure the array allocation size is sufficient such that dereferencing long at any index
        // could not possibly reference unallocated memory.
        byte[] bytesToCompare = new byte[Math.Max(entries.Length, sizeof(nint) * 2)];;
        int arrayIndex = 0;
        foreach (var segment in entries)
        {
            if (!segment.Equals(MaskIgnore, StringComparison.Ordinal))
            {
                bytesToCompare[arrayIndex] = byte.Parse(segment, NumberStyles.HexNumber);
                arrayIndex += 1;
            }
        }

        // Get bytes to make instructions with.
        Instructions  = new GenericInstruction[Length];
        NumberOfInstructions = 0;

        // Optimization for short-medium patterns with masks.
        // Check if our pattern is 1-8 bytes and contains any skips.
        var spanEntries = new Span<string>(entries, 0, entries.Length);
        while (spanEntries.Length > 0)
        {
            int nextSliceLength = Math.Min(sizeof(nint), spanEntries.Length);
            GenerateMaskAndValue(spanEntries.Slice(0, nextSliceLength), out nuint mask, out nuint value);
            AddInstruction(new GenericInstruction(value, mask));
            spanEntries = spanEntries.Slice(nextSliceLength);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void AddInstruction(GenericInstruction instruction)
    {
        Instructions[NumberOfInstructions] = instruction;
        NumberOfInstructions++;
    }

    /// <summary>
    /// Generates a mask given a pattern between size 0-8.
    /// </summary>
    private unsafe void GenerateMaskAndValue(Span<string> entries, out nuint mask, out nuint value)
    {
        mask  = 0;
        value = 0;
        for (int x = 0; x < entries.Length; x++)
        {
            mask  = mask  << 8;
            value = value << 8;
            if (entries[x] != MaskIgnore)
            {
                mask  = mask | 0xFF;
                value = value | byte.Parse(entries[x], NumberStyles.HexNumber);
            }
        }

        // Reverse order of value.
        if (BitConverter.IsLittleEndian)
        {
            Endian.Reverse(ref value);
            Endian.Reverse(ref mask);

            // Trim excess zeroes.
            int extraPadding = sizeof(nint) - entries.Length;
            for (int x = 0; x < extraPadding; x++)
            {
                mask  = mask >> 8;
                value = value >> 8;
            }
        }
    }
}