using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Reloaded.Memory.Sigscan;
using Reloaded.Memory.Sigscan.Definitions;
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
    private List<string> _mainModulePatterns = new();
    private List<Action<PatternScanResult>> _mainModuleActions = new();
    private List<ArbitraryScanActionTuple> _arbitraryScanActions = new();

    public StartupScanner(IModLoader modLoader, ILogger logger)
    {
        _logger = logger;
        modLoader.OnModLoaderInitialized += OnLoaderInitialized;
    }

    /// <summary/>
    private void OnLoaderInitialized()
    {
        RunMainModuleScans();
        RunArbitraryScans();
    }

    private void RunArbitraryScans()
    {
        if (_arbitraryScanActions.Count <= 0)
            return;

        var results = new PatternScanResult[_arbitraryScanActions.Count];
        Parallel.ForEach(Partitioner.Create(_arbitraryScanActions, true), (item, _, index) =>
        {
            results[index] = item.scanForResult();
        });

        for (int x = 0; x < results.Length; x++)
            _arbitraryScanActions[x].callback(results[x]);
    }

    private void RunMainModuleScans()
    {
        // Scan registered patterns.
        var curProcScanner = new Scanner(Process.GetCurrentProcess());

        // Find all the patterns, and run the results.
        var results = curProcScanner.FindPatternsCached(_mainModulePatterns);
        for (int x = 0; x < results.Length; x++)
            _mainModuleActions[x](results[x]);

        // Cleanup
        _mainModulePatterns.Clear();
        _mainModuleActions.Clear();
    }

    /// <inheritdoc />
    public void AddMainModuleScan(string pattern, Action<PatternScanResult> action)
    {
        _mainModulePatterns.Add(pattern);
        _mainModuleActions.Add(action);
    }

    /// <inheritdoc />
    public void AddArbitraryScan(IScanner scanner, string pattern, Action<PatternScanResult> action)
    {
        _arbitraryScanActions.Add(new ArbitraryScanActionTuple()
        {
            scanForResult = () => scanner.FindPattern(pattern),
            callback = action
        });
    }

    struct ArbitraryScanActionTuple
    {
        internal Func<PatternScanResult> scanForResult;
        internal Action<PatternScanResult> callback;
    }
}