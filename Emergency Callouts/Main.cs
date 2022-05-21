using static EmergencyCallouts.Essential.Helper;
using EmergencyCallouts.Essential;
using LSPD_First_Response.Mod.API;
using Rage;

namespace EmergencyCallouts
{
    public class Main : Plugin
    {
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;

            Game.LogTrivial("[Emergency Callouts]: Successfully loaded v" + LocalVersion);
        }

        public override void Finally()
        {
            Game.LogTrivial("[Emergency Callouts]: Successfully unloaded");
        }

        private static void OnOnDutyStateChangedHandler(bool OnDuty)
        {
            if (OnDuty)
            {
                Settings.Initialize();
                UpdateChecker.IsUpdateAvailable();
                RegisterCallouts();
            }
        }

        private static void RegisterCallouts()
        {
            Game.LogTrivial("[Emergency Callouts]: Registered x callouts");
        }
    }
}