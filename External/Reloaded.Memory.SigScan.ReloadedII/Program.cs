using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using System;
using Reloaded.Memory.Sigscan;
using Reloaded.Memory.Sigscan.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

namespace Reloaded.Memory.SigScan.ReloadedII;

public class Program : IMod, IExports
{
    /// <summary>
    /// Used for writing text to the Reloaded log.
    /// </summary>
    private ILogger _logger = null!;

    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    private IModLoader _modLoader = null!;

    /// <summary>
    /// Entry point for your mod.
    /// </summary>
    public void StartEx(IModLoaderV1 loaderApi, IModConfigV1 modConfig)
    {
        // For more information about this template, please see
        // https://reloaded-project.github.io/Reloaded-II/ModTemplate/
        _modLoader = (IModLoader)loaderApi;
        _logger = (ILogger)_modLoader.GetLogger();

        // Please put your mod code in the class below,
        // use this class for only interfacing with mod loader.
        _modLoader.AddOrReplaceController<IScannerFactory>(this, new ScannerFactory());
        _modLoader.AddOrReplaceController<IStartupScanner>(this, new StartupScanner(_modLoader, _logger));
    }

    /* Mod loader actions. */
    public void Suspend() { }
    public void Resume() { }
    public void Unload() { }

    /*  If CanSuspend == false, suspend and resume button are disabled in Launcher and Suspend()/Resume() will never be called.
        If CanUnload == false, unload button is disabled in Launcher and Unload() will never be called.
    */
    public bool CanUnload() => false;
    public bool CanSuspend() => false;

    /* Automatically called by the mod loader when the mod is about to be unloaded. */
    public Action Disposing { get; } = null!;

    // Exports
    public Type[] GetTypes() => new[]
    {
        typeof(IScanner), // Includes Reloaded.Memory.Sigscan.Definitions
        typeof(IStartupScanner) // Includes Reloaded.Memory.SigScan.ReloadedII.Interfaces
    };
}