using System.Diagnostics;

namespace Reloaded.Memory.Sigscan.Definitions;

/// <summary>
/// Represents a factory for creating scanner instances.
/// </summary>
public unsafe interface IScannerFactory
{
    /// <summary>
    /// Creates a signature scanner given the data in which patterns are to be found.
    /// </summary>
    /// <param name="data">The data to look for signatures inside.</param>
    public IScanner CreateScanner(byte[] data);

    /// <summary>
    /// Creates a signature scanner given a process.
    /// The scanner will be initialised to scan the main module of the process.
    /// </summary>
    /// <param name="process">The process from which to scan patterns in. (Not Null)</param>
    public IScanner CreateScanner(Process process);

    /// <summary>
    /// Creates a signature scanner given a process and a module (EXE/DLL)
    /// from which the signatures are to be found.
    /// </summary>
    /// <param name="process">The process from which to scan patterns in. (Not Null)</param>
    /// <param name="module">An individual module of the given process, which denotes the start and end of memory region scanned.</param>
    public IScanner CreateScanner(Process process, ProcessModule module);

    /// <summary>
    /// Creates a signature scanner given the data in which patterns are to be found.
    /// </summary>
    /// <param name="data">The data to look for signatures inside.</param>
    /// <param name="length">The length of the data.</param>
    public IScanner CreateScanner(byte* data, int length);
}