using System;
using System.Linq;

namespace EmergencyCallouts.Other
{
    /// <summary>
    /// Checks to see if external plugins are available.
    /// </summary>
    internal static class PluginChecker
    {
        private static readonly Func<string, Version, bool> IsVersionLoaded = (plugin, version) =>
            LSPD_First_Response.Mod.API.Functions.GetAllUserPlugins().Any(x => x.GetName().Name.Equals(plugin) && x.GetName().Version.CompareTo(version) >= 0);

        public static readonly bool IsCalloutInterfaceRunning = IsVersionLoaded("CalloutInterface", new Version("1.2"));
    }
}