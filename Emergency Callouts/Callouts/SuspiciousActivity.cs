using EmergencyCallouts.Essential;
using LSPD_First_Response.Engine.UI;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using static EmergencyCallouts.Essential.Helper;
using static EmergencyCallouts.Essential.Inventory;
using Entity = EmergencyCallouts.Essential.Helper.Entity;

namespace EmergencyCallouts.Callouts
{
    [CalloutInfo("Suspicious Activity", CalloutProbability.Medium)]
    public class SuspiciousActivity : Callout
    {
        readonly int ScenarioNumber = random.Next(1, 6);

        bool CalloutActive;
        bool OnScene;
        bool FoundPed;
        bool FoundPed2;
        bool PedDetained;
        bool PedArrested;
        bool DialogueStarted;

        // Main
        #region Positions

        Vector3 Entrance;
        Vector3 Center;

        readonly Vector3[] CalloutPositions =
        {
            new Vector3(167.0673f, -1247.618f, 29.19848f), // Strawberry
            new Vector3(-1283.511f, -811.2982f, 17.32025f), // Del Perro
            new Vector3(651.5822f, 2762.731f, 41.94574f), // Harmony
        };
        #endregion

        // Strawberry
        #region Positions
        readonly Vector3 StrawberrySuspectPosition = new Vector3(151.7143f, -1262.308f, 29.31358f);
        readonly float StrawberrySuspectHeading = 25.75f;

        readonly Vector3 StrawberrySuspect2Position = new Vector3(150.6859f, -1259.686f, 29.29234f);
        readonly float StrawberrySuspect2Heading = 212f;

        readonly Vector3 StrawberryVehiclePosition = new Vector3(151.4726f, -1264.789f, 28.93705f);
        readonly float StrawberryVehicleHeading = 209f;

        readonly Vector3 StrawberryVehicle2Position = new Vector3(149.1889f, -1259.119f, 28.89105f);
        readonly float StrawberryVehicle2Heading = 116f;
        #endregion

        // Del Perro
        #region Positions
        readonly Vector3 DelPerroSuspectPosition = new Vector3(-1260.832f, -826.6248f, 17.0973f);
        readonly float DelPerroSuspectHeading = 25.81f;

        readonly Vector3 DelPerroSuspect2Position = new Vector3(-1261.028f, -824.2448f, 17.09965f);
        readonly float DelPerroSuspect2Heading = 179.82f;

        readonly Vector3 DelPerroVehiclePosition = new Vector3(-1264.427f, -818.228f, 16.62829f);
        readonly float DelPerroVehicleHeading = 35.55f;

        readonly Vector3 DelPerroVehicle2Position = new Vector3(-1269.506f, -824.8015f,16.2714f);
        readonly float DelPerroVehicle2Heading = 129.07f;
        #endregion

        // Harmony
        #region Positions
        readonly Vector3 Location3SuspectPosition = new Vector3(604.8357f, 2789.544f, 42.1919f);
        readonly float Location3SuspectHeading = 4.81f;

        readonly Vector3 Location3Suspect2Position = new Vector3(604.5134f, 2792.431f, 42.14416f);
        readonly float Location3Suspect2Heading = 187.87f;

        readonly Vector3 Location3VehiclePosition = new Vector3(606.4803f, 2791.021f, 41.69831f);
        readonly float Location3VehicleHeading = 6.87f;

        readonly Vector3 Location3Vehicle2Position = new Vector3(602.7837f, 2790.826f, 41.7882f);
        readonly float Location3Vehicle2Heading = 7.75f;
        #endregion

        Vehicle SuspectVehicle;
        Vehicle Suspect2Vehicle;

        VehicleDoor[] vehDoors;
        VehicleDoor[] veh2Doors;

        Ped Suspect;
        Ped Suspect2;

        Blip SuspectBlip;
        Blip Suspect2Blip;
        Blip EntranceBlip;
        Blip SearchArea;

        LHandle pursuit;

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

            CalloutMessage = Settings.SuspiciousActivityName;

            Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT CRIME_SUSPICIOUS_ACTIVITY IN_OR_ON_POSITION", CalloutPosition);

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
                Display.CalloutDetails(Settings.SuspiciousActivityDetails);

                // EntranceBlip
                EntranceBlip = new Blip(Entrance);

                // Suspect
                Suspect = new Ped(Entity.GetRandomMaleModel(), CalloutPosition, 0f);
                Suspect.SetDefaults();
                Game.LogTrivial($"[TRACE] Emergency Callouts: Created Suspect ({Suspect.Model.Name}) at " + Suspect.Position);

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.Color = Colors.Yellow;
                SuspectBlip.ScaleForPed();
                Entity.Disable(SuspectBlip);

                // Suspect 2
                Suspect2 = new Ped(Entity.GetRandomMaleModel(), CalloutPosition, 0f);
                Suspect2.SetDefaults();
                Game.LogTrivial($"[TRACE] Emergency Callouts: Created Suspect2 ({Suspect2.Model.Name}) at " + Suspect2.Position);

                Suspect2Blip = Suspect2.AttachBlip();
                Suspect2Blip.Color = Colors.Yellow;
                Suspect2Blip.ScaleForPed();
                Entity.Disable(Suspect2Blip);

                // SuspectVehicle
                SuspectVehicle = new Vehicle(Vehicles.GetRandomFourDoor(), CalloutPosition, 0f);
                SuspectVehicle.IsPersistent = true;
                Game.LogTrivial($"[TRACE] Emergency Callouts: Created SuspectVehicle ({SuspectVehicle.Model.Name}) at " + SuspectVehicle.Position);

                vehDoors = SuspectVehicle.GetDoors();
                vehDoors[vehDoors.Length - 1].Open(false);

                // Suspect2Vehicle
                Suspect2Vehicle = new Vehicle(Vehicles.GetRandomFourDoor(), CalloutPosition, 0f);
                Suspect2Vehicle.IsPersistent = true;
                Game.LogTrivial($"[TRACE] Emergency Callouts: Created Suspect2Vehicle ({Suspect2Vehicle.Model.Name}) at " + Suspect2Vehicle.Position);

                veh2Doors = Suspect2Vehicle.GetDoors();
                veh2Doors[veh2Doors.Length - 1].Open(false);

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
                if (CalloutPosition == CalloutPositions[0]) // Strawberry
                {
                    Center = new Vector3(167.0673f, -1247.618f, 29.19848f);
                    Entrance = new Vector3(207.6943f, -1261.656f, 29.16432f);
                    EntranceBlip.Position = Entrance;
                }
                else if (CalloutPosition == CalloutPositions[1]) // Del Perro
                {
                    Center = new Vector3(-1283.511f, -811.2982f, 17.32025f);
                    Entrance = new Vector3(-1364.522f, -709.0762f, 24.67615f);
                    EntranceBlip.Position = Entrance;
                }
                else if (CalloutPosition == CalloutPositions[2]) // Harmony
                {
                    Center = new Vector3(597.8428f, 2796.708f, 41.99812f);
                    Entrance = new Vector3(651.5822f, 2762.731f, 41.94574f);
                    EntranceBlip.Position = Entrance;
                }
                #endregion

                // Scenario Deciding
                switch (ScenarioNumber)
                {
                    case 1:
                        Scenario5();
                        break;
                    case 2:
                        Scenario5();
                        break;
                    case 3:
                        Scenario5();
                        break;
                    case 4:
                        Scenario5();
                        break;
                    case 5:
                        Scenario5();
                        break;
                }

                // Enabling Route
                Entity.EnableRoute(EntranceBlip);
                Game.LogTrivial("[TRACE] Emergency Callouts: Enabled route to EntranceBlip");
            }
            catch (Exception e)
            {
                Log.CalloutException(this, "CalloutHandler", e);
            }
            #endregion
        }

        private void RetrievePedPosition()
        {
            #region Positions
            if (CalloutPosition == CalloutPositions[0]) // Strawberry
            {
                Suspect.Position = StrawberrySuspectPosition;
                Suspect.Heading = StrawberrySuspectHeading;

                Suspect2.Position = StrawberrySuspect2Position;
                Suspect2.Heading = StrawberrySuspect2Heading;

                SuspectVehicle.Position = StrawberryVehiclePosition;
                SuspectVehicle.Heading = StrawberryVehicleHeading;

                Suspect2Vehicle.Position = StrawberryVehicle2Position;
                Suspect2Vehicle.Heading = StrawberryVehicle2Heading;
            }
            else if (CalloutPosition == CalloutPositions[1]) // Del Perro
            {
                Suspect.Position = DelPerroSuspectPosition;
                Suspect.Heading = DelPerroSuspectHeading;

                Suspect2.Position = DelPerroSuspect2Position;
                Suspect2.Heading = DelPerroSuspect2Heading;

                SuspectVehicle.Position = DelPerroVehiclePosition;
                SuspectVehicle.Heading = DelPerroVehicleHeading;

                Suspect2Vehicle.Position = DelPerroVehicle2Position;
                Suspect2Vehicle.Heading = DelPerroVehicle2Heading;
            }
            else if (CalloutPosition == CalloutPositions[2]) // Harmony
            {
                Suspect.Position = Location3SuspectPosition;
                Suspect.Heading = Location3SuspectHeading;

                Suspect2.Position = Location3Suspect2Position;
                Suspect2.Heading = Location3Suspect2Heading;

                SuspectVehicle.Position = Location3VehiclePosition;
                SuspectVehicle.Heading = Location3VehicleHeading;

                Suspect2Vehicle.Position = Location3Vehicle2Position;
                Suspect2Vehicle.Heading = Location3Vehicle2Heading;
            }
            #endregion
        }

        private void RetrieveDrugDealPosition()
        {
            #region Positions
            if (CalloutPosition == CalloutPositions[0]) // Strawberry
            {
                SuspectVehicle.Position = new Vector3(153.1114f, -1236.517f, 28.82794f);
                Suspect2Vehicle.Position = new Vector3(154.2649f, -1238.243f, 28.90071f);

                SuspectVehicle.Heading = 127.4f;
                Suspect2Vehicle.Heading = 308.8f;
            }
            else if (CalloutPosition == CalloutPositions[1]) // Del Perro
            {
                SuspectVehicle.Position = new Vector3(-1281.334f, -817.5996f, 16.65075f);
                Suspect2Vehicle.Position = new Vector3(-1279.269f, -816.8776f, 16.73894f);

                SuspectVehicle.Heading = 205.01f;
                Suspect2Vehicle.Heading = 24.12f;
            }
            else if (CalloutPosition == CalloutPositions[2]) // Harmony
            {
                SuspectVehicle.Position = new Vector3(592.0656f, 2804.81f, 41.53365f);
                Suspect2Vehicle.Position = new Vector3(595f, 2804.858f, 41.5457f);

                SuspectVehicle.Heading = 186.73f;
                Suspect2Vehicle.Heading = 6.83f;
            }

            vehDoors = SuspectVehicle.GetDoors();
            veh2Doors = Suspect2Vehicle.GetDoors();

            vehDoors[vehDoors.Length - 1].Close(false);
            veh2Doors[veh2Doors.Length - 1].Close(false);
            #endregion
        }

        private void Scenario1() // Fight And Flee
        {
            #region Scenario 1
            try
            {
                RetrievePedPosition();
                Game.LogTrivial("[TRACE] Emergency Callouts: Retrieved ped position");

                Suspect.GiveRandomWeapon(WeaponType.AssaultRifle, -1, true);
                Game.LogTrivial("[TRACE] Emergency Callouts: Added random assault rifle to Suspect inventory");

                Suspect2.GiveRandomWeapon(WeaponType.Handgun, -1, true);
                Game.LogTrivial("[TRACE] Emergency Callouts: Added random handgun to Suspect2 inventory");

                Suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@machinery@weapon_test@"), "base_amy_skater_01", 5f, AnimationFlags.Loop); // Weapon Inspect
                Game.LogTrivial("[TRACE] Emergency Callouts: Suspect playing animation");

                Suspect2.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@peds@"), "amb_world_human_hang_out_street_male_c_base", 5f, AnimationFlags.None); // Cross Arms
                Game.LogTrivial("[TRACE] Emergency Callouts: Suspect2 playing animation");

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 25f && OnScene == true)
                        {
                            Suspect.Tasks.FightAgainst(MainPlayer);
                            Suspect2.Tasks.ClearImmediately();

                            Game.LogTrivial("[TRACE] Emergency Callouts: Assigned Suspect to fight player");
                            Game.LogTrivial("[TRACE] Emergency Callouts: Assigned Suspect tasks to null");

                            pursuit = Functions.CreatePursuit();
                            Game.LogTrivial("[TRACE] Emergency Callouts: Created pursuit");

                            Functions.AddPedToPursuit(pursuit, Suspect2);
                            Game.LogTrivial("[TRACE] Emergency Callouts: Added ped to pursuit");

                            Functions.RequestBackup(Suspect2.Position, LSPD_First_Response.EBackupResponseType.Pursuit, LSPD_First_Response.EBackupUnitType.LocalUnit);
                            Game.LogTrivial("[TRACE] Emergency Callouts: Requested backup for pursuit");

                            break;
                        }
                    }
                });

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if ((Suspect.IsDead || Suspect.IsCuffed) && Suspect.Exists())
                        {
                            Entity.Delete(SuspectBlip);
                            Game.LogTrivial("[TRACE] Emergency Callouts: Deleted SuspectBlip");

                            Entity.Delete(Suspect2Blip);
                            Game.LogTrivial("[TRACE] Emergency Callouts: Deleted Suspect2Blip");

                            Entity.Delete(SearchArea);
                            Game.LogTrivial("[TRACE] Emergency Callouts: Deleted SearchArea");

                            Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                            Game.LogTrivial("[TRACE] Emergency Callouts: Set pursuit is active for player");

                            Play.PursuitAudio();
                            Game.LogTrivial("[TRACE] Emergency Callouts: Played pursuit audio");

                            break;
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Log.CalloutException(this, "Scenario1", e);
            }
            #endregion
        }

        private void Scenario2() // Both Attack
        {
            #region Scenario 2
            try
            {
                RetrievePedPosition();
                Game.LogTrivial("[TRACE] Emergency Callouts: Retrieved ped position");

                Suspect.GiveRandomWeapon(WeaponType.SubmachingeGun, -1, true);
                Game.LogTrivial("[TRACE] Emergency Callouts: Assigned random submachine gun to Suspect inventory");

                Suspect2.GiveRandomWeapon(WeaponType.Handgun, -1, true);
                Game.LogTrivial("[TRACE] Emergency Callouts: Assigned random handgun to Suspect2 inventory");

                Suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@machinery@weapon_test@"), "base_amy_skater_01", 5f, AnimationFlags.Loop); // Weapon Inspect
                Game.LogTrivial("Emergency Callouts: Suspect playing animation");

                Suspect2.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@peds@"), "amb_world_human_hang_out_street_male_c_base", 5f, AnimationFlags.None); // Cross Arms
                Game.LogTrivial("Emergency Callouts: Suspect2 playing animation");

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 25f && OnScene == true)
                        {
                            Suspect.Tasks.FightAgainst(MainPlayer);
                            Game.LogTrivial("[TRACE] Emergency Callouts: Assigned Suspect to fight player");

                            Suspect2.Tasks.FightAgainst(MainPlayer);
                            Game.LogTrivial("[TRACE] Emergency Callouts: Assigned Suspect2 to fight player");

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

        private void Scenario3() // In-Vehicle Drug Deal
        {
            #region Scenario 3
            try
            {
                RetrieveDrugDealPosition();
                Game.LogTrivial("[TRACE] Emergency Callouts: Retrieved drug deal position");

                Suspect.GiveRandomWeapon(WeaponType.Handgun, -1, true);
                Game.LogTrivial("[TRACE] Emergency Callouts: Assigned random handgun to Suspect inventory");

                Suspect2.GiveRandomWeapon(WeaponType.Handgun, -1, true);
                Game.LogTrivial("[TRACE] Emergency Callouts: Assigned random handgun to Suspect2 inventory");

                Suspect.WarpIntoVehicle(SuspectVehicle, -1);
                Game.LogTrivial("[TRACE] Emergency Callouts: Warped Suspect into SuspectVehicle");

                Suspect2.WarpIntoVehicle(Suspect2Vehicle, -1);
                Game.LogTrivial("[TRACE] Emergency Callouts: Warped Suspect2 into Suspect2Vehicle");

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 20f && OnScene == true)
                        {
                            Entity.Delete(SuspectBlip);
                            Game.LogTrivial("[TRACE] Emergency Callouts: Deleted SuspectBlip");

                            Entity.Delete(Suspect2Blip);
                            Game.LogTrivial("[TRACE] Emergency Callouts: Deleted Suspect2Blip");

                            Entity.Delete(SearchArea);
                            Game.LogTrivial("Emergency Callouts: Deleted SearchArea");

                            pursuit = Functions.CreatePursuit();
                            Game.LogTrivial("[TRACE] Emergency Callouts: Created pursuit");

                            Functions.AddPedToPursuit(pursuit, Suspect);
                            Game.LogTrivial("[TRACE] Emergency Callouts: Added Suspect to pursuit");

                            Functions.AddPedToPursuit(pursuit, Suspect2);
                            Game.LogTrivial("[TRACE] Emergency Callouts: Added Suspect2 to pursuit");

                            Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                            Game.LogTrivial("[TRACE] Emergency Callouts: Set pursuit is active for player");

                            Play.PursuitAudio();

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

        private void Scenario4() // Suspect Searching Trunk
        {
            #region Scenario 4
            try
            {
                // Retrieve Ped Positions
                RetrievePedPosition();
                Game.LogTrivial("[TRACE] Emergency Callouts: Retrieved ped position");

                // Delete Suspect2, Suspect2Blip, SuspectVehicle.
                Entity.Delete(Suspect2);
                Game.LogTrivial("[TRACE] Emergency Callouts: Deleted Suspect2");
                Entity.Delete(Suspect2Blip);
                Game.LogTrivial("[TRACE] Emergency Callouts: Deleted Suspect2Blip");
                Entity.Delete(Suspect2Vehicle);
                Game.LogTrivial("[TRACE] Emergency Callouts: Deleted Suspect2Vehicle");

                // Clear Suspect Inventory
                Suspect.ClearInventory();
                Game.LogTrivial("[TRACE] Emergency Callouts: Cleared Suspect inventory");

                // Suspect Position & Heading
                Suspect.Position = SuspectVehicle.GetOffsetPositionFront(-SuspectVehicle.Length + 1.9f);
                Game.LogTrivial("[TRACE] Emergency Callouts: Changed Suspect position");
                Suspect.Heading = SuspectVehicle.Heading;
                Game.LogTrivial("[TRACE] Emergency Callouts: Changed Suspect heading");

                // Play Animation
                Suspect2.Tasks.PlayAnimation(new AnimationDictionary("anim@gangops@facility@servers@bodysearch@"), "player_search", 5f, AnimationFlags.UpperBodyOnly | AnimationFlags.Loop);
                Game.LogTrivial("[TRACE] Emergency Callouts: Assigned Suspect2 to play animation");

                #region Dialogue
                string[] dialogue =
                {
                    "~b~You~s~: Hello, what are you doing here?",
                    "~y~Suspect~s~: Hey officer, I'm about to pick up some packages from the store, they told me to wait behind.",
                    "~b~You~s~: Okay, do you have a receipt to prove that?",
                    "~y~Suspect~s~: Yes I do, here you go",
                    "~b~You~s~: Looks legit.",
                    "~g~Person~s~: Well that's because it is.",
                    "~b~You~s~: I can see that.",
                    "~b~You~s~: Well, I better get going!",
                    "~g~Person~s~: Goodbye."
                };

                int line = 0;

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 3f)
                        {
                            if (Game.IsKeyDown(Settings.TalkKey))
                            {
                                DialogueStarted = true;
                                Game.LogTrivial("[TRACE] Emergency Callouts: Dialogue Started");

                                Suspect.Tasks.Clear();
                                Game.LogTrivial("[TRACE] Emergency Callouts: Assigned Suspect tasks to null");


                                Suspect.Tasks.AchieveHeading(MainPlayer.Heading - 180).WaitForCompletion();
                                Game.LogTrivial("[TRACE] Emergency Callouts: Suspect achieved player heading");

                                Game.DisplaySubtitle(dialogue[line], 99999);
                                line++;

                                if (line == 3)
                                {
                                    Suspect.Tasks.PlayAnimation(new AnimationDictionary("mp_common"), "givetake1_b", 5f, AnimationFlags.None).WaitForCompletion();
                                    Game.LogTrivial("[TRACE] Emergency Callouts: Assigned Suspect to play animation");

                                    if (CalloutPosition == CalloutPositions[0]) // Strawberry
                                    {
                                        Game.DisplayNotification("", "", "Strawberry's Best Strawberries", "~y~Order ID: 4702", "3x xxx\n1x xxx\n9x xxx\n 1x xxx\n\nTotal: xxx");
                                    }
                                    else if (CalloutPosition == CalloutPositions[1]) // Del Perro
                                    {
                                        Game.DisplayNotification("", "", "Elen's Elentronics", "~y~Order ID: 0068", "1x Samsung 43AU7170 TV: $589 \n1x 1 Year of insurance: $0.00\n\nTotal: xxx");
                                    }
                                    else if (CalloutPosition == CalloutPositions[2]) // Harmony
                                    {
                                        Game.DisplayNotification("", "", "", "~y~Order ID: 0151", "1x Leather couch: $1200\n3x Bar stool: $149.97\n\nTotal: 1349.97");
                                    }

                                    Game.LogTrivial("[TRACE] Emergency Callouts: Displayed Receipt");
                                }

                                if (line == 4)
                                {
                                    MainPlayer.Tasks.PlayAnimation(new AnimationDictionary("mp_common"), "givetake1_b", 5f, AnimationFlags.None);
                                    Game.LogTrivial("[TRACE] Emergency Callouts: Assigned MainPlayer to play animation");

                                    SuspectBlip.Color = Colors.Green;
                                    Game.LogTrivial("[TRACE] Emergency Callouts: Changed SuspectBlip color to green");
                                }

                                if (line == dialogue.Length)
                                {
                                    GameFiber.Sleep(3000);
                                    Play.CodeFourAudio();
                                    Game.LogTrivial("[TRACE] Emergency Callouts: Played code four audio");

                                    End();
                                    break;
                                }
                                GameFiber.Sleep(500);
                            }
                            else
                            {
                                if (DialogueStarted == false)
                                {
                                    Game.DisplayHelp($"Press ~y~{Settings.TalkKey}~s~ to talk to the ~y~suspect~s~.");
                                }
                            }
                        }
                    }
                });
                #endregion

            }
            catch (Exception e)
            {
                Log.CalloutException(this, "Scenario4", e);
            }
            #endregion
        }

        private void Scenario5() // Ripdeal
        {
            #region Scenario 5
            try
            {
                // Close SuspectVehicle Doors
                vehDoors = SuspectVehicle.GetDoors();
                vehDoors[vehDoors.Length - 1].Close(false);

                // Retrieve Ped Positions
                RetrievePedPosition();
                Game.LogTrivial("[TRACE] Emergency Callouts: Retrieved ped position");

                // Set ped resistance
                Functions.SetPedResistanceChance(Suspect, 100f);
                Game.LogTrivial("[TRACE] Emergency Callouts: Set ped resistance to 100%");

                // Give random shotgun
                Suspect.GiveRandomWeapon(Inventory.WeaponType.Shotgun, -1, true);
                Game.LogTrivial("[TRACE] Emergency Callouts: Assigned random shotgun to Suspect inventory");

                // Change Suspect health
                Suspect2.Health = 110;
                Game.LogTrivial("[TRACE] Emergency Callouts: Set Suspect health to 150");

                // If player is close, Suspect kills Suspect2
                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 30f && OnScene == true)
                        {
                            // Change SuspectBlip Color
                            SuspectBlip.Color = Colors.Red;
                            Suspect2Blip.Color = Colors.Red;

                            // Start fight
                            Suspect.Tasks.FightAgainst(Suspect2);
                            Game.LogTrivial("[TRACE] Emergency Callouts: Assigned Suspect to fight Suspect2");
                            break;
                        }
                    }

                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (Suspect2.IsDead && Suspect2.Exists())
                        {
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
                Check.PreventDistanceCrash(CalloutPosition, OnScene, FoundPed);
                Check.PreventParamedicCrash(Suspect, Suspect2);

                #region OnPlayerArrival
                if (MainPlayer.Position.DistanceTo(Entrance) < 15f && OnScene == false)
                {
                    // Set OnScene
                    OnScene = true;
                    Game.LogTrivial("[TRACE] Emergency Callouts: Entered scene");

                    // Display Arriving Subtitle
                    Display.ArriveSubtitle("Find", "suspect", 'y');
                       
                    // Disable route
                    Entity.DisableRoute(EntranceBlip);
                    Game.LogTrivial("[TRACE] Emergency Callouts: Disabled route");

                    // Delete EntranceBlip
                    Entity.Delete(EntranceBlip);
                    Game.LogTrivial("[TRACE] Emergency Callouts: Deleted EntranceBlip");

                    // Create SearchArea
                    SearchArea = new Blip(Center, 85f);
                    SearchArea.Color = Colors.Yellow;
                    SearchArea.Alpha = 0.5f;
                    Game.LogTrivial("[TRACE] Emergency Callouts: Created SearchArea");
                }
                #endregion

                #region OnPedFound
                if (MainPlayer.Position.DistanceTo(Suspect.Position) < 5f && FoundPed == false && OnScene == true && Suspect.Exists())
                {
                    // Set PedFound
                    FoundPed = true;
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

                if (MainPlayer.Position.DistanceTo(Suspect2.Position) < 5f && FoundPed2 == false && OnScene == true && Suspect2.Exists())
                {
                    // Set FoundPed2
                    FoundPed2 = true;
                    Game.LogTrivial("[TRACE] Emergency Callouts: Found Suspect2");

                    // Hide Subtitle
                    Display.HideSubtitle();
                    Game.LogTrivial("[TRACE] Emergency Callouts: Hid subtitle");

                    // Enable Suspect2Blip
                    Entity.Enable(SuspectBlip);
                    Game.LogTrivial("[TRACE] Emergency Callouts: Enabled Suspect2Blip");

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

                #region OnPedArrested
                if (PedArrested == false && Suspect.IsCuffed && Suspect.Exists())
                {
                    // Set PedArrested
                    PedArrested = true;
                    Game.LogTrivial("[TRACE] Emergency Callouts: Suspect arrested");

                    // Display ArrestLine
                    Display.ArrestLine();
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
                    EntranceBlip = new Blip(Entrance);
                    Game.LogTrivial("[TRACE] Emergency Callouts: Created EntranceBlip");

                    // Enable Route
                    Entity.EnableRoute(EntranceBlip);
                    Game.LogTrivial("[TRACE] Emergency Callouts: Enabled route to EntranceBlip");
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
            Entity.Dismiss(Suspect2);
            Entity.Dismiss(SuspectVehicle);
            Entity.Dismiss(Suspect2Vehicle);
            Entity.Delete(SuspectBlip);
            Entity.Delete(Suspect2Blip);
            Entity.Delete(SearchArea);
            Entity.Delete(EntranceBlip);

            Display.HideSubtitle();
            Display.DetachMessage();
            Log.CalloutEnded(CalloutMessage, ScenarioNumber);
        }
    }
}