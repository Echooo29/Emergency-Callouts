﻿using EmergencyCallouts.Essential;
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
                RegisterCallouts();
                UpdateChecker.UpdateAvailable();

                if (Functions.GetPlayerRadioAction() == LSPD_First_Response.Mod.Menus.EPoliceRadioAction.None)
                {
                    Functions.SetPlayerRadioAction(LSPD_First_Response.Mod.Menus.EPoliceRadioAction.Chest);
                }
            }
        }

        private static void RegisterCallouts()
        {
            Game.LogTrivial("[Emergency Callouts]: Registering callouts");
            if (Settings.PublicIntoxication) { Functions.RegisterCallout(typeof(Callouts.PublicIntoxication)); }
            if (Settings.Trespassing) { Functions.RegisterCallout(typeof(Callouts.Trespassing)); }
            if (Settings.DomesticViolence) { Functions.RegisterCallout(typeof(Callouts.DomesticViolence)); }
            if (Settings.Burglary) { Functions.RegisterCallout(typeof(Callouts.Burglary)); }
            if (Settings.SuspiciousActivity) { Functions.RegisterCallout(typeof(Callouts.SuspiciousActivity)); }
            Game.LogTrivial("[Emergency Callouts]: Registered callouts");
        }
    }
}