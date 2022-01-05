using EmergencyCallouts.Essential;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Reflection;
using static EmergencyCallouts.Essential.Color;
using static EmergencyCallouts.Essential.Helper;
using Color =  EmergencyCallouts.Essential.Color;
using Entity = EmergencyCallouts.Essential.Helper.Entity;

namespace EmergencyCallouts.Callouts
{
    [CalloutInfo("Suspicious Activity", CalloutProbability.Medium)]
    public class SuspiciousActivity : Callout
    {
        bool CalloutActive;
        bool PlayerArrived;
        bool PedFound;
        bool Ped2Found;
        bool PedDetained;
        bool StopChecking;

        Vector3 Entrance;
        Vector3 Center;

        // Main
        #region Positions
        readonly Vector3[] CalloutPositions =
        {
            new Vector3(167.0673f, -1247.618f, 29.19848f),  // Strawberry
            new Vector3(-1283.511f, -811.2982f, 17.32025f), // Del Perro
            new Vector3(651.5822f, 2762.731f, 41.94574f),   // Harmony
            new Vector3(1243.041f, -2395.421f, 47.91381f),  // El Burro
            new Vector3(2165.78f, 4758.762f, 42f),          // Grapeseed
            new Vector3(1485.026f, 6412.347f, 22.35379f),   // Paleto Bay
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

        // McKenzie Field
        #region Positions
        readonly Vector3 McKenzieFieldSuspectPosition = new Vector3(2142.929f, 4781.304f, 40.97033f);
        readonly float McKenzieFieldSuspectHeading = 134.41f;

        readonly Vector3 McKenzieFieldSuspect2Position = new Vector3(2141.337f, 4779.794f, 40.97033f);
        readonly float McKenzieFieldSuspect2Heading = 305.29f;

        readonly Vector3 McKenzieFieldVehiclePosition = new Vector3(2141.803f, 4784.83f, 40.50724f);
        readonly float McKenzieFieldVehicleHeading = 26.35f;

        readonly Vector3 McKenzieFieldVehicle2Position = new Vector3(2139.113f, 4781.735f, 40.38437f);
        readonly float McKenzieFieldVehicle2Heading = 63.12f;
        #endregion

        // Paleto Bay
        #region Positions
        readonly Vector3 PaletoBaySuspectPosition = new Vector3(1534.92f, 6341.109f, 24.1971f);
        readonly float PaletoBaySuspectHeading = 141.26f;

        readonly Vector3 PaletoBaySuspect2Position = new Vector3(1533.391f, 6338.765f, 24.20876f);
        readonly float PaletoBaySuspect2Heading = 333.21f;

        readonly Vector3 PaletoBayVehiclePosition = new Vector3(1532.055f, 6342.446f, 23.83713f);
        readonly float PaletoBayVehicleHeading = 57.85f;

        readonly Vector3 PaletoBayVehicle2Position = new Vector3(1530.411f, 6339.453f, 23.86996f);
        readonly float PaletoBayVehicle2Heading = 80.94f;
        #endregion

        Vehicle SuspectVehicle;
        Vehicle Suspect2Vehicle;

        VehicleDoor[] vehDoors;
        VehicleDoor[] veh2Doors;

        Ped Suspect;
        Ped Suspect2;

        Persona SuspectPersona;
        Persona Suspect2Persona;

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
                    CalloutArea = World.GetStreetName(loc).Replace("Olympic Fwy", "Strawberry Ave"); ;
                }
            }

            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, Settings.SearchAreaSize / 2.5f);

            CalloutMessage = "Suspicious Activity";
            CalloutDetails = "Multiple civilians called about a person handling possible firearms in the trunk of their car.";
            CalloutScenario = GetRandomScenarioNumber(2);

            Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT CRIME_SUSPICIOUS_ACTIVITY IN_OR_ON_POSITION", CalloutPosition);

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

                // Accept Messages
                Display.AcceptNotification(CalloutDetails);
                Display.AcceptSubtitle(CalloutMessage, CalloutArea);
                Display.OutdatedReminder();

                // EntranceBlip
                EntranceBlip = new Blip(Entrance);

                // Suspect
                Suspect = new Ped(Entity.GetRandomMaleModel(), CalloutPosition, 0f);
                SuspectPersona = Functions.GetPersonaForPed(Suspect);
                Suspect.SetDefaults();

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColorRed();
                SuspectBlip.ScaleForPed();
                SuspectBlip.Disable();

                // Suspect 2
                Suspect2 = new Ped(Entity.GetRandomMaleModel(), CalloutPosition, 0f);
                Suspect2Persona = Functions.GetPersonaForPed(Suspect2);
                Suspect2.SetDefaults();

                Suspect2Blip = Suspect2.AttachBlip();
                Suspect2Blip.SetColorRed();
                Suspect2Blip.ScaleForPed();
                Suspect2Blip.Disable();

                // SuspectVehicle
                SuspectVehicle = new Vehicle(Vehicles.GetRandomFourDoor(), CalloutPosition, 0f);
                SuspectVehicle.IsPersistent = true;

                vehDoors = SuspectVehicle.GetDoors();
                vehDoors[5].Open(false);

                // Suspect2Vehicle
                Suspect2Vehicle = new Vehicle(Vehicles.GetRandomFourDoor(), CalloutPosition, 0f);
                Suspect2Vehicle.IsPersistent = true;

                veh2Doors = Suspect2Vehicle.GetDoors();
                veh2Doors[5].Open(false);

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
                else if (CalloutPosition == CalloutPositions[4]) // McKenzie Field
                {
                    Center = new Vector3(2118.948f, 4802.422f, 41.19594f);
                    Entrance = new Vector3(2165.78f, 4758.762f, 42f);
                    EntranceBlip.Position = Entrance;
                }
                else if (CalloutPosition == CalloutPositions[5]) // Paleto Bay
                {
                    Center = new Vector3(1477.096f, 6343.949f, 22.35379f);
                    Entrance = new Vector3(1485.026f, 6412.347f, 22.35379f);
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
                }

                // Enabling Route
                EntranceBlip.EnableRoute();
                Game.LogTrivial("[Emergency Callouts]: Enabled route to EntranceBlip");

                // Log Creation
                Log.Creation(Suspect, PedCategory.Suspect);
                Log.Creation(Suspect2, PedCategory.Suspect2);
                Log.Creation(SuspectVehicle, PedCategory.Suspect);
                Log.Creation(Suspect2Vehicle, PedCategory.Suspect2);
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
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
            else if (CalloutPosition == CalloutPositions[4]) // McKenzie Field
            {
                Suspect.Position = McKenzieFieldSuspectPosition;
                Suspect.Heading = McKenzieFieldSuspectHeading;

                Suspect2.Position = McKenzieFieldSuspect2Position;
                Suspect2.Heading = McKenzieFieldSuspect2Heading;

                SuspectVehicle.Position = McKenzieFieldVehiclePosition;
                SuspectVehicle.Heading = McKenzieFieldVehicleHeading;

                Suspect2Vehicle.Position = McKenzieFieldVehicle2Position;
                Suspect2Vehicle.Heading = McKenzieFieldVehicle2Heading;
            }
            else if (CalloutPosition == CalloutPositions[5]) // Paleto Bay
            {
                Suspect.Position = PaletoBaySuspectPosition;
                Suspect.Heading = PaletoBaySuspectHeading;

                Suspect2.Position = PaletoBaySuspect2Position;
                Suspect2.Heading = PaletoBaySuspect2Heading;

                SuspectVehicle.Position = PaletoBayVehiclePosition;
                SuspectVehicle.Heading = PaletoBayVehicleHeading;

                Suspect2Vehicle.Position = PaletoBayVehicle2Position;
                Suspect2Vehicle.Heading = PaletoBayVehicle2Heading;
            }
            #endregion
        }

        private void Scenario1() // Fight And Flee
        {
            #region Scenario 1
            try
            {
                RetrievePedPosition();

                Suspect.GiveRandomAssaultRifle(-1, true);
                Suspect2.GiveRandomHandgun(-1, true);

                Suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@machinery@weapon_test@"), "base_amy_skater_01", 5f, AnimationFlags.Loop); // Weapon Inspect
                Suspect2.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@peds@"), "amb_world_human_hang_out_street_male_c_base", 5f, AnimationFlags.None); // Cross Arms

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 25f && PlayerArrived)
                        {
                            if (Suspect2Blip.Exists()) { Suspect2Blip.Delete(); }

                            Suspect.Tasks.FightAgainst(MainPlayer);
                            Suspect2.Tasks.ClearImmediately();

                            StopChecking = true;

                            pursuit = Functions.CreatePursuit();

                            Functions.AddPedToPursuit(pursuit, Suspect2);

                            Functions.RequestBackup(Suspect2.Position, LSPD_First_Response.EBackupResponseType.Pursuit, LSPD_First_Response.EBackupUnitType.LocalUnit);

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
                            // Delete Blips
                            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                            if (SearchArea.Exists()) { SearchArea.Delete(); }
                            if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

                            Functions.SetPursuitIsActiveForPlayer(pursuit, true);

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

        private void Scenario2() // Ripdeal
        {
            #region Scenario 2
            try
            {
                // Close SuspectVehicle Doors
                vehDoors = SuspectVehicle.GetDoors();
                vehDoors[vehDoors.Length - 1].Close(false);

                // Retrieve Ped Positions
                RetrievePedPosition();

                // Set ped resistance
                Functions.SetPedResistanceChance(Suspect, 100f);

                // Give new random shotgun
                Suspect.GiveRandomShotgun(-1, true);

                // Change Suspect health
                Suspect2.Health = 110;

                // If player is close, Suspect kills Suspect2
                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 30f && PlayerArrived)
                        {
                            // Start fight
                            Suspect.Tasks.FightAgainst(Suspect2);
                            break;
                        }
                    }

                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (Suspect2.IsDead && Suspect2.Exists())
                        {
                            Suspect.Tasks.Clear();
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

        public override void Process()
        {
            base.Process();
            try
            {
                Handle.ManualEnding();
                Handle.AutomaticEnding(Suspect, Suspect2);
                Handle.PreventFirstResponderCrash(Suspect, Suspect2);
                //Handle.PreventDistanceCrash(CalloutPosition, PlayerArrived, PedFound);

                #region PlayerArrived
                if (MainPlayer.Position.DistanceTo(Entrance) < 15f && !PlayerArrived)
                {
                    // Set PlayerArrived
                    PlayerArrived = true;

                    // Delete Nearby Vehicles
                    Handle.DeleteNearbyVehicles(SuspectVehicle, Suspect2Vehicle);

                    // Display Arriving Subtitle
                    Game.DisplaySubtitle("Find the ~r~suspect~s~ in the ~y~area~s~.", 20000);

                    // Disable route
                    EntranceBlip.DisableRoute();

                    // Delete EntranceBlip
                    if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

                    // Create SearchArea
                    SearchArea = new Blip(Center, Settings.SearchAreaSize + 25f);
                    SearchArea.SetColorYellow();
                    SearchArea.Alpha = 0.5f;

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has arrived on scene");
                }
                #endregion

                #region PedFound
                if (MainPlayer.Position.DistanceTo(Suspect.Position) < 5f && !PedFound && PlayerArrived && Suspect.Exists())
                {
                    // Set PedFound
                    PedFound = true;

                    // Enable SuspectBlip
                    SuspectBlip.Enable();

                    // Delete SearchArea
                    if (SearchArea.Exists()) { SearchArea.Delete(); }

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has found {SuspectPersona.FullName} (Suspect)");
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

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has found {Suspect2Persona.FullName} (Suspect2)");
                }
                #endregion

                #region PedDetained
                if (Suspect.IsPedDetained() && !PedDetained && Suspect.Exists())
                {
                    // Set PedDetained
                    PedDetained = true;

                    // Delete SuspectBlip
                    if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has detained {SuspectPersona.FullName} (Suspect)");
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
            if (Suspect2.Exists()) { Suspect2.Dismiss(); }
            if (SuspectVehicle.Exists()) { SuspectVehicle.Dismiss(); }
            if (Suspect2Vehicle.Exists()) { Suspect2Vehicle.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            if (Suspect2Blip.Exists()) { Suspect2Blip.Delete(); }
            if (SearchArea.Exists()) { SearchArea.Delete(); }
            if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

            Display.HideSubtitle();
            Display.EndNotification();
            Log.OnCalloutEnded(CalloutMessage, CalloutScenario);
        }
    }
}