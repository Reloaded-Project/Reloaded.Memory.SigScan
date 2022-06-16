using System;
using System.Collections.Generic;
using System.Diagnostics;
using Reloaded.Memory.Sigscan;
using Reloaded.Memory.Sigscan.Definitions.Structs;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

namespace Reloaded.Memory.SigScan.ReloadedII;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public class StartupScanner : IStartupScanner
{
    private ILogger _logger;
    private List<string> _startupPatterns = new();
    private List<Action<PatternScanResult>> _patternActions = new();

    public StartupScanner(IModLoader modLoader, ILogger logger)
    {
        _logger = logger;
        modLoader.OnModLoaderInitialized += OnLoaderInitialized;
    }

    /// <summary/>
    private void OnLoaderInitialized()
    {
        // Scan registered patterns.
        var curProcScanner = new Scanner(Process.GetCurrentProcess());

        // Find all the patterns, and run the results.
        var results = curProcScanner.FindPatternsCached(_startupPatterns);
        for (int x = 0; x < results.Length; x++)
            _patternActions[x](results[x]);

        // Cleanup
        _startupPatterns.Clear();
        _patternActions.Clear();
    }

    /// <inheritdoc />
    public void AddMainModuleScan(string pattern, Action<PatternScanResult> action)
    {
        _startupPatterns.Add(pattern);
        _patternActions.Add(action);
    }
}