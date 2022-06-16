using System.Diagnostics;
using Reloaded.Memory.Sigscan.Definitions;

namespace Reloaded.Memory.Sigscan;

/// <inheritdoc />
public class ScannerFactory : IScannerFactory
{
    /// <inheritdoc />
    public IScanner CreateScanner(byte[] data) => new Scanner(data);

    /// <inheritdoc />
    public IScanner CreateScanner(Process process) => new Scanner(process);

    /// <inheritdoc />
    public IScanner CreateScanner(Process process, ProcessModule module) => new Scanner(process, module);

    /// <inheritdoc />
    public unsafe IScanner CreateScanner(byte* data, int length) => new Scanner(data, length);
}