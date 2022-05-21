using Rage;

namespace EmergencyCallouts.Essential
{
    internal static class Settings
    {
        // Backend
        internal static bool EarlyAccess = false;

        internal static void Initialize()
        {
            var iniFile = new InitializationFile(@"plugins\LSPDFR\Emergency Callouts.ini");
            iniFile.Create();
        }
    }
}