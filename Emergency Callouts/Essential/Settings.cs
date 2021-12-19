using EmergencyCallouts.Essential;
using Rage;
using System.Windows.Forms;

namespace EmergencyCallouts
{
    internal static class Settings
    {
        // Callouts
        internal static bool PublicIntoxication = true;
        internal static bool Trespassing = true;
        internal static bool DomesticViolence = true;
        internal static bool Burglary = true;
        internal static bool SuspiciousActivity = true;

        // Measurements
        internal static int SearchAreaSize = 60;
        internal static int CalloutDistance = 1000;

        // Customization
        internal static string Callsign = "1-LINCOLN-18";
        internal static string SubtitleColor = "Yellow";

        // Keys
        internal static Keys TalkKey = Keys.Y;
        internal static Keys EndCalloutKey = Keys.End;

        internal static void Initialize()
        {
            Game.LogTrivial("[Emergency Callouts]: Loading settings.");

            // Create the INI file
            var iniFile = new InitializationFile(Project.SettingsPath);
            iniFile.Create();

            // Callout Toggling
            PublicIntoxication = iniFile.ReadBoolean("Callouts", "PublicIntoxication", PublicIntoxication);
            Trespassing = iniFile.ReadBoolean("Callouts", "Trespassing", Trespassing);
            DomesticViolence = iniFile.ReadBoolean("Callouts", "DomesticViolence", DomesticViolence);
            Burglary = iniFile.ReadBoolean("Callouts", "Burglary", Burglary);
            SuspiciousActivity = iniFile.ReadBoolean("Callouts", "SuspiciousActivity", SuspiciousActivity);

            // Callout Measurements
            SearchAreaSize = iniFile.ReadInt32("Measurements", "SearchAreaSize", SearchAreaSize);
            CalloutDistance = iniFile.ReadInt32("Measurements", "CalloutDistance", CalloutDistance);

            // Customization
            Callsign = iniFile.ReadString("Customization", "Callsign", Callsign);
            SubtitleColor = iniFile.ReadString("Customization", "SubtitleColor", SubtitleColor).Substring(0, 1).ToLower();

            // Keybindings
            TalkKey = iniFile.ReadEnum("Keybindings", "TalkKey", TalkKey);
            EndCalloutKey = iniFile.ReadEnum("Keybindings", "EndCalloutKey", EndCalloutKey);

            Game.LogTrivial("[Emergency Callouts]: Loaded settings.");
        }
    }
}