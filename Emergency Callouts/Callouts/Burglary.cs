using EmergencyCallouts.Essential;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Engine.UI;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using static EmergencyCallouts.Essential.Color;
using static EmergencyCallouts.Essential.Helper;
using Entity = EmergencyCallouts.Essential.Helper.Entity;

namespace EmergencyCallouts.Callouts
{
    [CalloutInfo("Burglary", CalloutProbability.Medium)]
    public class Burglary : Callout
    {
        readonly int ScenarioNumber = random.Next(1, 6);

        bool CalloutActive;
        bool PlayerArrived;
        bool PedFound;
        bool PedDetained;

        // Main
        #region Positions
        Vector3 Entrance;
        Vector3 Center;

        // CalloutPositions (Entrance)
        readonly Vector3[] CalloutPositions =
        {
            new Vector3(916.261f, -623.7192f, 58.05202f), // Mirror Park
            new Vector3(-663.6192f, -1358.232f, 10.49708f), // La Puerta
            new Vector3(2652.853f, 4308.485f, 44.39388f), // Grapeseed
        };
        #endregion

        // Mirror Park
        #region Positions
        readonly Vector3[] MirrorParkBreakInPositions =
        {
            new Vector3(891.6117f, -625.4667f, 58.26054f), // Backdoor
            new Vector3(905.5065f, -632.9874f, 58.04898f), // Shed 1
            new Vector3(869.7964f, -607.5421f, 58.21951f), // Shed 2
        };

        readonly float[] MirrorParkBreakInHeadings =
        {
            295f,
            212f,
            39.6f,
        };
        #endregion

        // La Puerta
        #region Positions
        readonly Vector3[] LaPuertaBreakInPositions =
        {
            new Vector3(-759.6483f, -1515.452f, 4.976925f), // Building Door
            new Vector3(-721.8896f, -1513.393f, 5.000525f), // Building Door 2
        };

        readonly float[] LaPuertaBreakInHeadings =
        {
            174.41f,
            104.47f,
        };
        #endregion

        // Grapeseed
        #region Positions
        readonly Vector3[] GrapeseedBreakInPositions =
        {
            new Vector3(2641.462f, 4235.202f, 45.49297f), // Small House
            new Vector3(2709.033f, 4316.569f, 46.15852f), // Small House 2
            new Vector3(2736.02f, 4279.527f, 48.49361f),  // Connected Trailers
        };

        readonly float[] GrapeseedBreakInHeadings =
        {
            51f,
            83f,
            283.19f,
        };
        #endregion

        Vehicle SuspectVehicle;

        Ped Suspect;

        Blip SuspectBlip;
        Blip EntranceBlip;
        Blip SearchArea;

        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutPosition = new Vector3(0, 0, 3000);
            foreach (Vector3 loc in CalloutPositions)
            {
                if (Vector3.Distance(MainPlayer.Position, loc) < Vector3.Distance(MainPlayer.Position, CalloutPosition))
                {
                    CalloutPosition = loc;
                }
            }

            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, Settings.SearchAreaSize / 2.5f);

            CalloutMessage = "Burglary";

            Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT CRIME_BURGLARY IN_OR_ON_POSITION", CalloutPosition);

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
                Display.AttachMessage("A person has been seen looking through windows, caller states he's now lockpicking a door.");

                // EntranceBlip
                EntranceBlip = new Blip(Entrance);
                Game.LogTrivial("[Emergency Callouts]: Created EntranceBlip");

                // Suspect
                Suspect = new Ped(CalloutPosition);
                Suspect.SetDefaults();
                Game.LogTrivial($"[Emergency Callouts]: Created Suspect ({Suspect.Model.Name}) at " + Suspect.Position);

                // SuspectBlip
                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColor(Colors.Red);
                SuspectBlip.ScaleForPed();
                Entity.Disable(SuspectBlip);

                CalloutHandler();
            }
            catch (Exception e)
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

                // Positioning
                #region Positioning
                if (CalloutPosition == CalloutPositions[0]) // Mirror Park
                {
                    Center = new Vector3(888.6841f, -625.1655f, 58.04898f);
                    Entrance = new Vector3(916.261f, -623.7192f, 58.05202f);
                    EntranceBlip.Position = Entrance;
                    Settings.SearchAreaSize = 40;
                }
                else if (CalloutPosition == CalloutPositions[1]) // La Puerta
                {
                    Center = new Vector3(-741.3954f, -1453.013f, 5.000523f);
                    Entrance = new Vector3(-663.6192f, -1358.232f, 10.49708f);
                    EntranceBlip.Position = Entrance;
                }
                else if (CalloutPosition == CalloutPositions[2]) // Grapeseed
                {
                    Center = new Vector3(2685.283f, 4256.731f, 45.41756f);
                    Entrance = new Vector3(2652.853f, 4308.485f, 44.39388f);
                    EntranceBlip.Position = Entrance;
                    Settings.SearchAreaSize = 85;
                }
                #endregion

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

                // Enabling Route
                Entity.EnableRoute(EntranceBlip);
                Game.LogTrivial("[Emergency Callouts]: Enabled route to EntranceBlip");

            }
            catch (Exception e)
            {
                Log.CalloutException(this, "CalloutHandler", e);
            }
            #endregion
        }

        private void RetrievePedPositions()
        {
            #region Positions
            if (CalloutPosition == CalloutPositions[0]) // Mirror Park
            {
                int num = random.Next(MirrorParkBreakInPositions.Length);
                Suspect.Position = MirrorParkBreakInPositions[num];
                Suspect.Heading = MirrorParkBreakInHeadings[num];
            }
            else if (CalloutPosition == CalloutPositions[1]) // La Puerta
            {
                int num = random.Next(LaPuertaBreakInPositions.Length);
                Suspect.Position = LaPuertaBreakInPositions[num];
                Suspect.Heading = LaPuertaBreakInHeadings[num];
            }
            else if (CalloutPosition == CalloutPositions[2]) // Grapeseed
            {
                int num = random.Next(GrapeseedBreakInPositions.Length);
                Suspect.Position = GrapeseedBreakInPositions[num];
                Suspect.Heading = GrapeseedBreakInHeadings[num];
            }

            // Lockpick Animation
            Suspect.Tasks.PlayAnimation(new AnimationDictionary("mp_common_heist"), "pick_door", 5f, AnimationFlags.Loop);
            Game.LogTrivial("[Emergency Callouts]: Assigned Suspect to play animation");

            #endregion
        }

        private void RetrieveVehiclePositions()
        {
            #region Positions
            SuspectVehicle = new Vehicle(Vehicles.GetRandomVan(), CalloutPosition, 0f);
            SuspectVehicle.IsPersistent = true;
            
            if (CalloutPosition == CalloutPositions[0]) // Mirror Park
            {
                SuspectVehicle.Position = new Vector3(909.9557f, -624.8691f, 57.66842f);
                SuspectVehicle.Heading = 318.71f;
            }
            else if (CalloutPosition == CalloutPositions[1]) // La Puerta
            {
                SuspectVehicle.Position = new Vector3(-723.9453f, -1491.7f, 4.61949f);
                SuspectVehicle.Heading = 347.67f;
            }
            else if (CalloutPosition == CalloutPositions[2]) // Grapeseed
            {
                SuspectVehicle.Position = new Vector3(2716.37f, 4263.91f, 46.86611f);
                SuspectVehicle.Heading = 166.61f;
            }

            Game.LogTrivial($"[Emergency Callouts]: Created SuspectVehicle ({SuspectVehicle.Model.Name}) at " + SuspectVehicle.Position);

            VehicleDoor[] vehDoors = SuspectVehicle.GetDoors();
            vehDoors[vehDoors.Length - 2].Open(false);
            #endregion
        }

        private void Scenario1() // Unnoticed
        {
            #region Scenario 1
            try
            {
                // Retrieve Ped Position
                RetrievePedPositions();
                Game.LogTrivial("[Emergency Callouts]: Retrieved ped position");
            }
            catch (Exception e)
            {
                Log.CalloutException(this, "Scenario1", e);
            }
            #endregion
        }
        
        private void Scenario2() // Pursuit
        {
            #region Scenario 2
            try
            {
                // Retrieve Ped Positions
                RetrievePedPositions();
                Game.LogTrivial("[Emergency Callouts]: Retrieved ped position");

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (PedFound == true)
                        {
                            // Delete SuspectBlip
                            Entity.Delete(SuspectBlip);
                            Game.LogTrivial("[Emergency Callouts]: Deleted SuspectBlip");

                            // Create Pursuit
                            LHandle pursuit = Functions.CreatePursuit();
                            Game.LogTrivial("[Emergency Callouts]: Created pursuit");

                            // Add Suspect To Pursuit
                            Functions.AddPedToPursuit(pursuit, Suspect);
                            Game.LogTrivial("[Emergency Callouts]: Added Suspect to pursuit");

                            // Set Pursuit Active
                            Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                            Game.LogTrivial("[Emergency Callouts]: Set pursuit active for player");

                            // Play Pursuit Audio
                            Play.PursuitAudio();
                            Game.LogTrivial("[Emergency Callouts]: Played pursuit audio");

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

        private void Scenario3() // Surrender
        {
            #region Scenario 3
            try
            {
                // Retrieve Ped Positions
                RetrievePedPositions();
                Game.LogTrivial("[Emergency Callouts]: Retrieved ped position");

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();
                        
                        if (PedFound == true)
                        {
                            // Put Suspect's Hands up
                            Suspect.Tasks.PutHandsUp(-1, MainPlayer);
                            Game.LogTrivial("[Emergency Callouts]: Assigned Suspect to put hands up");

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

        private void Scenario4() // Attack
        {
            #region Scenario 4
            try
            {
                // Retrieve Ped Positions
                RetrievePedPositions();
                Game.LogTrivial("[Emergency Callouts]: Retrieved ped position");

                // Give Weapon
                Suspect.Inventory.GiveNewWeapon("WEAPON_CROWBAR", -1, true);
                Game.LogTrivial($"[Emergency Callouts]: Assigned (WEAPON_CROWBAR) to Suspect inventory");

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (PedFound == true)
                        {
                            Suspect.Tasks.FightAgainst(MainPlayer);
                            Game.LogTrivial("[Emergency Callouts]: Assigned Suspect to fight player");
                            break;
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Log.CalloutException(this, "Scenario4", e);
            }
            #endregion
        }

        private void Scenario5() // Putting Items In Vehicle
        {
            #region Scenario 5
            try
            {
                // Retrieve Vehicle Positions
                RetrieveVehiclePositions();
                Game.LogTrivial("[Emergency Callouts]: Retrieved vehicle position");

                // Suspect Resistance Chance
                Functions.SetPedResistanceChance(Suspect, 40f);
                Game.LogTrivial("[Emergency Callouts]: Set ped resistance chance to 40%");

                // Suspect Position
                Suspect.Position = SuspectVehicle.GetOffsetPositionFront(-SuspectVehicle.Length + 1.9f);
                Game.LogTrivial("[Emergency Callouts]: Changed Suspect position");

                // Suspect Heading
                Suspect.Heading = SuspectVehicle.Heading;
                Game.LogTrivial("[Emergency Callouts]: Changed Suspect heading");

                // Search Animation
                Suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@gangops@facility@servers@bodysearch@"), "player_search", 5f, AnimationFlags.UpperBodyOnly | AnimationFlags.Loop);
                Game.LogTrivial("[Emergency Callouts]: Assigned Suspect to play animation");

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 15f && Suspect.Exists())
                        {
                            // Delete SuspectBlip
                            Entity.Delete(SuspectBlip);
                            Game.LogTrivial("[Emergency Callouts]: Deleted SuspectBlip");

                            // Create Pursuit
                            LHandle pursuit = Functions.CreatePursuit();
                            Game.LogTrivial("[Emergency Callouts]: Created pursuit");

                            // Add Suspect to pursuit
                            Functions.AddPedToPursuit(pursuit, Suspect);
                            Game.LogTrivial("[Emergency Callouts]: Added Suspect to pursuit");

                            // Set Pursuit Active
                            Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                            Game.LogTrivial("[Emergency Callouts]: Set pursuit is active for player");

                            // Play pursuit audio
                            Play.PursuitAudio();
                            Game.LogTrivial("[Emergency Callouts]: Played pursuit audio");

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
                Check.PreventResponderCrash(Suspect, CalloutMessage);

                #region PlayerArrived
                if (MainPlayer.Position.DistanceTo(Entrance) < 15f && PlayerArrived == false)
                {
                    // Set PlayerArrived
                    PlayerArrived = true;

                    // Display Arriving Subtitle
                    Display.ArriveSubtitle("Find", "burglar", 'r');

                    // Disable route
                    Entity.DisableRoute(EntranceBlip);

                    // Delete EntranceBlip
                    Entity.Delete(EntranceBlip);

                    // Create SearchArea
                    SearchArea = new Blip(Center, 85f);
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
                    EntranceBlip = new Blip(Entrance);

                    // Enable Route
                    Entity.EnableRoute(EntranceBlip);

                    Game.LogTrivial("[Emergency Callouts]: Player left callout position");
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