using System;
using Reloaded.Memory.Sigscan.Definitions;
using Reloaded.Memory.Sigscan.Definitions.Structs;

namespace Reloaded.Memory.SigScan.ReloadedII.Interfaces;

/// <summary>
/// Provides the interface to the sigscan mod's Startup Scanner, a signature scanner that runs
/// all scans it receives in parallel once the mod finishes intialising.<br/><br/>
///
/// The idea is people submit all their signature scans here, and once all mods complete initialising,
/// all the signature scans are performed in parallel to save on startup time.<br/><br/>
///
/// Scans submitted to this class are performed in the order they are submitted.<br/>
/// There is no need to worry about conflicts/race conditions.
/// </summary>
public interface IStartupScanner
{
    /// <summary>
    /// Adds a pattern to be scanned in parallel once mod loader finishes loading.<br/>
    /// Scan range is the binary contents of the currently executing EXE.
    /// </summary>
    /// <param name="pattern">The pattern to signature scan for.</param>
    /// <param name="action">The function to execute with the result of the function.</param>
    public void AddMainModuleScan(string pattern, Action<PatternScanResult> action);

    /// <summary>
    /// Adds a pattern to be scanned in parallel with other arbitrary patterns.  
    /// Note: This pattern is not scanned in parallel in a separate queue from main module scans.  
    /// </summary>
    /// <param name="scanner">Scanner preconfigured to scan a specific memory region.</param>
    /// <param name="pattern">The pattern to scan using the scanner.</param>
    /// <param name="action">The function to execute with the result of the function.</param>
    public void AddArbitraryScan(IScanner scanner, string pattern, Action<PatternScanResult> action);
}