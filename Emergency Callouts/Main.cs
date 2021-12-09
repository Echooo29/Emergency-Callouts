using EmergencyCallouts.Essential;
using LSPD_First_Response.Mod.API;
using Rage;
using System.Net;
using static EmergencyCallouts.Essential.Helper;

namespace EmergencyCallouts
{
    public class Main : Plugin
    {
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;

            Game.LogTrivial($"[INFO] Emergency Callouts {Project.LocalVersion} has been loaded.");
        }

        public override void Finally()
        {
            Game.LogTrivial("[INFO] Emergency Callouts has been unloaded.");
        }

        private static void OnOnDutyStateChangedHandler(bool OnDuty)
        {
            if (OnDuty)
            {
                Settings.Initialize();
                Check.RemoteState();
                RegisterCallouts();
                UpdateChecker.UpdateAvailable();
                FileExists.EmergencyCalloutsINI();
                FileExists.StopThePed(true);
                FileExists.UltimateBackup(true);
            }
        }

        private static void RegisterCallouts()
        {
            if (Settings.PublicIntoxication && PUBRemoteState == true) { Functions.RegisterCallout(typeof(Callouts.PublicIntoxication)); }
            if (Settings.Trespassing && TRERemoteState == true) { Functions.RegisterCallout(typeof(Callouts.Trespassing)); }
            if (Settings.DomesticViolence && DOMRemoteState == true) { Functions.RegisterCallout(typeof(Callouts.DomesticViolence)); }
            if (Settings.Burglary && BURRemoteState == true) { Functions.RegisterCallout(typeof(Callouts.Burglary)); }
            if (Settings.SuspiciousActivity && SUSRemoteState == true) { Functions.RegisterCallout(typeof(Callouts.SuspiciousActivity)); }
        }
    }
}