using EmergencyCallouts.Essential;
using Rage;
using System.Windows.Forms;

namespace EmergencyCallouts
{
    internal static class Localization
    {
        // General
        internal static string DispatchName = "Dispatch";

        // Callout Names
        internal static string Burglary = "Burglary";
        internal static string DomesticViolence = "Domestic Violence";
        internal static string PublicIntoxication = "Public Intoxication";
        internal static string SuspiciousActivity = "Suspicious Activity";
        internal static string Trespassing = "Trespassing";

        // Accept Notification
        internal static string AcceptNotificationSubtitle = "~y~Notification";

        internal static string BurglaryDetails = "A person has been seen looking through windows, caller states he's now lockpicking a door.";
        internal static string DomesticViolenceDetails = "A ~o~wife~s~ called about her ~r~husband~s~, claims she's continuingly being ~r~assaulted~s~.";
        internal static string PublicIntoxicationDetails = "There are multiple reports of a person under the influence of alcohol.";
        internal static string SuspiciousActivityDetails = "Multiple civilians called about a person handling possible firearms in the trunk of their car.";
        internal static string TrespassingDetails = "Someone reported a person trespassing on private property.";

        // Accept Subtitle
        internal static string AcceptSubtitleIntro = "Go to the";
        internal static string AcceptSubtitleAt = "At";

        // End Notification
        internal static string EndNotificationSubtitle = "~y~Notification";
        internal static string EndNotificationText = "Situation is under control.";
        internal static string EndNotificationTransmit = "~b~You~s~: Dispatch, no further assistance is needed.";
        internal static string EndNotificationAlertedPed = "You alerted the ~r~suspect~s~!";

        // Arriving Subtitle
        internal static string BurglarySubtitle = "Find the ~r~burglar~s~ in the ~y~area~s~.";
        internal static string DomesticViolenceSubtitle = "Find the ~o~victim~s~ and the ~r~suspect~s~ in the ~y~area~s~.";
        internal static string PublicIntoxicationSubtitle = "Find the ~r~drunk person~s~ in the ~y~area~s~.";
        internal static string SuspiciousActivitySubtitle = "Find the ~r~suspect~s~ in the ~y~area~s~.";
        internal static string TrespassingSubtitle = "Find the ~r~trespasser~s~ in the ~y~area~s~.";

        // Other
        internal static string ClimbClue = "~p~Clue~s~: The ~r~suspect~s~ has not climbed anything";

        internal static void Initialize()
        {
            Game.LogTrivial("[Emergency Callouts]: Loading localization");

            // Create the INI file
            var locFile = new InitializationFile(Project.LocalizationPath);
            locFile.Create();

            // General
            DispatchName = locFile.ReadString("General", "DispatchName", DispatchName);

            // Callout Names
            Burglary = locFile.ReadString("Callout Names", "Burglary", Burglary);
            DomesticViolence = locFile.ReadString("Callout Names", "DomesticViolence", DomesticViolence);
            PublicIntoxication = locFile.ReadString("Callout Names", "PublicIntoxication", PublicIntoxication);
            SuspiciousActivity = locFile.ReadString("Callout Names", "SuspiciousActivity", SuspiciousActivity);
            Trespassing = locFile.ReadString("Callout Names", "Trespassing", Trespassing);

            // Accept Notification
            AcceptNotificationSubtitle = locFile.ReadString("Accept Notification", "Subtitle", AcceptNotificationSubtitle);

            BurglaryDetails = locFile.ReadString("Accept Notification", "BurglaryDetails", BurglaryDetails);
            DomesticViolenceDetails = locFile.ReadString("Accept Notification", "DomesticViolenceDetails", DomesticViolenceDetails);
            PublicIntoxicationDetails = locFile.ReadString("Accept Notification", "PublicIntoxicationDetails", PublicIntoxicationDetails);
            SuspiciousActivityDetails = locFile.ReadString("Accept Notification", "SuspiciousActivityDetails", SuspiciousActivityDetails);
            TrespassingDetails = locFile.ReadString("Accept Notification", "DomesticViolenceDetails", TrespassingDetails);

            // Accept Subtitle
            AcceptSubtitleIntro = locFile.ReadString("Accept Subtitle", "Intro", AcceptSubtitleIntro);
            AcceptSubtitleAt = locFile.ReadString("Accept Subtitle", "At", AcceptSubtitleAt);

            // End Notification
            EndNotificationSubtitle = locFile.ReadString("End Notification", "Subtitle", EndNotificationSubtitle);
            EndNotificationText = locFile.ReadString("End Notification", "Text", EndNotificationText);
            EndNotificationTransmit = locFile.ReadString("End Notification", "Transmit", EndNotificationTransmit);
            EndNotificationAlertedPed = locFile.ReadString("End Notification", "AlertedPed", EndNotificationAlertedPed);

            // Arriving Subtitle

            // Other
            ClimbClue = locFile.ReadString("Other", "ClimbClue", ClimbClue);

            Game.LogTrivial("[Emergency Callouts]: Loaded localization");
        }
    }
}