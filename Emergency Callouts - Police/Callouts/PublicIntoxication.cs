using EmergencyCallouts.Essential;
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
        bool NearPed;
        bool PedDetained;
        bool NeedsRefreshing;
        bool CalloutActive;

        Ped Suspect;
        Blip EntranceBlip;
        Blip SearchArea;
        Blip SuspectBlip;

        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutPosition = World.GetNextPositionOnStreet(MainPlayer.Position.Around(1000f));

            CalloutMessage = "Public Intoxication";
            CalloutDetails = "There are multiple reports of a person under the influence of alcohol.";

            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 60f);
            //AddMinimumDistanceCheck(30f, CalloutPosition);

            Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT CRIME_PUBLIC_INTOXICATION IN_OR_ON_POSITION UNITS_RESPOND_CODE_02", CalloutPosition);

            return base.OnBeforeCalloutDisplayed();
        }

        public override void OnCalloutNotAccepted()
        {
            Functions.PlayScannerAudio("PED_RESPONDING_DISPATCH");
            base.OnCalloutNotAccepted();
        }

        public override bool OnCalloutAccepted()
        {
            Display.AttachMessage(CalloutDetails);

            EntranceBlip = new Blip(CalloutPosition);
            EntranceBlip.EnableRoute();

            Suspect = new Ped(Entity.GetRandomMaleModel(), CalloutPosition, 0f);
            Suspect.SetDefaults();
            Suspect.SetIntoxicated();
            Game.LogTrivial($"[Emergency Callouts]: Created Suspect ({Suspect.Model.Name}) at " + Suspect.Position);

            SuspectBlip = Suspect.AttachBlip();
            SuspectBlip.SetColor(Colors.Yellow);
            SuspectBlip.ScaleForPed();
            SuspectBlip.Disable();

            Suspect.Tasks.Wander();
            Display.PedDescription(Suspect, DescriptionCategories.Suspect);

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
                Log.CalloutException(this, "CalloutHandler", e);
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
                Log.CalloutException(this, "Scenario1", e);
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
                            Game.LogTrivial("[Emergency Callouts]: Assigned Suspect to fight " + PlayerPersona.FullName);

                            break;
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Log.CalloutException(this, "Scenario2", e);
            }
            #endregion
        }

        private void Scenario3()
        {
            #region Bottle
            try
            {
                Suspect.Inventory.GiveNewWeapon("WEAPON_BOTTLE", -1, true);
                Game.LogTrivial("[Emergency Callouts]: Added weapon (WEAPON_BOTTLE) to Suspect inventory");
            }
            catch (Exception e)
            {
                Log.CalloutException(this, "Scenario3", e);
            }
            #endregion
        }

        private void Scenario4()
        {
            #region Bottle & Hostile
            try
            {
                Suspect.Inventory.GiveNewWeapon("WEAPON_BOTTLE", -1, true);
                Game.LogTrivial("[Emergency Callouts]: Added weapon (WEAPON_BOTTLE) to Suspect inventory");

                while (CalloutActive)
                {
                    GameFiber.Yield();

                    if (MainPlayer.Position.DistanceTo(Suspect.Position) < 10f && MainPlayer.IsOnFoot)
                    {
                        Suspect.Tasks.FightAgainst(MainPlayer);
                        Game.LogTrivial("[Emergency Callouts]: Assigned Suspect to fight " + PlayerPersona.FullName);

                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.CalloutException(this, "Scenario4", e);
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
                            Entity.Kill(Suspect);
                            Game.LogTrivial("[Emergency Callouts]: Killed Suspect");

                            break;
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Log.CalloutException(this, "Scenario5", e);
            }
            #endregion
        }

        public override void Process()
        {
            base.Process();

            Handle.ManualEnding();
            Handle.AutomaticEnding(Suspect);
            Handle.PreventFirstResponderCrash(Suspect);
            Handle.PreventDistanceCrash(CalloutPosition, OnScene, NearPed);
            
            #region On Scene
            if (MainPlayer.Position.DistanceTo(CalloutPosition) < Settings.SearchAreaSize && !OnScene)
            {
                Display.PedDescription(Suspect, DescriptionCategories.Suspect);

                // Remove EntranceBlip
                EntranceBlip.Remove();

                // Create SearchArea
                SearchArea = new Blip(Suspect.Position.Around(5f, 30f), Settings.SearchAreaSize);
                SearchArea.SetColor(Colors.Yellow);
                SearchArea.Alpha = 0.5f;

                // Display Subtitle
                Game.DisplaySubtitle("Find the ~y~drunk person~s~ in the ~y~area~s~.", 10000);

                Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has arrived on scene");

                OnScene = true;
            }
            #endregion

            #region NearPed
            if (MainPlayer.Position.DistanceTo(Suspect.Position) < 5f && !NearPed && OnScene && Suspect)
            {
                // Hide Subtitle
                Display.HideSubtitle();

                // Enable SuspectBlip
                SuspectBlip.Enable();

                // Remove SearchArea
                SearchArea.Remove();

                Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has found the suspect");

                NearPed = true;
            }
            #endregion

            #region Ped Detained
            if (Suspect.IsPedDetained() && !PedDetained)
            {
                // Remove SuspectBlip
                SuspectBlip.Remove();

                Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has detained the suspect");

                PedDetained = true;
            }
            #endregion

            #region Left Scene
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
            if (!NearPed)
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

            if (Suspect.Position.DistanceTo(CalloutPosition) > Settings.SearchAreaSize && NeedsRefreshing == true)
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

            Entity.Dismiss(Suspect);
            Entity.Delete(SuspectBlip);
            Entity.Delete(SearchArea);
            Entity.Delete(EntranceBlip);

            Display.HideSubtitle();
            Display.DetachMessage();
            Log.CalloutEnded(CalloutMessage, CalloutScenario);
        }
    }
}