using EmergencyCallouts.Essential;
using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Reflection;
using static EmergencyCallouts.Essential.Helper;

namespace EmergencyCallouts
{
    public class Main : Plugin
    {
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;

            Game.LogTrivial("[Emergency Callouts]: Successfully Loaded v" + Project.LocalVersion);
        }

        public override void Finally()
        {
            Game.LogTrivial("[Emergency Callouts]: Successfully Unloaded");
        }

        private static void OnOnDutyStateChangedHandler(bool OnDuty)
        {
            if (OnDuty)
            {
                try
                {
                    Settings.Initialize();
                    RegisterCallouts();
                    UpdateChecker.UpdateAvailable();

                    if (Functions.GetPlayerRadioAction() == LSPD_First_Response.Mod.Menus.EPoliceRadioAction.None)
                    {
                        Game.LogTrivial("[Emergency Callouts]: User didn't set a radio action");
                        Functions.SetPlayerRadioAction(LSPD_First_Response.Mod.Menus.EPoliceRadioAction.Chest);
                        Game.LogTrivial("[Emergency Callouts]: Set a radio action for user");
                    }
                }
                catch (Exception e)
                {
                    Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
                }
            }
        }

        private static void RegisterCallouts()
        {
            if (Settings.AttemptedMurder) { Functions.RegisterCallout(typeof(Callouts.AttemptedMurder)); }
            if (Settings.Trespassing) { Functions.RegisterCallout(typeof(Callouts.Trespassing)); }
            if (Settings.DomesticViolence) { Functions.RegisterCallout(typeof(Callouts.DomesticViolence)); }
            if (Settings.Burglary) { Functions.RegisterCallout(typeof(Callouts.Burglary)); }
            if (Settings.SuspiciousActivity) { Functions.RegisterCallout(typeof(Callouts.SuspiciousActivity)); }

            Game.LogTrivial("[Emergency Callouts]: Registered 4 callouts");
        }
    }
}