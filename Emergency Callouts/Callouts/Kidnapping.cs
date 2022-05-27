using EmergencyCallouts.Essential;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using RAGENativeUI;
using System;
using System.Reflection;
using static EmergencyCallouts.Essential.Color;
using static EmergencyCallouts.Essential.Helper;

namespace EmergencyCallouts.Callouts
{
    [CalloutInfo("[EC] Kidnapping", CalloutProbability.Medium)]
    public class Kidnapping : Callout
    {
        bool CalloutActive;
        bool PlayerArrived;
        bool PedFound;
        bool Ped2Found;
        bool PedDetained;
        bool Ped2Detained;
        bool PullingOver;

        string[] SuspectVehicles = { "SPEEDO", "BURRITO", "BURRITO2", "BURRITO3", "BURRITO4", "GBURRITO", "GBURRITO2" };

        Vehicle SuspectVehicle;

        Ped Suspect;
        Ped Victim;

        Blip SearchArea;
        Blip SuspectBlip;
        Blip VictimBlip;

        public override bool OnBeforeCalloutDisplayed()
        {
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, Settings.SearchAreaSize / 2.5f);

            CalloutPosition = World.GetNextPositionOnStreet(MainPlayer.Position.Around2D(200, Settings.MaxCalloutDistance));
            CalloutArea = World.GetStreetName(CalloutPosition);
            CalloutMessage = "Kidnapping";
            CalloutAdvisory = "";
            CalloutScenario = random.Next(1, 4);

            Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT CRIME_ASSAULT IN_OR_ON_POSITION", CalloutPosition);

            return base.OnBeforeCalloutDisplayed();
        }

        public override void OnCalloutDisplayed()
        {
            if (Other.PluginChecker.IsCalloutInterfaceRunning)
            {
                Other.CalloutInterfaceFunctions.SendCalloutDetails(this, "CODE-2-HIGH", "");
            }

            base.OnCalloutDisplayed();
        }

        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} ignored the callout");

            if (!Other.PluginChecker.IsCalloutInterfaceRunning)
            {
                Functions.PlayScannerAudio("PED_RESPONDING_DISPATCH");
            }

            base.OnCalloutNotAccepted();
        }

        public override bool OnCalloutAccepted()
        {
            try
            {
                // Callout Accepted
                Log.OnCalloutAccepted(CalloutMessage, CalloutScenario);

                // Accepting Messages
                Display.AcceptSubtitle(CalloutMessage, CalloutArea);
                Display.OutdatedReminder();

                // Suspect Vehicle
                SuspectVehicle = new Vehicle(SuspectVehicles[new Random().Next(0, SuspectVehicles.Length)], CalloutPosition, 0f);
                SuspectVehicle.IsPersistent = true;

                // Suspect
                Suspect = new Ped(CalloutPosition);
                Suspect.IsPersistent = true;
                Suspect.BlockPermanentEvents = true;

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColorRed();
                SuspectBlip.Scale = (float)Settings.PedBlipScale;
                SuspectBlip.Alpha = 1f;

                // Victim
                Victim = new Ped(CalloutPosition);
                Victim.IsPersistent = true;
                Victim.BlockPermanentEvents = true;

                VictimBlip = Victim.AttachBlip();
                VictimBlip.SetColorGreen();
                VictimBlip.Scale = (float)Settings.PedBlipScale;
                VictimBlip.Alpha = 0f;

                // Search Area
                SearchArea = new Blip(CalloutPosition, Settings.SearchAreaSize * 2);
                SearchArea.SetColorYellow();
                SearchArea.Alpha = 0.5f;
                SearchArea.IsRouteEnabled = true;

                Suspect.WarpIntoVehicle(SuspectVehicle, -1);
                Suspect.Tasks.CruiseWithVehicle(10f);
                Victim.WarpIntoVehicle(SuspectVehicle, 3);

                CalloutHandler();
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            }

            return base.OnCalloutAccepted();
        }

        private void CalloutHandler()
        {
            #region CalloutHandler
            try
            {
                CalloutActive = true;

                // Scenario Deciding
                switch (CalloutScenario)
                {
                    case 1:
                        Scenario1();
                        break;
                    case 2:
                        Scenario2();
                        break;
                    case 3:
                        Scenario3();
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            }
            #endregion
        }

        private void Scenario1()
        {
            #region Scenario 1
            try
            {
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            }
            #endregion
        }

        private void Scenario2()
        {
            #region Scenario 2
            try
            {
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            }
            #endregion
        }

        private void Scenario3()
        {
            #region Scenario 3
            try
            {
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            }
            #endregion
        }

        public override void Process()
        {
            base.Process();
            try
            {
                Handle.ManualEnding();
                if (Settings.AllowController) { NativeFunction.Natives.xFE99B66D079CF6BC(0, 27, true); }

                #region Pulling Over Suspect
                if (MainPlayer.Position.DistanceTo(Suspect.Position) <= 15f && !PullingOver)
                {
                    LHandle pursuit = Functions.CreatePursuit();
                    Functions.AddPedToPursuit(pursuit, Suspect);
                    Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                    PullingOver = true;
                }
                #endregion

                #region PedFound
                if (MainPlayer.Position.DistanceTo(Suspect.Position) < 5f && !PedFound && PlayerArrived && Suspect.Exists())
                {
                    // Set PedFound
                    PedFound = true;

                    // Enable SuspectBlip
                    if (Suspect.Exists()) { SuspectBlip.Alpha = 1f; }
                }

                if (MainPlayer.Position.DistanceTo(Victim.Position) < 5f && !Ped2Found && PlayerArrived && Victim.Exists())
                {
                    // Set Ped2Found
                    Ped2Found = true;

                    // Enable VictimBlip
                    if (Victim.Exists()) { VictimBlip.Alpha = 1f; }
                }
                #endregion

                #region PedDetained
                if (Functions.IsPedStoppedByPlayer(Suspect) && !PedDetained && Suspect.Exists())
                {
                    // Set PedDetained
                    PedDetained = true;

                    // Delete SuspectBlip
                    if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                    Game.LogTrivial("[Emergency Callouts]: Deleted SuspectBlip");
                }

                if (Functions.IsPedStoppedByPlayer(Victim) && !Ped2Detained && Victim.Exists())
                {
                    // Set Ped2Detained
                    Ped2Detained = true;

                    // Delete VictimBlip
                    if (VictimBlip.Exists()) { VictimBlip.Delete(); }
                    Game.LogTrivial("[Emergency Callouts]: Deleted VictimBlip");
                }
                #endregion

                #region Search Area Refresh
                if (Suspect.Position.DistanceTo(SearchArea.Position) > Settings.SearchAreaSize * 2)
                {
                    if (SearchArea.Exists()) { SearchArea.Delete(); }
                    SearchArea.Position = Suspect.Position.Around2D(10, Settings.SearchAreaSize);
                    CalloutPosition = Suspect.Position;

                    SearchArea = new Blip(CalloutPosition, Settings.SearchAreaSize * 2);
                    SearchArea.Alpha = 0.5f;
                    SearchArea.SetColorYellow();
                    SearchArea.IsRouteEnabled = true;
                }
                #endregion
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
                End();
            }
        }

        public override void End()
        {
            base.End();

            CalloutActive = false;

            if (SuspectVehicle.Exists()) { SuspectVehicle.Dismiss(); }
            if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            if (Victim.Exists()) { Victim.Delete(); }
            if (VictimBlip.Exists()) { VictimBlip.Delete(); }
            if (SearchArea.Exists()) { SearchArea.Delete(); }

            Display.HideSubtitle();
            Display.EndNotification();
            Log.OnCalloutEnded(CalloutMessage, CalloutScenario);
        }
    }
}