using EmergencyCallouts.Essential;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Reflection;
using static EmergencyCallouts.Essential.Color;
using static EmergencyCallouts.Essential.Helper;
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
        bool WithinRange;

        Vector3 Entrance;
        Vector3 Center;

        // Main
        #region Positions
        readonly Vector3[] CalloutPositions =
        {
            new Vector3(-646.7701f, -1639.802f, 25.06787f), // La Puerta
            new Vector3(-1283.511f, -811.2982f, 17.32025f), // Del Perro
            new Vector3(651.5822f, 2762.731f, 41.94574f),   // Harmony
            new Vector3(1243.041f, -2395.421f, 47.91381f),  // El Burro
            new Vector3(2165.78f, 4758.762f, 42f),          // Grapeseed
            new Vector3(1485.026f, 6412.347f, 22.35379f),   // Paleto Bay
        };
        #endregion

        // La Puerta
        #region Positions
        readonly Vector3 LaPuertaSuspectPosition = new Vector3(-587.6331f, -1587.74f, 26.75113f);
        readonly float LaPuertaSuspectHeading = 14.97f;

        readonly Vector3 LaPuertaSuspect2Position = new Vector3(-587.4804f, -1585.351f, 26.75113f);
        readonly float LaPuertaSuspect2Heading = 163.78f;

        readonly Vector3 LaPuertaVehiclePosition = new Vector3(-591.1713f, -1587.647f, 26.41216f);
        readonly float LaPuertaVehicleHeading = 89.17f;

        readonly Vector3 LaPuertaVehicle2Position = new Vector3(-590.5353f, -1584.49f, 26.44631f);
        readonly float LaPuertaVehicle2Heading = 77.31f;
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
                    CalloutArea = World.GetStreetName(loc);
                }
            }

            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, Settings.SearchAreaSize / 2.5f);

            CalloutMessage = "Suspicious Activity";
            CalloutDetails = "Multiple civilians called about a person handling possible ~y~firearms~s~ in the trunk of their car.";
            CalloutScenario = random.Next(1, 3);

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
                // Positioning
                #region Positioning
                if (CalloutPosition == CalloutPositions[0]) // La Puerta
                {
                    Center = new Vector3(-616.0434f, -1600.232f, 26.75098f);
                    Entrance = new Vector3(-646.7701f, -1639.802f, 25.06787f);
                }
                else if (CalloutPosition == CalloutPositions[1]) // Del Perro
                {
                    Center = new Vector3(-1283.511f, -811.2982f, 17.32025f);
                    Entrance = new Vector3(-1364.522f, -709.0762f, 24.67615f);
                }
                else if (CalloutPosition == CalloutPositions[2]) // Harmony
                {
                    Center = new Vector3(597.8428f, 2796.708f, 41.99812f);
                    Entrance = new Vector3(651.5822f, 2762.731f, 41.94574f);
                }
                else if (CalloutPosition == CalloutPositions[3]) // El Burro
                {
                    Center = new Vector3(1243.041f, -2395.421f, 47.91381f);
                    Entrance = new Vector3(1115.294f, -2555.428f, 31.27009f);
                }
                else if (CalloutPosition == CalloutPositions[4]) // McKenzie Field
                {
                    Center = new Vector3(2118.948f, 4802.422f, 41.19594f);
                    Entrance = new Vector3(2165.78f, 4758.762f, 42f);
                }
                else if (CalloutPosition == CalloutPositions[5]) // Paleto Bay
                {
                    Center = new Vector3(1477.096f, 6343.949f, 22.35379f);
                    Entrance = new Vector3(1485.026f, 6412.347f, 22.35379f);
                }
                #endregion

                // Callout Accepted
                Log.OnCalloutAccepted(CalloutMessage, CalloutScenario);

                // Accept Messages
                Display.AcceptNotification(CalloutDetails);
                Display.AcceptSubtitle(CalloutMessage, CalloutArea);
                Display.OutdatedReminder();

                // EntranceBlip
                EntranceBlip = new Blip(Entrance);
                if (EntranceBlip.Exists()) { EntranceBlip.IsRouteEnabled = true; }

                // Suspect
                Suspect = new Ped(Entity.GetRandomMaleModel(), Vector3.Zero, 0f);
                SuspectPersona = Functions.GetPersonaForPed(Suspect);
                Suspect.IsPersistent = true;
                Suspect.BlockPermanentEvents = true;

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColorRed();
                SuspectBlip.Scale = (float)Settings.PedBlipScale;
                SuspectBlip.Alpha = 0f;

                // Suspect 2
                Suspect2 = new Ped(Entity.GetRandomMaleModel(), Vector3.Zero, 0f);
                Suspect2Persona = Functions.GetPersonaForPed(Suspect2);
                Suspect2.IsPersistent = true;
                Suspect2.BlockPermanentEvents = true;

                Suspect2Blip = Suspect2.AttachBlip();
                Suspect2Blip.SetColorRed();
                Suspect2Blip.Scale = (float)Settings.PedBlipScale;
                Suspect2Blip.Alpha = 0f;

                // SuspectVehicle
                SuspectVehicle = new Vehicle(Vehicles.GetRandomFourDoor(), Vector3.Zero, 0f);
                SuspectVehicle.IsPersistent = true;

                vehDoors = SuspectVehicle.GetDoors();
                vehDoors[vehDoors.Length - 1].Open(false);

                // Suspect2Vehicle
                Suspect2Vehicle = new Vehicle(Vehicles.GetRandomFourDoor(), Vector3.Zero, 0f);
                Suspect2Vehicle.IsPersistent = true;

                veh2Doors = Suspect2Vehicle.GetDoors();
                veh2Doors[veh2Doors.Length - 1].Open(false);

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
            if (CalloutPosition == CalloutPositions[0]) // La Puerta
            {
                Suspect.Position = LaPuertaSuspectPosition;
                Suspect.Heading = LaPuertaSuspectHeading;

                Suspect2.Position = LaPuertaSuspect2Position;
                Suspect2.Heading = LaPuertaSuspect2Heading;

                SuspectVehicle.Position = LaPuertaVehiclePosition;
                SuspectVehicle.Heading = LaPuertaVehicleHeading;

                Suspect2Vehicle.Position = LaPuertaVehicle2Position;
                Suspect2Vehicle.Heading = LaPuertaVehicle2Heading;
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

            Log.Creation(Suspect, PedCategory.Suspect);
            Log.Creation(Suspect2, PedCategory.Suspect2);
            Log.Creation(SuspectVehicle, PedCategory.Suspect);
            Log.Creation(Suspect2Vehicle, PedCategory.Suspect2);

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

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) <= 30f && PlayerArrived && Suspect.Exists())
                        {
                            // Start fight
                            if (Suspect.IsAlive) { Suspect.Tasks.FightAgainst(Suspect2); }
                            break;
                        }
                    }

                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (Suspect.IsAlive && Suspect2.IsDead && Suspect.Exists() && Suspect2.Exists())
                        {
                            Suspect.Tasks.Clear();
                            Suspect.Tasks.FightAgainst(MainPlayer);
                            break;
                        }
                    }
                });

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (Suspect2.IsAlive && (Suspect.IsDead || Suspect.IsCuffed))
                        {
                            Suspect2.Tasks.PutHandsUp(-1, MainPlayer);
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
                Handle.PreventPickupCrash(Suspect, Suspect2);

                #region WithinRange
                if (MainPlayer.Position.DistanceTo(CalloutPosition) <= 200f && !WithinRange)
                {
                    // Set WithinRange
                    WithinRange = true;

                    // Delete Nearby Trailers
                    Handle.DeleteNearbyTrailers(Center, 100f);

                    // Delete Nearby Peds
                    Handle.DeleteNearbyPeds(Suspect, Suspect2, 40f);

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} is within 200 meters");
                }
                #endregion

                #region PlayerArrived
                if (MainPlayer.Position.DistanceTo(Entrance) < 15f && !PlayerArrived)
                {
                    // Set PlayerArrived
                    PlayerArrived = true;

                    // Gang Attack Fix
                    Handle.BlockPermanentEventsRadius(Center, 60f);

                    // Display Arriving Subtitle
                    Game.DisplaySubtitle("Find the ~r~suspect~s~ in the ~y~area~s~.", 10000);

                    // Delete EntranceBlip
                    if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

                    // Create SearchArea
                    SearchArea = new Blip(Suspect.Position.Around2D(30f), Settings.SearchAreaSize);
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
                    if (SuspectBlip.Exists()) { SuspectBlip.Alpha = 1f; }

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
                    if (Suspect2Blip.Exists()) { Suspect2Blip.Alpha = 1f; }

                    // Delete SearchArea
                    if (SearchArea.Exists()) { SearchArea.Delete(); }

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has found {Suspect2Persona.FullName} (Suspect2)");
                }
                #endregion

                #region PedDetained
                if (Functions.IsPedStoppedByPlayer(Suspect) && !PedDetained && Suspect.Exists())
                {
                    // Set PedDetained
                    PedDetained = true;

                    // Delete SuspectBlip
                    if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has detained {SuspectPersona.FullName} (Suspect)");
                }
                #endregion

                #region PlayerLeft
                if (MainPlayer.Position.DistanceTo(CalloutPosition) > Settings.SearchAreaSize * 3.5f && PlayerArrived && !PedFound)
                {
                    // Set PlayerArrived
                    PlayerArrived = false;

                    // Disable SuspectBlip
                    if (SuspectBlip.Exists()) { SuspectBlip.Alpha = 0f; }

                    // Delete SearchArea
                    if (SearchArea.Exists()) { SearchArea.Delete(); }

                    // Create EntranceBlip
                    EntranceBlip = new Blip(Entrance);

                    // Enable Route
                    if (EntranceBlip.Exists()) { EntranceBlip.IsRouteEnabled = true; }

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