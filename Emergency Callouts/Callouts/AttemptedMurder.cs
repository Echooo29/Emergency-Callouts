using EmergencyCallouts.Essential;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Reflection;
using System.Windows.Forms;
using static EmergencyCallouts.Essential.Color;
using static EmergencyCallouts.Essential.Helper;
using Entity = EmergencyCallouts.Essential.Helper.Entity;
using RAGENativeUI;
using Rage.Native;

namespace EmergencyCallouts.Callouts
{
    [CalloutInfo("[EC] Attempted Murder", CalloutProbability.Medium)]
    public class AttemptedMurder : Callout
    {
        bool PlayerArrived;
        bool PedFound;
        bool NeedsRefreshing;
        bool CalloutActive;

        new Vector3 CalloutPosition;

        Ped Suspect;
        Ped Victim;
        Persona SuspectPersona;
        Persona VictimPersona;

        Blip VictimBlip;
        Blip SuspectBlip;
        Blip EntranceBlip;
        Blip SearchArea;

        public override bool OnBeforeCalloutDisplayed()
        {
            int count = 0;

            CalloutMessage = "Attempted Murder";
            CalloutAdvisory = "";
            CalloutScenario = random.Next(1, 4);

            while (!World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around2D(200f, Settings.MaxCalloutDistance)).GetSafePositionForPed(out CalloutPosition))
            {
                GameFiber.Yield();

                count++;
                if (count >= 10) { CalloutPosition = World.GetNextPositionOnStreet(MainPlayer.Position.Around2D(200f, Settings.MaxCalloutDistance)); }
                CalloutArea = World.GetStreetName(CalloutPosition);
            }

            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, Settings.SearchAreaSize / 2.5f);
            AddMinimumDistanceCheck(30f, CalloutPosition);

            //Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT CRIME_PUBLIC_INTOXICATION IN_OR_ON_POSITION UNITS_RESPOND_CODE_02", CalloutPosition);

            return base.OnBeforeCalloutDisplayed();
        }

        public override void OnCalloutDisplayed()
        {
            if (Other.PluginChecker.IsCalloutInterfaceRunning)
            {
                Other.CalloutInterfaceFunctions.SendCalloutDetails(this, "CODE 3", "");
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
            // Callout Accepted
            Log.OnCalloutAccepted(CalloutMessage, CalloutScenario);

            // Accept Messages
            Display.AcceptSubtitle(CalloutMessage, CalloutArea);
            Display.OutdatedReminder();

            // EntranceBlip
            EntranceBlip = new Blip(CalloutPosition);
            if (EntranceBlip.Exists()) { EntranceBlip.IsRouteEnabled = true; }

            // Suspect
            Suspect = new Ped(Entity.GetRandomMaleModel(), CalloutPosition, 0f);
            SuspectPersona = Functions.GetPersonaForPed(Suspect);
            Suspect.IsPersistent = true;
            Suspect.BlockPermanentEvents = true;
            Log.Creation(Suspect, PedCategory.Suspect);

            SuspectBlip = Suspect.AttachBlip();
            SuspectBlip.SetColorRed();
            SuspectBlip.Scale = (float)Settings.PedBlipScale;
            SuspectBlip.Alpha = 0f;

            // Victim
            Victim = new Ped(CalloutPosition);
            VictimPersona = Functions.GetPersonaForPed(Victim);
            Victim.IsPersistent = true;
            Victim.BlockPermanentEvents = true;
            Log.Creation(Victim, PedCategory.Victim);

            VictimBlip = Victim.AttachBlip();
            VictimBlip.SetColorOrange();
            VictimBlip.Scale = (float)Settings.PedBlipScale;
            VictimBlip.Alpha = 0f;

            Victim.Tasks.ReactAndFlee(Suspect);

            Suspect.GiveRandomMeleeWeapon(-1, true);
            Suspect.Tasks.FightAgainst(Victim);

            CalloutHandler();

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
                Handle.PreventPickupCrash(Suspect);
                if (Settings.AllowController) { NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 27, true); }

                if (EntranceBlip.Exists()) { EntranceBlip.Position = Suspect.Position; }

                #region PlayerArrived
                if (EntranceBlip.Exists() && MainPlayer.Position.DistanceTo(EntranceBlip.Position) < Settings.SearchAreaSize && !PlayerArrived)
                {
                    // Remove EntranceBlip
                    if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

                    // Create SearchArea
                    SearchArea = new Blip(Suspect.Position.Around2D(30f), Settings.SearchAreaSize);
                    SearchArea.SetColorYellow();
                    SearchArea.Alpha = 0.5f;

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has arrived on scene");

                    PlayerArrived = true;
                }
                #endregion

                #region PlayerLeft
                if (MainPlayer.Position.DistanceTo(CalloutPosition) > Settings.SearchAreaSize * 3f && PlayerArrived && !PedFound)
                {
                    // Set OnScene
                    PlayerArrived = false;

                    // Delete SearchArea
                    if (SearchArea.Exists()) { SearchArea.Delete(); }

                    // Create EntranceBlip
                    EntranceBlip = new Blip(CalloutPosition);

                    // Enable Route
                    if (EntranceBlip.Exists()) { EntranceBlip.IsRouteEnabled = true; }

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has left the scene");
                }
                #endregion

                #region PedFound
                if (Suspect.Exists() && MainPlayer.Position.DistanceTo(Suspect.Position) < 20f && !PedFound && PlayerArrived)
                {
                    // Hide Subtitle
                    Display.HideSubtitle();

                    // Enable SuspectBlip
                    if (SuspectBlip.Exists()) { SuspectBlip.Alpha = 1f; }
                    if (VictimBlip.Exists()) { VictimBlip.Alpha = 1f; }

                    // Remove SearchArea
                    if (SearchArea.Exists()) { SearchArea.Delete(); }

                    // Make Ped Fall
                    NativeFunction.Natives.SET_PED_TO_RAGDOLL_WITH_FALL(Victim, 5000, 0, Victim.Position.X, Victim.Position.Y, Victim.Position.Z, 0, 0, 0, 0, 0, 0, 0);

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has found {SuspectPersona.FullName} (Suspect)");

                    PedFound = true;
                }
                #endregion

                #region RefreshSearchArea
                if (!PedFound)
                {
                    if (Suspect.Exists() && Suspect.Position.DistanceTo(CalloutPosition) < Settings.SearchAreaSize)
                    {
                        NeedsRefreshing = false;
                    }
                    else
                    {
                        NeedsRefreshing = true;
                    }
                }

                if (Suspect.Exists() && Suspect.Position.DistanceTo(CalloutPosition) > Settings.SearchAreaSize && NeedsRefreshing)
                {
                    CalloutPosition = Suspect.Position;
                    if (SearchArea.Exists()) { SearchArea.Delete(); }

                    SearchArea = new Blip(Suspect.Position.Around2D(30f), Settings.SearchAreaSize);
                    SearchArea.SetColorYellow();
                    SearchArea.Alpha = 0.5f;
                    Game.LogTrivial("[Emergency Callouts]: Refreshed SearchArea");

                    Functions.PlayScannerAudioUsingPosition("SUSPECT_LAST_SEEN IN_OR_ON_POSITION", Suspect.Position);
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

            if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (Victim.Exists()) { Victim.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            if (VictimBlip.Exists()) { VictimBlip.Delete(); }
            if (SearchArea.Exists()) { SearchArea.Delete(); }
            if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

            Display.HideSubtitle();
            Display.EndNotification();
            Log.OnCalloutEnded(CalloutMessage, CalloutScenario);
        }
    }
}