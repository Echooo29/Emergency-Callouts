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
                Handle.RemoteStates();
                RegisterCallouts();
                UpdateChecker.UpdateAvailable();
            }
        }

        private static void RegisterCallouts()
        {
            if (Settings.PublicIntoxication  && PUBRemoteState == true)  { Functions.RegisterCallout(typeof(Callouts.PublicIntoxication));  }
            if (Settings.Trespassing         && TRERemoteState == true)  { Functions.RegisterCallout(typeof(Callouts.Trespassing));         }
            if (Settings.DomesticViolence    && DOMRemoteState == true)  { Functions.RegisterCallout(typeof(Callouts.DomesticViolence));    }
            if (Settings.Burglary            && BURRemoteState == true)  { Functions.RegisterCallout(typeof(Callouts.Burglary));            }
            if (Settings.SuspiciousActivity  && SUSRemoteState == true)  { Functions.RegisterCallout(typeof(Callouts.SuspiciousActivity));  }
        }
    }
}