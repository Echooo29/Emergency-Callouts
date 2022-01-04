using EmergencyCallouts.Essential;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Reflection;
using static EmergencyCallouts.Essential.Color;
using static EmergencyCallouts.Essential.Helper;
using static EmergencyCallouts.Essential.Inventory;

namespace EmergencyCallouts.Callouts
{
    [CalloutInfo("Burglary", CalloutProbability.Medium)]
    public class Burglary : Callout
    {
        bool CalloutActive;
        bool PlayerArrived;
        bool PedFound;
        bool PedDetained;
        bool StopChecking;

        Vector3 Entrance;
        Vector3 Center;

        // Main
        #region Positions
        readonly Vector3[] CalloutPositions =
        {
            new Vector3(916.261f, -623.7192f, 58.052020f),  // Mirror Park
            new Vector3(-663.6192f, -1358.232f, 10.4971f),  // La Puerta
            new Vector3(1300.166f, -1719.278f, 54.04285f),  // El Burro
            new Vector3(2652.853f, 4308.485f, 44.393880f),  // Grapeseed
            new Vector3(1207.165f, 2694.605f, 37.823690f),  // Harmony
            new Vector3(194.8364f, 6576.915f, 31.820280f),  // Paleto Bay
        };
        #endregion

        // Mirror Park
        #region Positions
        readonly Vector3[] MirrorParkBreakInPositions =
        {
            new Vector3(891.7003f, -625.6554f, 58.26049f), // Backdoor
            new Vector3(905.5065f, -632.9874f, 58.04898f), // Shed 1
            new Vector3(869.7964f, -607.5421f, 58.21951f), // Shed 2
        };

        readonly float[] MirrorParkBreakInHeadings =
        {
            322.62f,
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

        // El Burro
        #region Positions
        readonly Vector3[] ElBurroBreakInPositions =
        {
            new Vector3(1283.446f, -1699.925f, 55.47572f), // Back Door
            new Vector3(1295.854f, -1697.502f, 55.07866f), // Shed
            new Vector3(1267.995f, -1713.858f, 54.65507f), // Building 2 Back Door
        };

        readonly float[] ElBurroBreakInHeadings =
        {
            175.15f,
            285.86f,
            313.77f,
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

        // Harmony
        #region Positions
        readonly Vector3[] HarmonyBreakInPositions =
        {
            new Vector3(1194.485f, 2721.754f, 38.81226f), // Clothing Store
            new Vector3(1233.377f, 2737.641f, 38.0054f),  // RV Store
            new Vector3(1258.049f, 2740.197f, 38.70864f), // Abandoned Home
        };

        readonly float[] HarmonyBreakInHeadings =
        {
            209.57f,
            86.06f,
            342.01f,
        };
        #endregion

        // Paleto Bay
        #region Positions
        readonly Vector3[] PaletoBayBreakInPositions =
        {
            new Vector3(125.4187f, 6643.836f, 31.79918f), // Toilet Building
            new Vector3(174.3788f, 6642.977f, 31.57312f), // Don's Country Store
            new Vector3(156.7488f, 6657.068f, 31.56969f), // Pop's Pills
        };

        readonly float[] PaletoBayBreakInHeadings =
        {
            231.12f,
            138.59f,
            216.67f,
        };
        #endregion

        Vehicle SuspectVehicle;

        Ped Suspect;

        Persona SuspectPersona;

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
                    CalloutArea = World.GetStreetName(loc).Replace("Amarillo Vista", "Amarillo Vista"); ;
                }
            }

            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, Settings.SearchAreaSize / 2.5f);

            CalloutMessage = "Burglary";
            CalloutDetails = "A person has been seen looking through windows, caller states he's now lockpicking a door.";
            CalloutScenario = GetRandomScenarioNumber(5);

            Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT CRIME_BURGLARY IN_OR_ON_POSITION", CalloutPosition);

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
            try
            {
                // Callout Accepted
                Log.OnCalloutAccepted(CalloutMessage, CalloutScenario);

                // Accepting Messages
                Display.AcceptNotification(CalloutDetails);
                Display.AcceptSubtitle(CalloutMessage, CalloutArea);
                Display.OutdatedReminder();

                // EntranceBlip
                EntranceBlip = new Blip(Entrance);

                // Suspect
                Suspect = new Ped(CalloutPosition);
                SuspectPersona = Functions.GetPersonaForPed(Suspect);
                Suspect.SetDefaults();

                // SuspectBlip
                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColorRed();
                SuspectBlip.ScaleForPed();
                SuspectBlip.Disable();

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
                    Settings.SearchAreaSize += 80;
                }
                else if (CalloutPosition == CalloutPositions[2]) // El Burro
                {
                    Center = new Vector3(1281.405f, -1710.742f, 55.05928f);
                    Entrance = new Vector3(1300.166f, -1719.278f, 54.04285f);
                    EntranceBlip.Position = Entrance;
                    Settings.SearchAreaSize = 40;
                }
                else if (CalloutPosition == CalloutPositions[3]) // Grapeseed
                {
                    Center = new Vector3(2685.283f, 4256.731f, 45.41756f);
                    Entrance = new Vector3(2652.853f, 4308.485f, 44.39388f);
                    EntranceBlip.Position = Entrance;
                    Settings.SearchAreaSize = 85;
                }
                else if (CalloutPosition == CalloutPositions[4]) // Harmony
                {
                    Center = new Vector3(1223.067f, 2719.288f, 38.00484f);
                    Entrance = new Vector3(1207.165f, 2694.605f, 37.82369f);
                    EntranceBlip.Position = Entrance;
                }

                else if (CalloutPosition == CalloutPositions[5]) // Paleto Bay
                {
                    Center = new Vector3(126.4832f, 6640.071f, 31.81017f);
                    Entrance = new Vector3(194.8364f, 6576.915f, 31.82028f);
                    EntranceBlip.Position = Entrance;
                }
                #endregion

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
                    case 4:
                        Scenario4();
                        break;
                    case 5:
                        Scenario5();
                        break;
                }

                // Enabling Route
                EntranceBlip.EnableRoute();
                Game.LogTrivial("[Emergency Callouts]: Enabled route to EntranceBlip");
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
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
            else if (CalloutPosition == CalloutPositions[2]) // El Burro
            {
                int num = random.Next(ElBurroBreakInPositions.Length);
                Suspect.Position = ElBurroBreakInPositions[num];
                Suspect.Heading = ElBurroBreakInHeadings[num];
            }
            else if (CalloutPosition == CalloutPositions[3]) // Grapeseed
            {
                int num = random.Next(GrapeseedBreakInPositions.Length);
                Suspect.Position = GrapeseedBreakInPositions[num];
                Suspect.Heading = GrapeseedBreakInHeadings[num];
            }
            else if (CalloutPosition == CalloutPositions[4]) // Harmony
            {
                int num = random.Next(HarmonyBreakInPositions.Length);
                Suspect.Position = HarmonyBreakInPositions[num];
                Suspect.Heading = HarmonyBreakInHeadings[num];
            }
            else if (CalloutPosition == CalloutPositions[5]) // Paleto Bay
            {
                int num = random.Next(PaletoBayBreakInPositions.Length);
                Suspect.Position = PaletoBayBreakInPositions[num];
                Suspect.Heading = PaletoBayBreakInHeadings[num];
            }

            // Lockpick Animation
            Suspect.Tasks.PlayAnimation(new AnimationDictionary("mp_common_heist"), "pick_door", 5f, AnimationFlags.Loop);

            // Log Creation
            Log.Creation(Suspect, PedCategory.Suspect);
            #endregion
        }

        private void RetrieveVehiclePositions()
        {
            #region Positions
            SuspectVehicle = new Vehicle(Vehicles.GetRandomVan(), CalloutPosition, 0f);
            SuspectVehicle.IsPersistent = true;
            Log.Creation(SuspectVehicle, PedCategory.Suspect);

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
            else if (CalloutPosition == CalloutPositions[2]) // El Burro
            {
                SuspectVehicle.Position = new Vector3(1308.245f, -1716.298f, 54.03547f);
                SuspectVehicle.Heading = 296.54f;
            }
            else if (CalloutPosition == CalloutPositions[3]) // Grapeseed
            {
                SuspectVehicle.Position = new Vector3(2716.37f, 4263.91f, 46.86611f);
                SuspectVehicle.Heading = 166.61f;
            }
            else if (CalloutPosition == CalloutPositions[4]) // Harmony
            {
                SuspectVehicle.Position = new Vector3(1234.011f, 2722.458f, 38.02638f);
                SuspectVehicle.Heading = 137.17f;
            }
            else if (CalloutPosition == CalloutPositions[5]) // Paleto Bay
            {
                SuspectVehicle.Position = new Vector3(130.7242f, 6666.58f, 31.65008f);
                SuspectVehicle.Heading = 158.69f;
            }

            VehicleDoor[] vehDoors = SuspectVehicle.GetDoors();
            vehDoors[vehDoors.Length - 2].Open(false);
            vehDoors[vehDoors.Length - 1].Open(false);
            #endregion
        }

        private void Scenario1() // Attack
        {
            #region Scenario 1
            try
            {
                // Retrieve Ped Position
                RetrievePedPositions();

                int num = random.Next(2);
                if (num == 0)
                {
                    Suspect.GiveRandomMeleeWeapon(-1, true);
                }
                else Suspect.GiveRandomHandgun(-1, true);

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 10f && Suspect.Exists() && PlayerArrived)
                        {
                            Suspect.Tasks.FightAgainst(MainPlayer);
                            break;
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
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

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 10f && Suspect.Exists() && PlayerArrived)
                        {
                            StopChecking = true;

                            // Delete Blips
                            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                            if (SearchArea.Exists()) { SearchArea.Delete(); }
                            if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

                            // Create Pursuit
                            LHandle pursuit = Functions.CreatePursuit();

                            // Add Suspect To Pursuit
                            Functions.AddPedToPursuit(pursuit, Suspect);

                            // Set Pursuit Active
                            Functions.SetPursuitIsActiveForPlayer(pursuit, true);

                            // Play Pursuit Audio
                            Play.PursuitAudio();

                            break;
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
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

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();
                        
                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 10f && Suspect.Exists() && PlayerArrived)
                        {
                            // Put Suspect's Hands up
                            Suspect.Tasks.PutHandsUp(-1, MainPlayer);

                            break;
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
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

                // Give Weapon
                Suspect.Inventory.GiveNewWeapon("WEAPON_CROWBAR", -1, true);

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 10f && Suspect.Exists() && PlayerArrived)
                        {
                            Suspect.Tasks.FightAgainst(MainPlayer);
                            break;
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
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

                // Suspect Resistance Chance
                Functions.SetPedResistanceChance(Suspect, 40f);

                // Suspect Position
                Suspect.Position = SuspectVehicle.GetOffsetPositionFront(-SuspectVehicle.Length + 1.9f);

                // Suspect Heading
                Suspect.Heading = SuspectVehicle.Heading;

                // Search Animation
                Suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@gangops@facility@servers@bodysearch@"), "player_search", 5f, AnimationFlags.UpperBodyOnly | AnimationFlags.Loop);

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 15f && Suspect.Exists() && PlayerArrived)
                        {
                            StopChecking = true;

                            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                            if (SearchArea.Exists()) { SearchArea.Delete(); }
                            if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

                            // Create Pursuit
                            LHandle pursuit = Functions.CreatePursuit();

                            // Add Suspect to pursuit
                            Functions.AddPedToPursuit(pursuit, Suspect);

                            // Set Pursuit Active
                            Functions.SetPursuitIsActiveForPlayer(pursuit, true);

                            // Play pursuit audio
                            Play.PursuitAudio();

                            break;
                        }
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
                Handle.AutomaticEnding(Suspect);
                Handle.PreventDistanceCrash(CalloutPosition, PlayerArrived, PedFound);
                Handle.PreventFirstResponderCrash(Suspect);

                #region PlayerArrived
                if (MainPlayer.Position.DistanceTo(Entrance) < 15f && !PlayerArrived)
                {
                    // Set PlayerArrived
                    PlayerArrived = true;

                    // Delete Nearby Peds
                    Handle.DeleteNearbyPeds(Suspect);

                    // Display Arriving Subtitle
                    Game.DisplaySubtitle("Find the ~r~burglar~s~ in the ~y~area~s~.", 20000);

                    // Disable route
                    EntranceBlip.DisableRoute();

                    // Delete EntranceBlip
                    if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

                    // Create SearchArea
                    SearchArea = new Blip(Center, Settings.SearchAreaSize);
                    SearchArea.SetColorYellow();
                    SearchArea.Alpha = 0.5f;

                    // SpookCheck
                    Handle.SpookCheck(Entrance, 10f);
                    
                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has arrived on scene");
                }
                #endregion

                #region PedFound
                if (MainPlayer.Position.DistanceTo(Suspect.Position) < 5f && !PedFound && PlayerArrived && Suspect.Exists())
                {
                    // Set PedFound
                    PedFound = true;

                    // Hide Subtitle
                    Display.HideSubtitle();

                    // Enable SuspectBlip
                    SuspectBlip.Enable();

                    // Delete SearchArea
                    if (SearchArea.Exists()) { SearchArea.Delete(); }

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has found {SuspectPersona.FullName} (Suspect)");
                }
                #endregion

                #region PedDetained
                if (Suspect.IsPedDetained() && !PedDetained && Suspect.Exists())
                {
                    // Set PedDetained
                    PedDetained = true;
                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has detained {SuspectPersona.FullName} (Suspect)");

                    // Delete SuspectBlip
                    if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                    Game.LogTrivial("[Emergency Callouts]: Deleted SuspectBlip");
                }
                #endregion

                #region PlayerLeft
                if (MainPlayer.Position.DistanceTo(CalloutPosition) > Settings.SearchAreaSize * 3.5f && PlayerArrived && !StopChecking)
                {
                    // Set PlayerArrived
                    PlayerArrived = false;

                    // Disable SuspectBlip
                    SuspectBlip.Disable();

                    // Delete SearchArea
                    if (SearchArea.Exists()) { SearchArea.Delete(); }

                    // Create EntranceBlip
                    EntranceBlip = new Blip(Entrance);

                    // Enable Route
                    EntranceBlip.EnableRoute();

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has left the scene");
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
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            if (SearchArea.Exists()) { SearchArea.Delete(); }
            if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

            Display.HideSubtitle();
            Display.EndNotification();
            Log.OnCalloutEnded(CalloutMessage, CalloutScenario);
        }
    }
}