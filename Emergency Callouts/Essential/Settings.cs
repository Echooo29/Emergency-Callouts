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

        // Callout Naming
        internal static string Callsign = "1-LINCOLN-18";
        internal static string DispatchName = "Dispatch";
        internal static string PublicIntoxicationName = "Public Intoxication";
        internal static string TrespassingName = "Trespassing";
        internal static string DomesticViolenceName = "Domestic Violence";
        internal static string BurglaryName = "Burglary In Progress";
        internal static string SuspiciousActivityName = "Suspicious Activity";

        // Callout Measurements
        internal static int SearchAreaSize = 60;
        internal static int CalloutDistance = 1000;

        // Callout Details
        internal static string CalloutDetailsSubtitle = "Callout Details";
        internal static string PublicIntoxicationDetails = "There are multiple reports of a person under the influence of alcohol.";
        internal static string TrespassingDetails = "Someone reported a person trespassing on private property.";
        internal static string DomesticViolenceDetails = "A concerned wife called about her husband, she said she's afraid for her life, shortly after she hung up abrubtly.";
        internal static string BurglaryDetails = "A person has been seen looking through windows, caller states he's now lockpicking a door.";
        internal static string SuspiciousActivityDetails = "Multiple civilians called about a person handling guns in the trunk of their car.";

        // AttachMessage
        internal static string AttachMessageSubtitle = "Unit Attached";
        internal static string AttachMessageText = "Attaching you to this call.";

        // DetachMessage
        internal static string DetachMessageSubtitle = "Unit Detached";
        internal static string DetachMessageText = "Detached you from this call.";

        // Notifications
        internal static string NotificationIconDictionary = "web_lossantospolicedept";
        internal static string NotificationIconName = "web_lossantospolicedept";

        // Keys
        internal static Keys TalkKey = Keys.Y;
        internal static Keys EndKey = Keys.End;

        // Other
        internal static bool PlayPursuitAudio = true;
        internal static bool DisplayArrestLine = true;
        internal static string EndCalloutRequest = "Dispatch, no further assistance is needed.";
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

            // Callout Naming
            Callsign = iniFile.ReadString("Callout Naming", "Callsign", Callsign);
            DispatchName = iniFile.ReadString("Callout Naming", "DispatchName", DispatchName);
            PublicIntoxicationName = iniFile.ReadString("Callout Naming", "PublicIntoxicationName", PublicIntoxicationName);
            TrespassingName = iniFile.ReadString("Callout Naming", "TrespassingName", TrespassingName);
            DomesticViolenceName = iniFile.ReadString("Callout Naming", "DomesticViolenceName", DomesticViolenceName);
            BurglaryName = iniFile.ReadString("Callout Naming", "BurglaryName", BurglaryName);
            SuspiciousActivityName = iniFile.ReadString("Callout Naming", "SuspiciousActivityName", SuspiciousActivityName);

            // Callout Measurements
            SearchAreaSize = iniFile.ReadInt32("Callout Measurements", "SearchAreaSize", SearchAreaSize);
            CalloutDistance = iniFile.ReadInt32("Callout Measurements", "CalloutDistance", CalloutDistance);

            // Callout Details
            CalloutDetailsSubtitle = iniFile.ReadString("Callout Details", "Subtitle", CalloutDetailsSubtitle);
            PublicIntoxicationDetails = iniFile.ReadString("Callout Details", "PublicIntoxicationDetails", PublicIntoxicationDetails);
            TrespassingDetails = iniFile.ReadString("Callout Details", "TrespassingDetails", TrespassingDetails);
            DomesticViolenceDetails = iniFile.ReadString("Callout Details", "DomesticViolenceDetails", DomesticViolenceDetails);
            BurglaryDetails = iniFile.ReadString("Callout Details", "BurglaryDetails", BurglaryDetails);
            SuspiciousActivityDetails = iniFile.ReadString("Callout Details", "SuspiciousActivityDetails", SuspiciousActivityDetails);

            // AttachMessage
            AttachMessageSubtitle = iniFile.ReadString("AttachMessage", "Subtitle", AttachMessageSubtitle);
            AttachMessageText = iniFile.ReadString("AttachMessage", "Text", AttachMessageText);

            // DetachMessage
            DetachMessageSubtitle = iniFile.ReadString("DetachMessage", "Subtitle", DetachMessageSubtitle);
            DetachMessageText = iniFile.ReadString("DetachMessage", "Text", DetachMessageText);

            // Notifications
            NotificationIconDictionary = iniFile.ReadString("Notifications", "NotificationIconDictionary", NotificationIconDictionary);
            NotificationIconName = iniFile.ReadString("Notifications", "NotificationIconName", NotificationIconName);

            // Keys
            TalkKey = iniFile.ReadEnum("Keys", "TalkKey", Keys.Y);
            EndKey = iniFile.ReadEnum("Keys", "EndKey", Keys.End);

            // Other
            PlayPursuitAudio = iniFile.ReadBoolean("Other", "PlayPursuitAudio", PlayPursuitAudio);
            DisplayArrestLine = iniFile.ReadBoolean("Other", "DisplayArrestLine", DisplayArrestLine);
            EndCalloutRequest = iniFile.ReadString("Other", "EndCalloutRequest", EndCalloutRequest);
            SubtitleColor = iniFile.ReadString("Other", "SubtitleColor", SubtitleColor).Substring(0, 1).ToLower();

            Game.LogTrivial("[TRACE] Emergency Callouts: Loaded settings.");
        }
    }
}