using EmergencyCallouts.Essential;
using LSPD_First_Response.Mod.API;
using Rage;
using static EmergencyCallouts.Essential.Helper;

namespace EmergencyCallouts
{
    public class Main : Plugin
    {
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;

            Game.LogTrivial("[Emergency Callouts]: Successfully Loaded v" + Project.LocalVersion);

            if (Functions.GetPlayerRadioAction() == LSPD_First_Response.Mod.Menus.EPoliceRadioAction.None)
            {
                Functions.SetPlayerRadioAction(LSPD_First_Response.Mod.Menus.EPoliceRadioAction.Shoulder);
            }
        }

        public override void Finally()
        {
            Game.LogTrivial("[Emergency Callouts]: Successfully Unloaded");
        }

        private static void OnOnDutyStateChangedHandler(bool OnDuty)
        {
            if (OnDuty)
            {
                Settings.Initialize();
                Handle.RemoteStates();
                RegisterCallouts();
                UpdateChecker.UpdateAvailable();
            }
        }

        private static void RegisterCallouts()
        {
            if (!Settings.EarlyAccess)
            {
                if (Settings.PublicIntoxication && PUBRemoteState) { Functions.RegisterCallout(typeof(Callouts.PublicIntoxication)); }
                if (Settings.Trespassing && TRERemoteState) { Functions.RegisterCallout(typeof(Callouts.Trespassing)); }
                if (Settings.DomesticViolence && DOMRemoteState) { Functions.RegisterCallout(typeof(Callouts.DomesticViolence)); }
                if (Settings.Burglary && BURRemoteState) { Functions.RegisterCallout(typeof(Callouts.Burglary)); }
                if (Settings.SuspiciousActivity && SUSRemoteState) { Functions.RegisterCallout(typeof(Callouts.SuspiciousActivity)); }
            }
            else
            {
                if (Settings.PublicIntoxication) { Functions.RegisterCallout(typeof(Callouts.PublicIntoxication)); }
                if (Settings.Trespassing) { Functions.RegisterCallout(typeof(Callouts.Trespassing)); }
                if (Settings.DomesticViolence) { Functions.RegisterCallout(typeof(Callouts.DomesticViolence)); }
                if (Settings.Burglary) { Functions.RegisterCallout(typeof(Callouts.Burglary)); }
                if (Settings.SuspiciousActivity) { Functions.RegisterCallout(typeof(Callouts.SuspiciousActivity)); }
            }
        }
    }
}