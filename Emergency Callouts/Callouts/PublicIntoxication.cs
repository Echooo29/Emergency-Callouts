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
        readonly int ScenarioNumber = random.Next(1, 6);

        bool CalloutActive;
        bool OnScene;
        bool PedFound;
        bool PedDetained;
        bool NeedsRefreshing;

        Ped Suspect;

        Blip SuspectBlip;
        Blip EntranceBlip;
        Blip SearchArea;

        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutPosition = World.GetNextPositionOnStreet(MainPlayer.Position.Around(100f, Settings.CalloutDistance));

            AddMinimumDistanceCheck(Settings.SearchAreaSize / 2.5f, CalloutPosition);
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, Settings.SearchAreaSize / 2.5f);

            CalloutMessage = Settings.PublicIntoxicationName;

            Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT CRIME_DISTURBING_THE_PEACE_01 IN_OR_ON_POSITION UNITS_RESPOND_CODE_02", CalloutPosition);

            return base.OnBeforeCalloutDisplayed();
        }

        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("[INFO] Emergency Callouts: Callout not accepted");
            Functions.PlayScannerAudio("PED_RESPONDING_DISPATCH");

            base.OnCalloutNotAccepted();
        }

        public override bool OnCalloutAccepted()
        {
            try
            {
                // Callout Accepted
                Log.CalloutAccepted(CalloutMessage, ScenarioNumber);

                // Attach Message
                Display.AttachMessage();

                // Callout Details
                Display.CalloutDetails(Settings.PublicIntoxicationDetails);

                // EntranceBlip
                EntranceBlip = new Blip(CalloutPosition);
                Game.LogTrivial("[TRACE] Emergency Callouts: Created EntranceBlip");

                Entity.EnableRoute(EntranceBlip);
                Game.LogTrivial("[TRACE] Emergency Callouts: Enabled route to CalloutPosition");

                // Suspect
                Suspect = new Ped(CalloutPosition);
                Suspect.SetDefaults();
                Suspect.SetIntoxicated();
                Game.LogTrivial($"[TRACE] Emergency Callouts: Created Suspect ({Suspect.Model.Name}) at " + Suspect.Position);

                // SuspectBlip
                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColor(Colors.Yellow);
                SuspectBlip.ScaleForPed();
                Entity.Disable(SuspectBlip);

                Suspect.Tasks.Wander();

                CalloutHandler();
            }
            catch(Exception e)
            {
                Log.CalloutException(this, "OnCalloutAccepted", e);
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
                switch (ScenarioNumber)
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
            #endregion
        }

        private void Scenario1() // Default
        {
            #region Scenario 1
            try
            {
            }
            catch (Exception e)
            {
                Log.CalloutException(this, "Scenario1", e);
            }
            #endregion
        }

        private void Scenario2() // Hostile
        {
            #region Scenario 2
            try
            {
                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 10f && MainPlayer.IsOnFoot)
                        {
                            // SuspectBlip Color Change
                            SuspectBlip.SetColor(Colors.Red);
                            Game.LogTrivial("[TRACE] Emergency Callouts: Changed SuspectBlip color to red");

                            // Start Fight
                            Suspect.Tasks.FightAgainst(MainPlayer);
                            Game.LogTrivial("[TRACE] Emergency Callouts: Assigned Suspect to fight player");
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

        private void Scenario3() // Passout
        {
            #region Scenario 3
            try
            {
                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 5f && MainPlayer.IsOnFoot && OnScene == true)
                        {
                            GameFiber.Sleep(1500);

                            // Kill Suspect
                            Entity.Kill(Suspect);
                            Game.LogTrivial("[TRACE] Emergency Callouts: Forcefully killed ped");

                            // Change Blip Color
                            SuspectBlip.SetColor(Colors.Green);
                            Game.LogTrivial("[TRACE] Emergency Callouts: Changed SuspectBlip color to green");
                            
                            // Display Unconsious Message
                            GameFiber.Sleep(1500);
                            Game.DisplayHelp("Person appears to be ~y~unconscious~s~.");

                            break;
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Log.CalloutException(this, "Scenario3", e);
            }
            #endregion
        }

        private void Scenario4() // Bottle
        {
            #region Scenario 4
            try
            {
                Suspect.Inventory.GiveNewWeapon("WEAPON_BOTTLE", -1, true);
                Game.LogTrivial("[TRACE] Emergency Callouts: Assigned WEAPON_BOTTLE to Suspect inventory");
            }
            catch (Exception e)
            {
                Log.CalloutException(this, "Scenario4", e);
            }
            #endregion
        }

        private void Scenario5() // Hostile w/ Bottle
        {
            #region Scenario 5
            try
            {
                Suspect.Inventory.GiveNewWeapon("WEAPON_BOTTLE", -1, true);
                Game.LogTrivial("[TRACE] Emergency Callouts: Added WEAPON_BOTTLE to Suspect inventory");

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 10f && MainPlayer.IsOnFoot)
                        {
                            // Change Blip Color
                            SuspectBlip.SetColor(Colors.Red);
                            Game.LogTrivial("[TRACE] Emergency Callouts: Changed SuspectBlip color to red");

                            // Start Fight
                            Suspect.Tasks.FightAgainst(MainPlayer);
                            Game.LogTrivial("[TRACE] Emergency Callouts: Assigned Suspect to fight player");
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
            try
            {
                Check.EndKeyDown();
                Check.NaturalEnding(Suspect);
                Check.PreventDistanceCrash(CalloutPosition, OnScene, PedFound);
                Check.PreventResponderCrash(Suspect);

                #region OnPlayerArrival
                if (MainPlayer.Position.DistanceTo(CalloutPosition) < 15f && OnScene == false)
                {
                    // Set OnScene
                    OnScene = true;
                    Game.LogTrivial("[TRACE] Emergency Callouts: Entered scene");

                    // Display Arriving Subtitle
                    Display.ArriveSubtitle("Find", "drunk person", 'y');
                    // Disable route
                    Entity.DisableRoute(EntranceBlip);
                    Game.LogTrivial("[TRACE] Emergency Callouts: Disabled route");

                    // Delete EntranceBlip
                    Entity.Delete(EntranceBlip);
                    Game.LogTrivial("[TRACE] Emergency Callouts: Deleted EntranceBlip");

                    // Create SearchArea
                    SearchArea = new Blip(Suspect.Position.Around(10f, 30f), 85f);
                    SearchArea.SetColor(Colors.Yellow);
                    SearchArea.Alpha = 0.5f;
                    Game.LogTrivial("[TRACE] Emergency Callouts: Created SearchArea");
                }
                #endregion

                #region OnPedFound
                if (MainPlayer.Position.DistanceTo(Suspect.Position) < 5f && PedFound == false && OnScene == true && Suspect.Exists())
                {
                    // Set PedFound
                    PedFound = true;
                    Game.LogTrivial("[TRACE] Emergency Callouts: Found Suspect");

                    // Hide Subtitle
                    Display.HideSubtitle();
                    Game.LogTrivial("[TRACE] Emergency Callouts: Hid subtitle");

                    // Enable SuspectBlip
                    Entity.Enable(SuspectBlip);
                    Game.LogTrivial("[TRACE] Emergency Callouts: Enabled SuspectBlip");

                    // Delete SearchArea
                    Entity.Delete(SearchArea);
                    Game.LogTrivial("[TRACE] Emergency Callouts: Deleted SearchArea");
                }
                #endregion

                #region OnPedDetained
                if (Suspect.IsDetained() == true && PedDetained == false && Suspect.Exists())
                {
                    // Set PedDetained
                    PedDetained = true;
                    Game.LogTrivial("[TRACE] Emergency Callouts: Suspect detained");

                    // Delete SuspectBlip
                    Entity.Delete(SuspectBlip);
                    Game.LogTrivial("[TRACE] Emergency Callouts: Deleted SuspectBlip");
                }
                #endregion

                #region OnPlayerLeave
                if (MainPlayer.Position.DistanceTo(CalloutPosition) > Settings.SearchAreaSize * 3f && OnScene == true)
                {
                    // Set OnScene
                    OnScene = false;
                    Game.LogTrivial("[TRACE] Emergency Callouts: Left scene");

                    // Disable SuspectBlip
                    Entity.Disable(SuspectBlip);
                    Game.LogTrivial("[TRACE] Emergency Callouts: Disabled SuspectBlip");

                    // Delete SearchArea
                    Entity.Delete(SearchArea);
                    Game.LogTrivial("[TRACE] Emergency Callouts: Deleted SearchArea");

                    // Create EntranceBlip
                    EntranceBlip = new Blip(CalloutPosition);
                    Game.LogTrivial("[TRACE] Emergency Callouts: Created EntranceBlip");

                    // Enable Route
                    Entity.EnableRoute(EntranceBlip);
                    Game.LogTrivial("[TRACE] Emergency Callouts: Enabled route to EntranceBlip.");
                }
                #endregion

                #region SearchAreaRefreshing
                if (PedFound == false)
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
                    Entity.Delete(SearchArea);

                    SearchArea = new Blip(Suspect.Position.Around(15f, Settings.SearchAreaSize), Settings.SearchAreaSize);
                    SearchArea.SetColor(Colors.Yellow);
                    SearchArea.Alpha = 0.5f;
                    Game.LogTrivial("[TRACE] Emergency Callouts: Created SearchArea");

                    Functions.PlayScannerAudioUsingPosition("SUSPECT IN_OR_ON_POSITION", Suspect.Position);
                }
                #endregion
            }
            catch (Exception e)
            {
                Log.CalloutException(this, "Process", e);
                End();
            }
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
            Log.CalloutEnded(CalloutMessage, ScenarioNumber);
        }
    }
}