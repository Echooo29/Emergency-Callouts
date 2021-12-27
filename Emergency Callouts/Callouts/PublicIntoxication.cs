using EmergencyCallouts.Essential;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using static EmergencyCallouts.Essential.Color;
using static EmergencyCallouts.Essential.Helper;
using Entity = EmergencyCallouts.Essential.Helper.Entity;

namespace EmergencyCallouts.Callouts
{
    [CalloutInfo("Public Intoxication", CalloutProbability.Medium)]
    public class PublicIntoxication : Callout
    {
        bool OnScene;
        bool PedFound;
        bool PedDetained;
        bool NeedsRefreshing;
        bool CalloutActive;

        Ped Suspect;
        Persona SuspectPersona;

        Blip EntranceBlip;
        Blip SearchArea;
        Blip SuspectBlip;

        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutPosition = World.GetNextPositionOnStreet(MainPlayer.Position.Around2D(100f, Settings.CalloutDistance));

            CalloutMessage = "Public Intoxication";
            CalloutDetails = "There are multiple reports of a person under the influence of alcohol.";
            CalloutArea = World.GetStreetName(CalloutPosition);

            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 60f);
            AddMinimumDistanceCheck(30f, CalloutPosition);

            Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT CRIME_PUBLIC_INTOXICATION IN_OR_ON_POSITION UNITS_RESPOND_CODE_02", CalloutPosition);

            return base.OnBeforeCalloutDisplayed();
        }

        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} ignored the callout");
            Functions.PlayScannerAudio("PED_RESPONDING_DISPATCH");

            base.OnCalloutNotAccepted();
        }

        public override bool OnCalloutAccepted()
        {
            // Callout Accepted
            Log.OnCalloutAccepted(CalloutMessage, CalloutScenario);

            // Accept Messages
            Display.AcceptNotification(CalloutDetails);
            Display.AcceptSubtitle($"Go to the ~r~{CalloutMessage}~s~ at ~y~{CalloutArea}~s~.");

            EntranceBlip = new Blip(CalloutPosition);
            EntranceBlip.EnableRoute();

            Suspect = new Ped(Entity.GetRandomMaleModel(), CalloutPosition, 0f);
            SuspectPersona = Functions.GetPersonaForPed(Suspect);
            Suspect.SetDefaults();
            Suspect.SetIntoxicated();
            Log.Creation(Suspect, PedCategory.Suspect);

            SuspectBlip = Suspect.AttachBlip();
            SuspectBlip.SetColor(Colors.Yellow);
            SuspectBlip.ScaleForPed();
            SuspectBlip.Disable();

            Suspect.Tasks.Wander();

            CalloutHandler();

            return base.OnCalloutAccepted();
        }

        private void CalloutHandler()
        {
            try
            {
                CalloutActive = true;
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
                    case 4:
                        Scenario4();
                        break;
                    case 5:
                        Scenario5();
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        private void Scenario1()
        {
            #region Default
            try
            {
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            #endregion
        }

        private void Scenario2()
        {
            #region Hostile
            try
            {
                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 10f && MainPlayer.IsOnFoot)
                        {
                            Suspect.Tasks.FightAgainst(MainPlayer);

                            break;
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            #endregion
        }

        private void Scenario3()
        {
            #region Bottle
            try
            {
                Suspect.Inventory.GiveNewWeapon("WEAPON_BOTTLE", -1, true);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            #endregion
        }

        private void Scenario4()
        {
            #region Bottle & Hostile
            try
            {
                Suspect.Inventory.GiveNewWeapon("WEAPON_BOTTLE", -1, true);

                while (CalloutActive)
                {
                    GameFiber.Yield();

                    if (MainPlayer.Position.DistanceTo(Suspect.Position) < 10f && MainPlayer.IsOnFoot)
                    {
                        Suspect.Tasks.FightAgainst(MainPlayer);

                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            #endregion
        }

        private void Scenario5()
        {
            #region Passout
            try
            {
                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 5f && MainPlayer.IsOnFoot)
                        {
                            if (Suspect.Exists()) { Suspect.Kill(); }

                            break;
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            #endregion
        }

        public override void Process()
        {
            base.Process();

            Handle.ManualEnding();
            Handle.AutomaticEnding(Suspect);
            Handle.PreventFirstResponderCrash(Suspect);
            Handle.PreventDistanceCrash(CalloutPosition, OnScene, PedFound);

            #region PlayerArrived
            if (MainPlayer.Position.DistanceTo(CalloutPosition) < Settings.SearchAreaSize && !OnScene)
            {
                // Remove EntranceBlip
                EntranceBlip.Remove();

                // Create SearchArea
                SearchArea = new Blip(Suspect.Position.Around(5f, 30f), Settings.SearchAreaSize);
                SearchArea.SetColor(Colors.Yellow);
                SearchArea.Alpha = 0.5f;

                // Display Subtitle
                Game.DisplaySubtitle("Find the ~r~drunk person~s~ in the ~y~area~s~.", 20000);

                Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has arrived on scene");

                OnScene = true;
            }
            #endregion

            #region PedFound
            if (MainPlayer.Position.DistanceTo(Suspect.Position) < 5f && !PedFound && OnScene && Suspect)
            {
                // Hide Subtitle
                Display.HideSubtitle();

                // Enable SuspectBlip
                SuspectBlip.Enable();

                // Remove SearchArea
                SearchArea.Remove();

                Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has found {SuspectPersona.FullName} (Suspect)");

                PedFound = true;
            }
            #endregion

            #region PedDetained
            if (Suspect.IsPedDetained() && !PedDetained)
            {
                // Remove SuspectBlip
                SuspectBlip.Remove();

                Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has detained {SuspectPersona.FullName} (Suspect)");

                PedDetained = true;
            }
            #endregion

            #region PlayerLeft
            if (MainPlayer.Position.DistanceTo(CalloutPosition) > Settings.SearchAreaSize * 3f && OnScene)
            {
                // Set OnScene
                OnScene = false;

                // Disable SuspectBlip
                SuspectBlip.Disable();

                // Delete SearchArea
                SearchArea.Remove();

                // Create EntranceBlip
                EntranceBlip = new Blip(CalloutPosition);

                // Enable Route
                EntranceBlip.EnableRoute();

                Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has left the scene");
            }
            #endregion

            #region Refresh Search Area
            if (!PedFound)
            {
                if (Suspect.Position.DistanceTo(CalloutPosition) < Settings.SearchAreaSize)
                {
                    NeedsRefreshing = false;
                }
                else
                {
                    NeedsRefreshing = true;
                }
            }

            if (Suspect.Position.DistanceTo(CalloutPosition) > Settings.SearchAreaSize && NeedsRefreshing)
            {
                CalloutPosition = Suspect.Position;
                SearchArea.Remove();

                SearchArea = new Blip(Suspect.Position.Around(10f, 30f), Settings.SearchAreaSize);
                SearchArea.SetColor(Colors.Yellow);
                SearchArea.Alpha = 0.5f;
                Game.LogTrivial("[Emergency Callouts]: Refreshed SearchArea");

                Functions.PlayScannerAudioUsingPosition("SUSPECT_LAST_SEEN IN_OR_ON_POSITION", Suspect.Position);
            }
            #endregion
        }

        public override void End()
        {
            base.End();

            CalloutActive = false;

            if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            if (SearchArea.Exists()) { SearchArea.Delete(); }
            if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

            Display.HideSubtitle();
            Display.DetachMessage();
            Log.OnCalloutEnded(CalloutMessage, CalloutScenario);
        }
    }
}