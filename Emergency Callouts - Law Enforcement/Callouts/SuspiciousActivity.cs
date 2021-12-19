using EmergencyCalloutsLE.Essential;
using LSPD_First_Response.Engine.UI;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using static EmergencyCalloutsLE.Essential.Color;
using static EmergencyCalloutsLE.Essential.Helper;
using static EmergencyCalloutsLE.Essential.Inventory;
using Entity = EmergencyCalloutsLE.Essential.Helper.Entity;

namespace EmergencyCalloutsLE.Callouts
{
    [CalloutInfo("Suspicious Activity", CalloutProbability.Medium)]
    public class SuspiciousActivity : Callout
    {
        bool CalloutActive;
        bool PlayerArrived;
        bool PedFound;
        bool Ped2Found;
        bool PedDetained;
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
            new Vector3(1243.041f, -2395.421f, 47.91381f), // El Burro
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
        readonly Vector3 HarmonySuspectPosition = new Vector3(604.8357f, 2789.544f, 42.1919f);
        readonly float HarmonySuspectHeading = 4.81f;

        readonly Vector3 HarmonySuspect2Position = new Vector3(604.5134f, 2792.431f, 42.14416f);
        readonly float HarmonySuspect2Heading = 187.87f;

        readonly Vector3 HarmonyVehiclePosition = new Vector3(606.4803f, 2791.021f, 41.69831f);
        readonly float HarmonyVehicleHeading = 6.87f;

        readonly Vector3 HarmonyVehicle2Position = new Vector3(602.7837f, 2790.826f, 41.7882f);
        readonly float HarmonyVehicle2Heading = 7.75f;
        #endregion

        // El Burro
        #region Positions
        readonly Vector3 ElBurroSuspectPosition = new Vector3(1228.299f, -2354.579f, 50.30099f);
        readonly float ElBurroSuspectHeading = 233.10f;

        readonly Vector3 ElBurroSuspect2Position = new Vector3(1230.999f, -2353.718f, 50.26699f);
        readonly float ElBurroSuspect2Heading = 102.50f;

        readonly Vector3 ElBurroVehiclePosition = new Vector3(1230.793f, -2357.722f, 49.84827f);
        readonly float ElBurroVehicleHeading = 214.59f;

        readonly Vector3 ElBurroVehicle2Position = new Vector3(1233.492f, -2355.017f, 49.81187f);
        readonly float ElBurroVehicle2Heading = 242.13f;
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

            CalloutMessage = "Suspicious Activity";
            CalloutDetails = "Multiple civilians called about a person handling guns in the trunk of their car.";
            CalloutScenario = GetRandomScenarioNumber(5);

            Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT CRIME_SUSPICIOUS_ACTIVITY IN_OR_ON_POSITION", CalloutPosition);

            return base.OnBeforeCalloutDisplayed();
        }

        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Callout not accepted");
            Functions.PlayScannerAudio("PED_RESPONDING_DISPATCH");

            base.OnCalloutNotAccepted();
        }

        public override bool OnCalloutAccepted()
        {
            try
            {
                // Callout Accepted
                Log.CalloutAccepted(CalloutMessage, CalloutScenario);

                // Attach Message
                Display.AttachMessage(CalloutDetails);

                // EntranceBlip
                EntranceBlip = new Blip(Entrance);

                // Suspect
                Suspect = new Ped(Entity.GetRandomMaleModel(), CalloutPosition, 0f);
                Suspect.SetDefaults();
                Game.LogTrivial($"[Emergency Callouts - Law Enforcement]: Created Suspect ({Suspect.Model.Name}) at " + Suspect.Position);

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColor(Colors.Yellow);
                SuspectBlip.ScaleForPed();
                SuspectBlip.Disable();

                // Suspect 2
                Suspect2 = new Ped(Entity.GetRandomMaleModel(), CalloutPosition, 0f);
                Suspect2.SetDefaults();
                Game.LogTrivial($"[Emergency Callouts - Law Enforcement]: Created Suspect2 ({Suspect2.Model.Name}) at " + Suspect2.Position);

                Suspect2Blip = Suspect2.AttachBlip();
                Suspect2Blip.SetColor(Colors.Yellow);
                Suspect2Blip.ScaleForPed();
                Suspect2Blip.Disable();

                // SuspectVehicle
                SuspectVehicle = new Vehicle(Vehicles.GetRandomFourDoor(), CalloutPosition, 0f);
                SuspectVehicle.IsPersistent = true;
                Game.LogTrivial($"[Emergency Callouts - Law Enforcement]: Created SuspectVehicle ({SuspectVehicle.Model.Name}) at " + SuspectVehicle.Position);

                vehDoors = SuspectVehicle.GetDoors();
                vehDoors[vehDoors.Length - 1].Open(false);

                // Suspect2Vehicle
                Suspect2Vehicle = new Vehicle(Vehicles.GetRandomFourDoor(), CalloutPosition, 0f);
                Suspect2Vehicle.IsPersistent = true;
                Game.LogTrivial($"[Emergency Callouts - Law Enforcement]: Created Suspect2Vehicle ({Suspect2Vehicle.Model.Name}) at " + Suspect2Vehicle.Position);

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
                else if (CalloutPosition == CalloutPositions[3]) // El Burro
                {
                    Center = new Vector3(1243.041f, -2395.421f, 47.91381f);
                    Entrance = new Vector3(1115.294f, -2555.428f, 31.27009f);
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
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Enabled route to EntranceBlip");
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
                Suspect.Position = HarmonySuspectPosition;
                Suspect.Heading = HarmonySuspectHeading;

                Suspect2.Position = HarmonySuspect2Position;
                Suspect2.Heading = HarmonySuspect2Heading;

                SuspectVehicle.Position = HarmonyVehiclePosition;
                SuspectVehicle.Heading = HarmonyVehicleHeading;

                Suspect2Vehicle.Position = HarmonyVehicle2Position;
                Suspect2Vehicle.Heading = HarmonyVehicle2Heading;
            }
            else if (CalloutPosition == CalloutPositions[3]) // El Burro
            {
                Suspect.Position = ElBurroSuspectPosition;
                Suspect.Heading = ElBurroSuspectHeading;

                Suspect2.Position = ElBurroSuspect2Position;
                Suspect2.Heading = ElBurroSuspect2Heading;

                SuspectVehicle.Position = ElBurroVehiclePosition;
                SuspectVehicle.Heading = ElBurroVehicleHeading;

                Suspect2Vehicle.Position = ElBurroVehicle2Position;
                Suspect2Vehicle.Heading = ElBurroVehicle2Heading;
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
            else if (CalloutPosition == CalloutPositions[2]) // El Burro
            {
                SuspectVehicle.Position = new Vector3(1232.671f, -2361.097f, 49.63579f);
                Suspect2Vehicle.Position = new Vector3(1230.62f, -2359.984f, 49.77252f);

                SuspectVehicle.Heading = 337.38f;
                Suspect2Vehicle.Heading = 158.24f;
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
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Retrieved ped position");

                Suspect.GiveRandomWeapon(WeaponType.AssaultRifle, -1, true);
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Added random assault rifle to Suspect inventory");

                Suspect2.GiveRandomWeapon(WeaponType.Handgun, -1, true);
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Added random handgun to Suspect2 inventory");

                Suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@machinery@weapon_test@"), "base_amy_skater_01", 5f, AnimationFlags.Loop); // Weapon Inspect
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Suspect playing animation");

                Suspect2.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@peds@"), "amb_world_human_hang_out_street_male_c_base", 5f, AnimationFlags.None); // Cross Arms
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Suspect2 playing animation");

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 20f && PlayerArrived)
                        {
                            Suspect.Tasks.FightAgainst(MainPlayer);
                            Suspect2.Tasks.ClearImmediately();

                            Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Assigned Suspect to fight " + PlayerPersona.FullName);
                            Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Cleared Suspect tasks");

                            pursuit = Functions.CreatePursuit();
                            Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Created pursuit");

                            Functions.AddPedToPursuit(pursuit, Suspect2);
                            Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Added ped to pursuit");

                            Functions.RequestBackup(Suspect2.Position, LSPD_First_Response.EBackupResponseType.Pursuit, LSPD_First_Response.EBackupUnitType.LocalUnit);
                            Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Requested backup for pursuit");

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
                            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                            Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Deleted SuspectBlip");

                            if (Suspect2Blip.Exists()) { Suspect2Blip.Delete(); }
                            Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Deleted Suspect2Blip");

                            if (SearchArea.Exists()) { SearchArea.Delete(); }
                            Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Deleted SearchArea");

                            Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                            Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Set pursuit is active for player");

                            Play.PursuitAudio();
                            Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Played pursuit audio");

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
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Retrieved ped position");

                Suspect.GiveRandomWeapon(WeaponType.SubmachineGun, -1, true);
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Assigned random submachine gun to Suspect inventory");

                Suspect2.GiveRandomWeapon(WeaponType.Handgun, -1, true);
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Assigned random handgun to Suspect2 inventory");

                Suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@machinery@weapon_test@"), "base_amy_skater_01", 5f, AnimationFlags.Loop); // Weapon Inspect
                Game.LogTrivial("Emergency Callouts: Suspect playing animation");

                Suspect2.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@peds@"), "amb_world_human_hang_out_street_male_c_base", 5f, AnimationFlags.None); // Cross Arms
                Game.LogTrivial("Emergency Callouts: Suspect2 playing animation");

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 20f && PlayerArrived)
                        {
                            Suspect.Tasks.FightAgainst(MainPlayer);
                            Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Assigned Suspect to fight " + PlayerPersona.FullName);

                            Suspect2.Tasks.FightAgainst(MainPlayer);
                            Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Assigned Suspect2 to fight " + PlayerPersona.FullName);

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
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Retrieved drug deal position");

                Suspect.GiveRandomWeapon(WeaponType.Handgun, -1, true);
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Assigned random handgun to Suspect inventory");

                Suspect2.GiveRandomWeapon(WeaponType.Handgun, -1, true);
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Assigned random handgun to Suspect2 inventory");

                Suspect.WarpIntoVehicle(SuspectVehicle, -1);
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Warped Suspect into SuspectVehicle");

                Suspect2.WarpIntoVehicle(Suspect2Vehicle, -1);
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Warped Suspect2 into Suspect2Vehicle");

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 20f && PlayerArrived)
                        {
                            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                            Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Deleted SuspectBlip");

                            if (Suspect2Blip.Exists()) { Suspect2Blip.Delete(); }
                            Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Deleted Suspect2Blip");

                            if (SearchArea.Exists()) { SearchArea.Delete(); }
                            Game.LogTrivial("Emergency Callouts: Deleted SearchArea");

                            pursuit = Functions.CreatePursuit();
                            Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Created pursuit");

                            Functions.AddPedToPursuit(pursuit, Suspect);
                            Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Added Suspect to pursuit");

                            Functions.AddPedToPursuit(pursuit, Suspect2);
                            Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Added Suspect2 to pursuit");

                            Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                            Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Set pursuit is active for player");

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
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Retrieved ped position");

                // Delete Suspect2, Suspect2Blip, SuspectVehicle.
                if (Suspect2.Exists()) { Suspect2.Delete(); }
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Deleted Suspect2");

                if (Suspect2Blip.Exists()) { Suspect2Blip.Delete(); }
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Deleted Suspect2Blip");

                if (Suspect2Vehicle.Exists()) { Suspect2Vehicle.Delete(); }
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Deleted Suspect2Vehicle");

                // Clear Suspect Inventory
                Suspect.Inventory.Weapons.Clear();
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Cleared Suspect inventory");

                // Suspect Position & Heading
                Suspect.Position = SuspectVehicle.GetOffsetPositionFront(-SuspectVehicle.Length + 1.9f);
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Changed Suspect position");
                Suspect.Heading = SuspectVehicle.Heading;
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Changed Suspect heading");

                // Play Animation
                Suspect2.Tasks.PlayAnimation(new AnimationDictionary("anim@gangops@facility@servers@bodysearch@"), "player_search", 5f, AnimationFlags.UpperBodyOnly | AnimationFlags.Loop);
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Assigned Suspect2 to play animation");

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
                                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Dialogue Started");

                                Suspect.Tasks.Clear();
                                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Assigned Suspect tasks to null");


                                Suspect.Tasks.AchieveHeading(MainPlayer.Heading - 180).WaitForCompletion();
                                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Suspect achieved player heading");

                                Game.DisplaySubtitle(dialogue[line], 99999);
                                line++;

                                if (line == 3)
                                {
                                    Suspect.Tasks.PlayAnimation(new AnimationDictionary("mp_common"), "givetake1_b", 5f, AnimationFlags.None).WaitForCompletion();
                                    Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Assigned Suspect to play animation");

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

                                    Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Displayed Receipt");
                                }

                                if (line == 4)
                                {
                                    MainPlayer.Tasks.PlayAnimation(new AnimationDictionary("mp_common"), "givetake1_b", 5f, AnimationFlags.None);
                                    Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Assigned MainPlayer to play animation");

                                    SuspectBlip.SetColor(Colors.Green);
                                    Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Changed SuspectBlip color to green");
                                }

                                if (line == dialogue.Length)
                                {
                                    GameFiber.Sleep(3000);
                                    Play.CodeFourAudio();
                                    Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Played code four audio");

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
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Retrieved ped position");

                // Set ped resistance
                Functions.SetPedResistanceChance(Suspect, 100f);
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Set ped resistance to 100%");

                // Give new random shotgun
                Suspect.GiveRandomWeapon(WeaponType.Shotgun, -1, true);
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Assigned random shotgun to Suspect inventory");

                // Change Suspect health
                Suspect2.Health = 110;
                Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Set Suspect health to 150");

                // If player is close, Suspect kills Suspect2
                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 30f && PlayerArrived)
                        {
                            // Change SuspectBlip Color
                            SuspectBlip.SetColor(Colors.Red);
                            Suspect2Blip.SetColor(Colors.Red);

                            // Start fight
                            Suspect.Tasks.FightAgainst(Suspect2);
                            Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Assigned Suspect to fight Suspect2");
                            break;
                        }
                    }

                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (Suspect2.IsDead && Suspect2.Exists())
                        {
                            Suspect.Tasks.FightAgainst(MainPlayer);
                            Game.LogTrivial("[Emergency Callouts - Law Enforcement]: Assigned Suspect to fight " + PlayerPersona.FullName);
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
                Handle.ManualEnding();
                Handle.AutomaticEnding(Suspect);
                Handle.PreventFirstResponderCrash(Suspect, Suspect2);
                Handle.PreventDistanceCrash(CalloutPosition, PlayerArrived, PedFound);

                #region PlayerArrived
                if (MainPlayer.Position.DistanceTo(Entrance) < 15f && !PlayerArrived)
                {
                    // Set PlayerArrived
                    PlayerArrived = true;

                    // Display Arriving Subtitle
                    Game.DisplaySubtitle("Find the ~y~suspect~s~ in the ~y~area~s~.", 10000);

                    // Disable route
                    EntranceBlip.DisableRoute();

                    // Delete EntranceBlip
                    if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

                    // Create SearchArea
                    SearchArea = new Blip(Center, 85f);
                    SearchArea.SetColor(Colors.Yellow);
                    SearchArea.Alpha = 0.5f;

                    Game.LogTrivial($"[Emergency Callouts - Law Enforcement]: {PlayerPersona.FullName} has arrived on scene");
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

                    Game.LogTrivial($"[Emergency Callouts - Law Enforcement]: {PlayerPersona.FullName} has found the suspect");
                }

                if (MainPlayer.Position.DistanceTo(Suspect2.Position) < 5f && !Ped2Found && PlayerArrived && Suspect2.Exists())
                {
                    // Set PedFound
                    Ped2Found = true;

                    // Hide Subtitle
                    Display.HideSubtitle();

                    // Enable Suspect2Blip
                    Suspect2Blip.Enable();

                    // Delete SearchArea
                    if (SearchArea.Exists()) { SearchArea.Delete(); }

                    Game.LogTrivial($"[Emergency Callouts - Law Enforcement]: {PlayerPersona.FullName} has found the second suspect");
                }
                #endregion

                #region PedDetained
                if (Suspect.IsPedDetained() && !PedDetained && Suspect.Exists())
                {
                    // Set PedDetained
                    PedDetained = true;

                    // Delete SuspectBlip
                    if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }

                    Game.LogTrivial($"[Emergency Callouts - Law Enforcement]: {PlayerPersona.FullName} has detained the suspect");
                }
                #endregion

                #region PlayerLeft
                if (MainPlayer.Position.DistanceTo(CalloutPosition) > Settings.SearchAreaSize * 3.5f && PlayerArrived)
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

                    Game.LogTrivial($"[Emergency Callouts - Law Enforcement]: {PlayerPersona.FullName} has left the scene");
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

            if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (Suspect2.Exists()) { Suspect2.Dismiss(); }
            if (SuspectVehicle.Exists()) { SuspectVehicle.Dismiss(); }
            if (Suspect2Vehicle.Exists()) { Suspect2Vehicle.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            if (Suspect2Blip.Exists()) { Suspect2Blip.Delete(); }
            if (SearchArea.Exists()) { SearchArea.Delete(); }
            if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

            Display.HideSubtitle();
            Display.DetachMessage();
            Log.CalloutEnded(CalloutMessage, CalloutScenario);
        }
    }
}