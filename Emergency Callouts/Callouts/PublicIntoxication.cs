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
        bool PlayerArrived;
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

            CalloutMessage = "Public Intoxication";

            Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT CRIME_DISTURBING_THE_PEACE_01 IN_OR_ON_POSITION UNITS_RESPOND_CODE_02", CalloutPosition);

            return base.OnBeforeCalloutDisplayed();
        }

        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("[Emergency Callouts]: Callout not accepted");
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
                Display.AttachMessage("There are multiple reports of a person under the influence of alcohol.");

                // EntranceBlip
                EntranceBlip = new Blip(CalloutPosition);
                Game.LogTrivial("[Emergency Callouts]: Created EntranceBlip");

                Entity.EnableRoute(EntranceBlip);
                Game.LogTrivial("[Emergency Callouts]: Enabled route to CalloutPosition");

                // Suspect
                Suspect = new Ped(CalloutPosition);
                Suspect.SetDefaults();
                Suspect.SetIntoxicated();
                Game.LogTrivial($"[Emergency Callouts]: Created Suspect ({Suspect.Model.Name}) at " + Suspect.Position);

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
                            Game.LogTrivial("[Emergency Callouts]: Changed SuspectBlip color to red");

                            // Start Fight
                            Suspect.Tasks.FightAgainst(MainPlayer);
                            Game.LogTrivial("[Emergency Callouts]: Assigned Suspect to fight player");
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

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 5f && MainPlayer.IsOnFoot && PlayerArrived == true)
                        {
                            GameFiber.Sleep(1500);

                            // Kill Suspect
                            Entity.Kill(Suspect);
                            Game.LogTrivial("[Emergency Callouts]: Forcefully killed ped");

                            // Change Blip Color
                            SuspectBlip.SetColor(Colors.Green);
                            Game.LogTrivial("[Emergency Callouts]: Changed SuspectBlip color to green");
                            
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
                Game.LogTrivial("[Emergency Callouts]: Assigned WEAPON_BOTTLE to Suspect inventory");
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
                Game.LogTrivial("[Emergency Callouts]: Added WEAPON_BOTTLE to Suspect inventory");

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 10f && MainPlayer.IsOnFoot)
                        {
                            // Change Blip Color
                            SuspectBlip.SetColor(Colors.Red);
                            Game.LogTrivial("[Emergency Callouts]: Changed SuspectBlip color to red");

                            // Start Fight
                            Suspect.Tasks.FightAgainst(MainPlayer);
                            Game.LogTrivial("[Emergency Callouts]: Assigned Suspect to fight player");
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
                Check.PreventDistanceCrash(CalloutPosition, PlayerArrived, PedFound);
                Check.PreventResponderCrash(Suspect);

                #region PlayerArrived
                if (MainPlayer.Position.DistanceTo(CalloutPosition) < 15f && PlayerArrived == false)
                {
                    // Set PlayerArrived
                    PlayerArrived = true;

                    // Display Arriving Subtitle
                    Display.ArriveSubtitle("Find", "drunk person", 'y');

                    // Disable route
                    Entity.DisableRoute(EntranceBlip);

                    // Delete EntranceBlip
                    Entity.Delete(EntranceBlip);

                    // Create SearchArea
                    SearchArea = new Blip(CalloutPosition, 85f);
                    SearchArea.SetColor(Colors.Yellow);
                    SearchArea.Alpha = 0.5f;

                    Game.LogTrivial("[Emergency Callouts]: Player arrived on scene");
                }
                #endregion

                #region PedFound
                if (MainPlayer.Position.DistanceTo(Suspect.Position) < 5f && PedFound == false && PlayerArrived == true && Suspect.Exists())
                {
                    // Set PedFound
                    PedFound = true;

                    // Hide Subtitle
                    Display.HideSubtitle();

                    // Enable SuspectBlip
                    Entity.Enable(SuspectBlip);

                    // Delete SearchArea
                    Entity.Delete(SearchArea);

                    Game.LogTrivial("[Emergency Callouts]: Player found ped");
                }
                #endregion

                #region PedDetained
                if (Suspect.IsDetained() == true && PedDetained == false && Suspect.Exists())
                {
                    // Set PedDetained
                    PedDetained = true;
                    Game.LogTrivial("[Emergency Callouts]: Suspect detained");

                    // Delete SuspectBlip
                    Entity.Delete(SuspectBlip);
                    Game.LogTrivial("[Emergency Callouts]: Deleted SuspectBlip");
                }
                #endregion

                #region PlayerLeft
                if (MainPlayer.Position.DistanceTo(CalloutPosition) > Settings.SearchAreaSize * 3.5f && PlayerArrived == true)
                {
                    // Set PlayerArrived
                    PlayerArrived = false;

                    // Disable SuspectBlip
                    Entity.Disable(SuspectBlip);

                    // Delete SearchArea
                    Entity.Delete(SearchArea);

                    // Create EntranceBlip
                    EntranceBlip = new Blip(CalloutPosition);

                    // Enable Route
                    Entity.EnableRoute(EntranceBlip);

                    Game.LogTrivial("[Emergency Callouts]: Player left callout position");
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
                    Game.LogTrivial("[Emergency Callouts]: Created SearchArea");

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