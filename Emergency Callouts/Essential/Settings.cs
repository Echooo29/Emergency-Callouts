using EmergencyCallouts.Essential;
using Rage;
using System.Windows.Forms;

namespace EmergencyCallouts
{
    internal static class Settings
    {
        // Callout Toggling
        internal static bool PublicIntoxication = true;
        internal static bool Trespassing = true;
        internal static bool DomesticViolence = true;
        internal static bool Burglary = true;
        internal static bool SuspiciousActivity = true;

        // Callout Measurements
        internal static int SearchAreaSize = 60;
        internal static int CalloutDistance = 1000;

        // Notifications
        internal static string Callsign = "1-LINCOLN-18";
        internal static string SubtitleColor = "Blue";

        // Keys
        internal static Keys ToggleMenu = Keys.Delete;
        internal static Keys Talk = Keys.Y;

        internal static void Initialize()
        {
            Game.LogTrivial("[Emergency Callouts]: Loading settings.");

            // Create the INI file
            var iniFile = new InitializationFile(Project.SettingsPath);
            iniFile.Create();

            // Callout Toggling
            PublicIntoxication = iniFile.ReadBoolean("Callout Toggling", "PublicIntoxication", PublicIntoxication);
            Trespassing = iniFile.ReadBoolean("Callout Toggling", "Trespassing", Trespassing);
            DomesticViolence = iniFile.ReadBoolean("Callout Toggling", "DomesticViolence", DomesticViolence);
            Burglary = iniFile.ReadBoolean("Callout Toggling", "Burglary", Burglary);
            SuspiciousActivity = iniFile.ReadBoolean("Callout Toggling", "SuspiciousActivity", SuspiciousActivity);

            // Callout Measurements
            SearchAreaSize = iniFile.ReadInt32("Callout Measurements", "SearchAreaSize", SearchAreaSize);
            CalloutDistance = iniFile.ReadInt32("Callout Measurements", "CalloutDistance", CalloutDistance);

            // Notifications
            Callsign = iniFile.ReadString("Notifications", "Callsign", Callsign);
            SubtitleColor = iniFile.ReadString("Notifications", "SubtitleColor", SubtitleColor).Substring(0, 1).ToLower();

            // Keybindings
            ToggleMenu = iniFile.ReadEnum("Keybindings", "ToggleMenu", Keys.Delete);
            Talk = iniFile.ReadEnum("Keybindings", "Talk", Keys.Y);

            Game.LogTrivial("[Emergency Callouts]: Loaded settings.");
        }
    }
}