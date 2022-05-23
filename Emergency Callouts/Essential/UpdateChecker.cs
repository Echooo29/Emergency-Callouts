using Rage;
using System;
using System.Net;

namespace EmergencyCallouts.Essential
{
    internal class UpdateChecker
    {
        internal static string OnlineVersion = string.Empty;
        private static bool ExceptionOccured;

        internal static bool UpdateAvailable()
        {
            WebClient webClient = new WebClient();
            Uri OnlineVersionURI = new Uri("https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=37760&textOnly=1");

            try
            {
                OnlineVersion = webClient.DownloadString(OnlineVersionURI).Trim();
            }
            catch (WebException)
            {
                Game.DisplayNotification("commonmenu", "mp_alerttriangle", "Emergency Callouts", "~r~Error", "Failed to check for updates, ~y~Possible network error.");
                Game.LogTrivial("[Emergency Callouts]: Failed to check for updates");
                ExceptionOccured = true;
            }

            if (OnlineVersion != Project.LocalVersion && !Settings.EarlyAccess && !ExceptionOccured)
            {
                Game.DisplayNotification("commonmenu", "mp_alerttriangle", "Emergency Callouts", $"~r~v{Project.LocalVersion} ~c~by Faya", $"Found update ~g~v{OnlineVersion} ~s~available for you!");
                Game.LogTrivial("[Emergency Callouts]: Found an update available");
                return true;
            }
            else if (Settings.EarlyAccess)
            {
                Game.DisplayNotification("dia_police", "dia_police", "Emergency Callouts", $"~g~v{Project.LocalVersion} ~c~by Faya", $"~y~Early Access~s~ ready for use!");
                Game.LogTrivial("[Emergency Callouts]: Loaded early access");
                return false;
            }
            else
            {
                Game.DisplayNotification("dia_police", "dia_police", "Emergency Callouts", $"~g~v{Project.LocalVersion} ~c~by Faya", "~y~Reporting for duty!");
                Game.LogTrivial("[Emergency Callouts]: No updates available");
                return false;
            }
        }
    }
}
