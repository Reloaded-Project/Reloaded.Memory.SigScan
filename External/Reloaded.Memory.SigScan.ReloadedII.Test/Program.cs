using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using System;
using Reloaded.Memory.Sigscan.Definitions.Structs;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

namespace Reloaded.Memory.SigScan.ReloadedII.Test
{
    public class Program : IMod
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

            // Get Controller
            _modLoader.GetController<IStartupScanner>().TryGetTarget(out var startupScanner);
            startupScanner.AddMainModuleScan("C3", OnMainModuleScan);
        }

        private void OnMainModuleScan(PatternScanResult obj)
        {
            _logger.WriteLine($"Found Ret at: {obj.Offset}");
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
    }
}