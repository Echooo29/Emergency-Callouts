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
        bool playerArrived;
        bool pedFound;
        bool ped2Found;
        bool needsRefreshing;
        bool calloutActive;
        bool pursuitActive;

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
            Victim = new Ped(World.GetNextPositionOnStreet(Suspect.Position.Around2D(10f)));
            VictimPersona = Functions.GetPersonaForPed(Victim);
            Victim.IsPersistent = true;
            Victim.BlockPermanentEvents = true;
            Victim.Health = 175;
            Victim.IsInvincible = true;
            Log.Creation(Victim, PedCategory.Victim);

            VictimBlip = Victim.AttachBlip();
            VictimBlip.SetColorOrange();
            VictimBlip.Scale = (float)Settings.PedBlipScale;
            VictimBlip.Alpha = 0f;

            Victim.Tasks.ReactAndFlee(Suspect);

            Suspect.Inventory.GiveNewWeapon("WEAPON_KNIFE", -1, true);
            Suspect.Tasks.FightAgainst(Victim);

            CalloutHandler();

            return base.OnCalloutAccepted();
        }

        private void CalloutHandler()
        {
            #region CalloutHandler
            try
            {
                calloutActive = true;

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

        private void Scenario1() // Start pursuit if victim is dead
        {
            #region Scenario 1
            try
            {
                GameFiber.StartNew(delegate
                {
                    try
                    {
                        while (true)
                        {
                            GameFiber.Yield();

                            if (Suspect.Exists() && Suspect.IsAlive && Victim.Exists() && Victim.IsDead && playerArrived && !pursuitActive)
                            {
                                LHandle pursuit = Functions.CreatePursuit();
                                Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                                Functions.AddPedToPursuit(pursuit, Suspect);
                                Play.PursuitAudio();

                                if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                                if (VictimBlip.Exists()) { VictimBlip.Delete(); }

                                pursuitActive = true;
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
                    }
                });
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            }
            #endregion
        }

        private void Scenario2() // Surrender if victim is dead
        {
            #region Scenario 2
            try
            {
                GameFiber.StartNew(delegate
                {
                    try
                    {
                        while (true)
                        {
                            GameFiber.Yield();

                            if (Suspect.Exists() && Suspect.IsAlive && Victim.Exists() && Victim.IsDead && playerArrived)
                            {
                                Suspect.Tasks.Clear();
                                Suspect.Tasks.PutHandsUp(-1, MainPlayer);
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
                    }
                });
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            }
            #endregion
        }

        private void Scenario3() // Shoot at victim and player
        {
            #region Scenario 3
            try
            {
                GameFiber.StartNew(delegate
                {
                    try
                    {
                        while (true)
                        {
                            GameFiber.Yield();

                            if (Suspect.Exists() && Victim.Exists() && playerArrived)
                            {
                                Suspect.GiveRandomHandgun(-1, true);
                                break;
                            }
                        }

                        while (true)
                        {
                            GameFiber.Yield();

                            if (Suspect.Exists() && Victim.Exists() && Victim.IsDead && Suspect.IsAlive)
                            {
                                Suspect.Tasks.Clear();
                                Suspect.Tasks.FightAgainst(MainPlayer);
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
                    }
                });
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
                
                if (Suspect.Exists() && (Suspect.IsDead || Suspect.IsCuffed) && !MainPlayer.IsAiming) { GameFiber.Sleep(5000); Handle.AdvancedEndingSequence(); }

                if (Settings.AllowController) { NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 27, true); }

                if (EntranceBlip.Exists()) { EntranceBlip.Position = Suspect.Position; }

                if (Suspect.Exists() && Suspect.IsAlive && !pursuitActive) { NativeFunction.Natives.SET_PED_MOVE_RATE_OVERRIDE(Suspect, 1.5f); }
                if (Victim.Exists() && Victim.IsAlive) { NativeFunction.Natives.SET_PED_MOVE_RATE_OVERRIDE(Victim, 0.75f); }

                #region PlayerArrived
                if (EntranceBlip.Exists() && MainPlayer.Position.DistanceTo(EntranceBlip.Position) < Settings.SearchAreaSize && !playerArrived)
                {
                    // Remove EntranceBlip
                    if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

                    // Make victim vulnerable
                    Victim.IsInvincible = false;

                    // Create SearchArea
                    SearchArea = new Blip(Suspect.Position.Around2D(30f), Settings.SearchAreaSize * 1.25f);
                    SearchArea.SetColorYellow();
                    SearchArea.Alpha = 0.5f;

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has arrived on scene");

                    playerArrived = true;
                }
                #endregion

                #region PlayerLeft
                if (MainPlayer.Position.DistanceTo(CalloutPosition) > Settings.SearchAreaSize * 3f && playerArrived && !pedFound)
                {
                    // Set OnScene
                    playerArrived = false;

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
                if (Suspect.Exists() && MainPlayer.Position.DistanceTo(Suspect.Position) < 10f && !pedFound && playerArrived)
                {
                    // Hide Subtitle
                    Display.HideSubtitle();

                    // Enable SuspectBlip
                    if (SuspectBlip.Exists()) { SuspectBlip.Alpha = 1f; }

                    // Remove SearchArea
                    if (SearchArea.Exists() && ped2Found) { SearchArea.Delete(); }

                    // Make Ped Fall
                    Victim.IsRagdoll = true;
                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has found {SuspectPersona.FullName} (Suspect)");

                    pedFound = true;
                }

                if (Victim.Exists() && MainPlayer.Position.DistanceTo(Victim.Position) < 15f && !ped2Found && playerArrived)
                {
                    // Hide Subtitle
                    Display.HideSubtitle();

                    // Enable VictimBlip
                    if (VictimBlip.Exists()) { VictimBlip.Alpha = 1f; }

                    // Remove SearchArea
                    if (SearchArea.Exists() && pedFound) { SearchArea.Delete(); }

                    // Make Ped Fall
                    Victim.IsRagdoll = true;
                    GameFiber.Sleep(1000);
                    Victim.IsRagdoll = false;

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has found {VictimPersona.FullName} (Victim)");

                    ped2Found = true;
                }
                #endregion

                #region RefreshSearchArea
                if (!pedFound)
                {
                    if (Suspect.Exists() && Suspect.Position.DistanceTo(CalloutPosition) < Settings.SearchAreaSize)
                    {
                        needsRefreshing = false;
                    }
                    else
                    {
                        needsRefreshing = true;
                    }
                }

                if (Suspect.Exists() && Suspect.Position.DistanceTo(CalloutPosition) > Settings.SearchAreaSize && needsRefreshing)
                {
                    CalloutPosition = Suspect.Position;
                    if (SearchArea.Exists()) { SearchArea.Delete(); }

                    SearchArea = new Blip(Suspect.Position.Around2D(30f), Settings.SearchAreaSize * 1.25f);
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

            calloutActive = false;

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