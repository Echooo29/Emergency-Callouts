using EmergencyCalloutsLE.Essential;
using LSPD_First_Response.Mod.API;
using Rage;
using static EmergencyCalloutsLE.Essential.Helper;

namespace EmergencyCalloutsLE
{
    public class Main : Plugin
    {
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;

            Game.LogTrivial("[Emergency Callouts - LE]: Successfully Loaded v" + Project.LocalVersion);
        }

        public override void Finally()
        {
            Game.LogTrivial("[Emergency Callouts - LE]: Successfully Unloaded");
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
            if (Settings.PublicIntoxication  && PUBRemoteState)  { Functions.RegisterCallout(typeof(Callouts.PublicIntoxication));  }
            if (Settings.Trespassing         && TRERemoteState)  { Functions.RegisterCallout(typeof(Callouts.Trespassing));         }
            if (Settings.DomesticViolence    && DOMRemoteState)  { Functions.RegisterCallout(typeof(Callouts.DomesticViolence));    }
            if (Settings.Burglary            && BURRemoteState)  { Functions.RegisterCallout(typeof(Callouts.Burglary));            }
            if (Settings.SuspiciousActivity  && SUSRemoteState)  { Functions.RegisterCallout(typeof(Callouts.SuspiciousActivity));  }
        }
    }
}