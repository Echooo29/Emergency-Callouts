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
        internal static string NotificationIconDictionary = "web_lossantospolicedept";
        internal static string NotificationIconName = "web_lossantospolicedept";

        // Keys
        internal static Keys ToggleMenu = Keys.Delete;
        internal static Keys Talk = Keys.Y;
        internal static Keys EndCallout = Keys.End;

        // Other
        internal static bool PlayPursuitAudio = true;
        internal static string SubtitleColor = "b";

        internal static void Initialize()
        {
            Game.LogTrivial("[TRACE] Emergency Callouts: Loading settings.");

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
            NotificationIconDictionary = iniFile.ReadString("Notifications", "NotificationIconDictionary", NotificationIconDictionary);
            NotificationIconName = iniFile.ReadString("Notifications", "NotificationIconName", NotificationIconName);

            // Keybindings
            ToggleMenu = iniFile.ReadEnum("Keybindings", "ToggleMenu", Keys.Delete);
            Talk = iniFile.ReadEnum("Keybindings", "Talk", Keys.Y);
            EndCallout = iniFile.ReadEnum("Keybindings", "EndCallout", Keys.End);

            // Other
            PlayPursuitAudio = iniFile.ReadBoolean("Other", "PlayPursuitAudio", PlayPursuitAudio);
            SubtitleColor = iniFile.ReadString("Other", "SubtitleColor", SubtitleColor).Substring(0, 1).ToLower();

            Game.LogTrivial("[TRACE] Emergency Callouts: Loaded settings.");
        }
    }
}