using EmergencyCallouts.Essential;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Engine.UI;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using RAGENativeUI;
using System;
using static EmergencyCallouts.Essential.Color;
using static EmergencyCallouts.Essential.Helper;
using Entity = EmergencyCallouts.Essential.Helper.Entity;

namespace EmergencyCallouts.Callouts
{
    [CalloutInfo("Trespassing", CalloutProbability.Medium)]
    public class Trespassing : Callout
    {
        bool CalloutActive;
        bool PlayerArrived;
        bool PedFound;
        bool PedDetained;
        bool DialogueStarted;
        bool StopChecking;

        Vector3 Entrance;
        Vector3 Center;

        // Main
        #region Positions
        readonly Vector3[] CalloutPositions =
        {
            new Vector3(512.01f, -610.720f, 24.4312f),  // La Mesa Railyard
            new Vector3(-1106.7f, -1975.50f, 24.562f),  // LSIA Scrapyard
            new Vector3(1225.66f, -2923.435f, 9.4783f), // Terminal
            new Vector3(2165.78f, 4758.762f, 42.0235f), // McKenzie Airstrip
            new Vector3(191.53f, 2840.427f, 44.50375f), // Joshua Road Loading Dock
            new Vector3(426.6624f, 6549.066f, 27.601f), // Paleto Barn
        };
        #endregion

        // La Mesa Railyard
        #region Positions
        readonly Vector3[] RailyardHidingPositions =
        {
            new Vector3(488.5f, -631f, 24.98f),   // Alley
            new Vector3(523.5f, -563f, 24.765f),  // Power Unit 1
            new Vector3(524.5f, -592f, 24.788f),  // Power Unit 2
            new Vector3(487f, -555.5f, 25.992f),  // Container
            new Vector3(498f, -532f, 24.75114f),  // Pipes
            new Vector3(532f, -560.50f, 24.800f), // Between Train Carrier
            new Vector3(492f, -588.5f, 24.7189f), // Corner Garbage Collector
            new Vector3(493f, -579.913f, 24.57f), // Small Wooden Crate
            new Vector3(481.693f, -591f, 24.75f), // Alley 2
        };

        readonly float[] RailyardHidingPositionsHeadings =
        {
            146f,
            187f,
            15f,
            350f,
            80f,
            80f,
            1f,
            186f,
            300f,
        };

        readonly Vector3[] RailyardManagerPositions =
        {
            new Vector3(495.3361f, -585.4279f, 24.73708f), // Boxes
            new Vector3(495.5332f, -577.2258f, 24.65661f), // Boxes 2
            new Vector3(485.5925f, -634.8498f, 24.92816f), // Crates
        };

        readonly float[] RailyardManagerHeadings =
        {
            90f, // Boxes
            55f, // Boxes 2
            112f // Crates
        };

        readonly Vector3[] RailyardArsonPositions =
        {
            new Vector3(485f, -636.5899f, 25.02777f), // Alley
            new Vector3(522.1501f, -592.4759f, 25f),  // Power Unit
            new Vector3(500f, -609.7313f, 24.75132f), // Building 1
            new Vector3(493f, -573.0732f, 24.59121f)  // Building 2
        };

        readonly Vector3 RailyardWeldingPosition = new Vector3(491.9123f, -554.114f, 24.7505f); // Container
        readonly float RailyardWeldingHeading = 212f;
        #endregion

        // LSC Scrapyard
        #region Positions
        readonly Vector3[] ScrapyardHidingPositions =
        {
            new Vector3(-1155.449f, -2030.024f, 13.16065f), // Lifter
            new Vector3(-1179.068f, -2081.575f, 13.83974f), // Red Small Container
            new Vector3(-1180.712f, -2072.301f, 14.45590f), // Garbage container
            new Vector3(-1171.513f, -2071.196f, 13.96500f), // Abandoned Train Carrier
            new Vector3(-1181.182f, -2046.646f, 13.92571f), // Abandoned bus
        };

        readonly float[] ScrapyardHidingPositionsHeadings =
        {
            222f,
            346f,
            270f,
            145f,
            156f,
        };

        readonly Vector3[] ScrapyardManagerPositions =
        {
            new Vector3(-1161.378f, -2061.15f, 13.77043f),  // Huge Gas Containers
            new Vector3(-1157.412f, -2032.295f, 13.16054f), // Industrial Crane
            new Vector3(-1180.037f, -2058.742f, 14.09963f), // Casual Spot
        };

        readonly float[] ScrapyardManagerHeadings =
        {
            224f, 
            343f, 
            262f,
        };

        readonly Vector3[] ScrapyardArsonPositions =
        {
            new Vector3(-1157.412f, -2032.295f, 13.16054f), // Industrial Crane
            new Vector3(-1161.378f, -2061.15f, 13.77043f),  // Huge Gas Containers
            new Vector3(-1167.67f, -2044.833f, 14.02154f),  // Small Boxes
        };

        readonly Vector3 ScrapyardWeldingPosition = new Vector3(-1151.357f, -2034.422f, 13.16053f);
        readonly float ScrapyardWeldingHeading = 306.35f;
        #endregion

        // Terminal
        #region Positions
        readonly Vector3[] TerminalHidingPositions =
        {
            new Vector3(1249.729f, -2887.772f, 9.319264f), // Rear
            new Vector3(1242.891f, -2947.398f, 9.319264f), // Middle
            new Vector3(1236.903f, -2955.032f, 9.319268f), // Middle 2
            new Vector3(1238.818f, -3006.345f, 9.319253f), // Front
            new Vector3(1227.453f, -3009.943f, 9.319252f), // Front 2
        };

        readonly float[] TerminalHidingPositionsHeadings =
        {
            128.07f,
            160.45f,
            2.73f,
            92.93f,
            347.75f,
        };

        readonly Vector3[] TerminalManagerPositions =
        {
            new Vector3(1228.202f, -2970.058f, 9.319256f), // Tool Cabinet
            new Vector3(1238.923f, -2940.416f, 9.319255f), // Tool Cabinet 2
            new Vector3(1229.698f, -2908.376f, 9.319265f), // Boxes
        };

        readonly float[] TerminalManagerHeadings =
        {
            81.28f,
            30.57f,
            310.89f,
        };

        readonly Vector3[] TerminalArsonPositions =
        {
            new Vector3(1240.061f, -2889.315f, 9.319265f), // Pallets
            new Vector3(1251.003f, -2908.396f, 9.319266f), // Fuel Barrels
            new Vector3(1243.456f, -2953.213f, 9.319252f), // Fuel Barrels 2
        };

        readonly Vector3 TerminalWeldingPosition = new Vector3(1234.051f, -3022.098f, 10.96785f); // Front Containers
        readonly float TerminalWeldingHeading = 279.74f;
        #endregion

        // McKenzie Airstrip
        #region Positions
        readonly Vector3[] AirstripHidingPositions =
        {
            new Vector3(2149.073f, 4781.637f, 41.01651f), // Behind Hangar Garbage Container
            new Vector3(2121.007f, 4783.326f, 40.97028f), // Hangar Desk
            new Vector3(2120.194f, 4774.568f, 41.17796f), // Outside Cement Mixer
            new Vector3(2093.352f, 4738.548f, 41.3352f),  // Gas Tank
            new Vector3(2112.155f, 4759.638f, 41.25103f), // Gas Tank & Scrap holder
        };

        readonly float[] AirstripHidingPositionsHeadings =
        {
            200f,
            220f,
            120f,
            190f,
            5f,
        };

        readonly Vector3[] AirstripManagerPositions =
        {
            new Vector3(2137.664f, 4791.458f, 40.9702f), // Hangar Table
            new Vector3(2135.579f, 4772.35f, 40.97029f), // Red Tool Storage
            new Vector3(2144.962f, 4776.65f, 40.97034f), // Pile of Boxes
        };

        readonly float[] AirstripManagerHeadings =
        {
            290f,
            190f,
            317f,
        };

        readonly Vector3[] AirstripArsonPositions =
        {
            new Vector3(2144.962f, 4776.65f, 40.97034f), // Pile of Boxes
            new Vector3(2108.356f, 4762.68f, 41.04375f), // Gas Tank
            new Vector3(2125.861f, 4774.83f, 40.97033f), // Pile of Boxes 2
        };

        readonly Vector3 AirstripWeldingPosition = new Vector3(2135.423f, 4772.376f, 40.97033f);
        readonly float AirstripWeldingHeading = 189.83f;
        #endregion

        // Joshua Road Loading Dock
        #region Positions
        readonly Vector3[] LoadingDockHidingPositions =
        {
            new Vector3(216.5567f, 2808.267f, 45.65519f), // Pile Of Pallets
            new Vector3(223.5032f, 2802.028f, 45.65519f), // Containers
            new Vector3(167.6332f, 2736.069f, 43.37733f), // Containers 2
            new Vector3(193.4705f, 2766.353f, 43.42632f), // Truck
            new Vector3(163.1098f, 2770.249f, 45.69321f), // Red Container
        };

        readonly float[] LoadingDockHidingHeadings =
        {
            182.41f,
            43.53f,
            7.96f,
            183.06f,
            9.44f,
        };

        readonly Vector3[] LoadingDockManagerPositions =
        {
            new Vector3(221.0605f, 2774.253f, 45.65525f), // Barrels
            new Vector3(221.58971f, 2734.374f, 42.9960f), // Plyboards
            new Vector3(219.1572f, 2806.581f, 45.65519f), // Barrels 2
        };

        readonly float[] LoadingDockManagerHeadings =
        {
            148.58f,
            214.67f,
            36.07f,
        };

        readonly Vector3[] LoadingDockArsonPositions =
        {
            new Vector3(202.8873f, 2776.132f, 45.65527f), // Plyboards
            new Vector3(164.554f, 2777.1820f, 45.70289f), // Storage Tank
            new Vector3(197.8674f, 2803.913f, 45.65517f), // Generator
        };

        readonly Vector3 LoadingDockWeldingPosition = new Vector3(221.1813f, 2746.937f, 43.3394f);
        readonly float LoadingDockWeldingHeading = 268.66f;
        #endregion

        // Paleto Bay Barn
        #region Positions
        readonly Vector3[] BarnHidingPositions =
        {
            new Vector3(435.1073f, 6456.916f, 28.74582f), // Back of the barn
            new Vector3(426.8463f, 6479.765f, 28.86706f), // Front of the barn
            new Vector3(436.3864f, 6502.764f, 28.77272f), // Outside overhang
            new Vector3(399.7721f, 6474.169f, 29.33945f), // Outside Garbage Container
            new Vector3(432.5182f, 6499.175f, 28.89931f), // Bushes
        };

        readonly float[] BarnHidingHeadings =
        {
            337.39f,
            185.75f,
            29.36f,
            256.73f,
            121.03f,
        };

        readonly Vector3[] BarnManagerPositions =
        {
            new Vector3(411.7186f, 6494.792f, 28.13385f), // Generator
            new Vector3(430.6472f, 6502.231f, 28.71397f), // Shed
            new Vector3(425.288f, 6467.4321f, 28.79181f), // Barn
        };

        readonly float[] BarnManagerHeadings =
        {
            41.61f,
            97.09f,
            19.64f,
        };

        readonly Vector3 BarnArsonPosition = new Vector3(419.651f, 6467.322f, 28.82159f);

        readonly Vector3 BarnWeldingPosition = new Vector3();
        readonly float BarnWeldingHeading = 0f;
        #endregion

        Vehicle BarnVehicle;

        static Ped Suspect;
        Ped Guard;
        Persona SuspectPersona;

        Blip SuspectBlip;
        Blip GuardBlip;
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
                    CalloutArea = World.GetStreetName(loc);
                }
            }

            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, Settings.SearchAreaSize / 2.5f);

            CalloutMessage = "Trespassing";
            CalloutDetails = "Someone reported a person trespassing on private property.";
            CalloutScenario = GetRandomScenarioNumber(5);

            Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT CRIME_TRESPASSING IN_OR_ON_POSITION", CalloutPosition);

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
                Log.CalloutAccepted(CalloutMessage, CalloutScenario);

                // Accept Messages
                Display.AcceptNotification(CalloutDetails);
                Display.AcceptSubtitle($"Go to the ~r~{CalloutMessage}~s~ in ~y~{CalloutArea}~s~.");

                // EntranceBlip
                EntranceBlip = new Blip(Entrance);

                // Suspect
                Suspect = new Ped(CalloutPosition);
                SuspectPersona = Functions.GetPersonaForPed(Suspect);
                Suspect.SetDefaults();
                Log.PedCreation(Suspect, PedCategory.Suspect);

                // SuspectBlip
                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColor(Colors.Yellow);
                SuspectBlip.ScaleForPed();
                SuspectBlip.Disable();
               
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

                // Main Positioning
                #region Positioning
                if (CalloutPosition == CalloutPositions[0]) // La Mesa Railyard
                {
                    Center = new Vector3(512f, -610.72f, 24.43f);
                    Entrance = new Vector3(510.59f, -666.95f, 24.40f);
                    EntranceBlip.Position = Entrance;
                }
                else if (CalloutPosition == CalloutPositions[1]) // LSC Scrapyard
                {
                    Center = new Vector3(-1170.024f, -2045.655f, 14.22536f);
                    Entrance = new Vector3(-1156.879f, -1988.801f, 13.16036f);
                    EntranceBlip.Position = Entrance;
                }
                else if (CalloutPosition == CalloutPositions[2]) // Terminal
                {
                    Center = new Vector3(1254.056f, -2948.477f, 9.319256f);
                    Entrance = new Vector3(1218.99f, -2915.958f, 5.866064f);
                    EntranceBlip.Position = Entrance;
                }
                else if (CalloutPosition == CalloutPositions[3]) // McKenzie Airstrip
                {
                    Center = new Vector3(2118.948f, 4802.422f, 41.19594f);
                    Entrance = new Vector3(2165.78f, 4758.762f, 42f);
                    EntranceBlip.Position = Entrance;
                }
                else if (CalloutPosition == CalloutPositions[4]) // Joshua Road Loading Dock
                {
                    Center = new Vector3(195.43f, 2786.759f, 45.65519f);
                    Entrance = new Vector3(191.53f, 2840.427f, 44.50375f);
                    EntranceBlip.Position = Entrance;
                }
                else if (CalloutPosition == CalloutPositions[5]) // Paleto Bay Barn
                {
                    Center = new Vector3(424.5334f, 6508.625f, 27.75672f);
                    Entrance = new Vector3(426.6624f, 6549.066f, 27.6012f);
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

                Handle.DecreaseSearchArea(SearchArea, Suspect, 10);

                // Enabling Route
                EntranceBlip.EnableRoute();
                Game.LogTrivial("[Emergency Callouts]: Enabled route to EntranceBlip");
            }
            catch (Exception e)
            {
                Log.CalloutException(this, "CalloutHandler", e);
            }
            #endregion
        }

        private void RetrieveHidingPosition()
        {
            #region Positions
            if (CalloutPosition == CalloutPositions[0]) // La Mesa Railyard
            {
                int RailyardHidingSpotNum = random.Next(RailyardHidingPositions.Length);
                Suspect.Position = RailyardHidingPositions[RailyardHidingSpotNum];
                Suspect.Heading = RailyardHidingPositionsHeadings[RailyardHidingSpotNum];
            }
            else if (CalloutPosition == CalloutPositions[1]) // LSC Scrapyard
            {
                int ScrapyardHidingSpotNum = random.Next(ScrapyardHidingPositions.Length);
                Suspect.Position = ScrapyardHidingPositions[ScrapyardHidingSpotNum];
                Suspect.Heading = ScrapyardHidingPositionsHeadings[ScrapyardHidingSpotNum];
            }
            else if (CalloutPosition == CalloutPositions[2]) // Terminal
            {
                int AirstripHidingSpotNum = random.Next(TerminalHidingPositions.Length);
                Suspect.Position = TerminalHidingPositions[AirstripHidingSpotNum];
                Suspect.Heading = TerminalHidingPositionsHeadings[AirstripHidingSpotNum];
                Settings.SearchAreaSize -= 15;
            }
            else if (CalloutPosition == CalloutPositions[3]) // McKenzie Airstrip
            {
                int AirstripHidingSpotNum = random.Next(AirstripHidingPositions.Length);
                Suspect.Position = AirstripHidingPositions[AirstripHidingSpotNum];
                Suspect.Heading = AirstripHidingPositionsHeadings[AirstripHidingSpotNum];
            }
            else if (CalloutPosition == CalloutPositions[4]) // Joshua Road Loading Dock
            {
                int AirstripHidingSpotNum = random.Next(LoadingDockHidingPositions.Length);
                Suspect.Position = LoadingDockHidingPositions[AirstripHidingSpotNum];
                Suspect.Heading = LoadingDockHidingHeadings[AirstripHidingSpotNum];
            }
            else if (CalloutPosition == CalloutPositions[5]) // Paleto Bay Barn
            {
                int AirstripHidingSpotNum = random.Next(BarnHidingPositions.Length);
                Suspect.Position = BarnHidingPositions[AirstripHidingSpotNum];
                Suspect.Heading = BarnHidingHeadings[AirstripHidingSpotNum];
            }

            Suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@inspect@crouch@male_a@base"), "base", 4f, AnimationFlags.StayInEndFrame);
            Game.LogTrivial($"[Emergency Callouts]: {SuspectPersona.FullName} now playing animation");
            #endregion
        }

        private void RetrieveManagerPosition()
        {
            #region Positions
            if (Suspect.Exists()) { Suspect.Delete(); }

            if (CalloutPosition == CalloutPositions[0]) // La Mesa Railyard
            {
                Suspect = new Ped("ig_lifeinvad_01", CalloutPosition, 0f);
                Suspect.SetDefaults();

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColor(Colors.Yellow);
                SuspectBlip.ScaleForPed();
                SuspectBlip.Disable();

                int ManagerPositionNum = random.Next(RailyardManagerPositions.Length);
                Suspect.Position = RailyardManagerPositions[ManagerPositionNum];
                Suspect.Heading = RailyardManagerHeadings[ManagerPositionNum];
            }
            else if (CalloutPosition == CalloutPositions[1]) // LSC Scrapyard
            {
                Suspect = new Ped("ig_chef", CalloutPosition, 0f);
                Suspect.SetDefaults();

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColor(Colors.Yellow);
                SuspectBlip.ScaleForPed();
                SuspectBlip.Disable();

                int ManagerPositionNum = random.Next(ScrapyardManagerPositions.Length);
                Suspect.Position = ScrapyardManagerPositions[ManagerPositionNum];
                Suspect.Heading = ScrapyardManagerHeadings[ManagerPositionNum];
            }
            else if (CalloutPosition == CalloutPositions[2]) // Terminal
            {
                Suspect = new Ped("ig_chef", CalloutPosition, 0f);
                Suspect.SetDefaults();

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColor(Colors.Yellow);
                SuspectBlip.ScaleForPed();
                SuspectBlip.Disable();

                int ManagerPositionNum = random.Next(TerminalManagerPositions.Length);
                Suspect.Position = TerminalManagerPositions[ManagerPositionNum];
                Suspect.Heading = TerminalManagerHeadings[ManagerPositionNum];
            }
            else if (CalloutPosition == CalloutPositions[3]) // McKenzie Airstrip
            {
                Suspect = new Ped("player_two", CalloutPosition, 0f);
                Suspect.SetDefaults();

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColor(Colors.Yellow);
                SuspectBlip.ScaleForPed();
                SuspectBlip.Disable();

                int ManagerPositionNum = random.Next(AirstripManagerPositions.Length);
                Suspect.Position = AirstripManagerPositions[ManagerPositionNum];
                Suspect.Heading = AirstripManagerHeadings[ManagerPositionNum];
            }
            else if (CalloutPosition == CalloutPositions[4]) // Joshua Road Loading Dock
            {
                Suspect = new Ped("player_two", CalloutPosition, 0f);
                Suspect.SetDefaults();

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColor(Colors.Yellow);
                SuspectBlip.ScaleForPed();
                SuspectBlip.Disable();

                int ManagerPositionNum = random.Next(LoadingDockManagerPositions.Length);
                Suspect.Position = LoadingDockManagerPositions[ManagerPositionNum];
                Suspect.Heading = LoadingDockManagerHeadings[ManagerPositionNum];
            }
            else if (CalloutPosition == CalloutPositions[5]) // Paleto Bay Barn
            {
                Suspect = new Ped("player_two", CalloutPosition, 0f);
                Suspect.SetDefaults();

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColor(Colors.Yellow);
                SuspectBlip.ScaleForPed();
                SuspectBlip.Disable();

                int ManagerPositionNum = random.Next(BarnManagerPositions.Length);
                Suspect.Position = BarnManagerPositions[ManagerPositionNum];
                Suspect.Heading = BarnManagerHeadings[ManagerPositionNum];
            }

            Suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@inspect@crouch@male_a@base"), "base", 4f, AnimationFlags.StayInEndFrame);
            Game.LogTrivial($"[Emergency Callouts]: {SuspectPersona.FullName} now playing animation");
            #endregion
        }

        private void RetrieveArsonPosition()
        {
            #region Positions
            if (CalloutPosition == CalloutPositions[0]) // La Mesa Railyard
            {
                int ArsonPositionNum = random.Next(RailyardArsonPositions.Length);
                Suspect.Position = RailyardArsonPositions[ArsonPositionNum];
            }
            else if (CalloutPosition == CalloutPositions[1]) // LSC Scrapyard
            {
                int ArsonPositionNum = random.Next(ScrapyardArsonPositions.Length);
                Suspect.Position = ScrapyardArsonPositions[ArsonPositionNum];
            }
            else if (CalloutPosition == CalloutPositions[2]) // Terminal
            {
                int ArsonPositionNum = random.Next(TerminalArsonPositions.Length);
                Suspect.Position = TerminalArsonPositions[ArsonPositionNum];
            }
            else if (CalloutPosition == CalloutPositions[3]) // McKenzie Airstrip
            {
                int ArsonPositionNum = random.Next(AirstripArsonPositions.Length);
                Suspect.Position = AirstripArsonPositions[ArsonPositionNum];
            }
            else if (CalloutPosition == CalloutPositions[4]) // Joshua Road Loading Dock
            {
                int ArsonPositionNum = random.Next(LoadingDockArsonPositions.Length);
                Suspect.Position = LoadingDockArsonPositions[ArsonPositionNum];
            }
            else if (CalloutPosition == CalloutPositions[5]) // Paleto Bay Barn
            {
                Suspect.Position = BarnArsonPosition;
            }
            #endregion
        }

        private void RetrieveWeldingPosition()
        {
            #region Positions
            if (CalloutPosition == CalloutPositions[0]) // La Mesa Railyard
            {
                Suspect.Position = RailyardWeldingPosition;
                Suspect.Heading = RailyardWeldingHeading;
            }
            else if (CalloutPosition == CalloutPositions[1]) // LSC Scrapyard
            {
                Suspect.Position = ScrapyardWeldingPosition;
                Suspect.Heading = ScrapyardWeldingHeading;
            }
            else if (CalloutPosition == CalloutPositions[2]) // Terminal
            {
                Suspect.Position = TerminalWeldingPosition;
                Suspect.Heading = TerminalWeldingHeading;
            }
            else if (CalloutPosition == CalloutPositions[3]) // McKenzie Airstrip
            {
                Suspect.Position = AirstripWeldingPosition;
                Suspect.Heading = AirstripWeldingHeading;
            }
            else if (CalloutPosition == CalloutPositions[4]) // Joshua Road Loading Dock
            {
                Suspect.Position = LoadingDockWeldingPosition;
                Suspect.Heading = LoadingDockWeldingHeading;
            }
            else if (CalloutPosition == CalloutPositions[5]) // Paleto Bay Barn
            {
                BarnVehicle = new Vehicle("RUMPO3", new Vector3(414.0602f, 6460.725f, 29.05295f), 44.97f);
                BarnVehicle.IsPersistent = true;
                Suspect.Position = new Vector3(412.6398f, 6459.688f, 28.809f);
                Suspect.Heading = 314.32f;
            }

            Suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_welding@male@base"), "base", 5f, AnimationFlags.Loop);
            Game.LogTrivial($"[Emergency Callouts]: {SuspectPersona.FullName} now playing animation");
            #endregion
        }

        private void Scenario1() // Pursuit
        {
            #region Scenario 1
            try
            {
                // Retrieve Hiding Position
                RetrieveHidingPosition();

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();
                        if (PedFound)
                        {
                            StopChecking = true;

                            // Delete Blips
                            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                            if (SearchArea.Exists()) { SearchArea.Delete(); }
                            if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }
                            Game.LogTrivial("[Emergency Callouts]: Deleted all blips");

                            LHandle pursuit = Functions.CreatePursuit();
                            Game.LogTrivial("[Emergency Callouts]: Created pursuit");

                            Functions.AddPedToPursuit(pursuit, Suspect);
                            Game.LogTrivial($"[Emergency Callouts]: Added {SuspectPersona.FullName} (Suspect) to pursuit");

                            Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                            Game.LogTrivial("[Emergency Callouts]: Set pursuit active for " + PlayerPersona.FullName);

                            Functions.AddPedContraband(Suspect, ContrabandType.Weapon, "Crowbar");
                            Game.LogTrivial($"[Emergency Callouts]: Added (WEAPON_CROWBAR) to {SuspectPersona.FullName} (Suspect) contraband");

                            Play.PursuitAudio();
                            Game.LogTrivial("[Emergency Callouts]: Played pursuit audio");

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

        private void Scenario2() // Surrender
        {
            #region Scenario 2
            try
            {
                // Retrieve Hiding Position
                RetrieveHidingPosition();

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (PedFound)
                        {
                            // Clear Suspect Tasks
                            Suspect.Tasks.Clear();
                            Game.LogTrivial($"[Emergency Callouts]: Cleared {SuspectPersona.FullName} (Suspect) tasks");

                            // Suspect Achieve Player Heading
                            Suspect.Tasks.PutHandsUp(-1, MainPlayer);
                            Game.LogTrivial($"[Emergency Callouts]: {SuspectPersona.FullName} (Suspect) now putting hands up");

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
       
        private void Scenario3() // Manager
        {
            #region Scenario 3
            try
            {
                // Retrieve Manager Position
                RetrieveManagerPosition();

                string[] dialogue =
                {
                    "~y~Person~s~: Can I help you sir? I'm the person in charge.",
                    $"~b~You~s~: Yes, we're looking for a person matching your description, do you have anything to prove that you work here?",
                    "~y~Person~s~: Yes ofcourse, here it is.",
                    $"~b~You~s~: Okay, looks fine to me, when did you last come here?",
                    "~g~Person~s~: A few minutes ago, when my shift started.",
                    $"~b~You~s~: Then the caller must've made a mistake.",
                    "~g~Person~s~: Well, I'm glad he called, we actually have alot of kids sneaking around here.",
                    $"~b~You~s~: Okay, well, I'm going back out on patrol, see you later!",
                    "~g~Person~s~: Goodbye!",
                };

                int line = 0;
                int num = random.Next(RailyardManagerPositions.Length);

                int day = random.Next(1, 31);
                int month = random.Next(1, 13);
                int year = random.Next(DateTime.Now.Year, DateTime.Now.Year + 7);

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColor(Colors.Yellow);
                SuspectBlip.ScaleForPed();
                SuspectBlip.Disable();
                Game.LogTrivial("[Emergency Callouts]: Created SuspectBlip");

                // Inspect animation
                Suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@inspect@crouch@male_a@idles"), "idle_a", 5f, AnimationFlags.Loop);
                Game.LogTrivial($"[Emergency Callouts]: {SuspectPersona.FullName} now playing animation");

                Functions.SetPedCantBeArrestedByPlayer(Suspect, true);
                Game.LogTrivial($"[Emergency Callouts]: Set {SuspectPersona.FullName} (Suspect) cant be arrested by player");

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 3f)
                        {
                            if (Game.IsKeyDown(Settings.TalkKey))
                            {
                                Suspect.Tasks.Clear();
                                Game.LogTrivial($"[Emergency Callouts]: Cleared {SuspectPersona.FullName} (Suspect) tasks");

                                DialogueStarted = true;
                                Game.LogTrivial("[Emergency Callouts]: Dialogue started with " + SuspectPersona.FullName);

                                Suspect.Tasks.AchieveHeading(MainPlayer.Heading - 180);
                                Game.LogTrivial($"[Emergency Callouts]: {SuspectPersona.FullName} (Suspect) achieved player heading");

                                Game.DisplaySubtitle(dialogue[line], 99999);
                                line++;

                                if (line == 3)
                                {
                                    Suspect.Tasks.PlayAnimation(new AnimationDictionary("mp_common"), "givetake1_b", 5f, AnimationFlags.None).WaitForCompletion();
                                    Game.LogTrivial($"[Emergency Callouts]: {SuspectPersona.FullName} now playing animation");

                                    if (CalloutPosition == CalloutPositions[0]) // La Mesa Railyard
                                    {
                                        Game.DisplayNotification("heisthud", "hc_n_ric", "Go Loco Railroad", $"~y~{SuspectPersona.FullName}", $"~b~Position~s~: Manager \n~g~Location~s~: La Mesa \n~c~Valid until {month}/{day}/{year}");
                                    }
                                    else if (CalloutPosition == CalloutPositions[1]) // LSC Scrapyard
                                    {
                                        Game.DisplayNotification("heisthud", "hc_n_che", "Los Santos Customs", $"~y~{SuspectPersona.FullName}", $"~b~Position~s~: Manager \n~g~Location~s~: Los Santos Int'l \n~c~Valid until {month}/{day}/{year}");
                                    }
                                    else if (CalloutPosition == CalloutPositions[2]) // Terminal
                                    {
                                        Game.DisplayNotification("heisthud", "hc_n_che", "Daisy-Lee", $"~y~{SuspectPersona.FullName}", $"~b~Position~s~: Manager \n~g~Ship~s~: Daisy-Lee \n~c~Valid until {month}/{day}/{year}");
                                    }
                                    else if (CalloutPosition == CalloutPositions[3]) // McKenzie Airstrip
                                    {
                                        SuspectPersona.Forename = "Trevor";
                                        SuspectPersona.Surname = "Philips";
                                        SuspectPersona.Wanted = true;
                                        Game.DisplayNotification("heisthud", "hc_trevor", "Trevor Philips Industries", $"~y~{SuspectPersona.FullName}", $"~b~Position~s~: CEO \n~g~Location~s~: Grapeseed \n~c~The best drugs you can buy!");
                                    }
                                    else if (CalloutPosition == CalloutPositions[4]) // Joshua Road Loading Dock
                                    {
                                        Game.DisplayNotification("heisthud", "hc_trevor", "Mediterranean Shipping Company", $"~y~{SuspectPersona.FullName}", $"~b~Position~s~: Manager \n~g~Location~s~: Blaine County \n~c~Valid until {month}/{day}/{year}");
                                    }
                                    else if (CalloutPosition == CalloutPositions[5]) // Paleto Barn
                                    {
                                        Game.DisplayNotification("heisthud", "hc_trevor", "Farm Company Nam Here", $"~y~{SuspectPersona.FullName}", $"~b~Position~s~: CEO \n~g~Location~s~: Paleto Bay \n~c~Valid until {month}/{day}/{year}");
                                    }

                                    Game.LogTrivial($"[Emergency Callouts]: Displayed {SuspectPersona.FullName}'s credentials");
                                }

                                if (line == 4)
                                {
                                    MainPlayer.Tasks.PlayAnimation(new AnimationDictionary("mp_common"), "givetake1_b", 5f, AnimationFlags.None);
                                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} now playing animation");

                                    SuspectBlip.SetColor(Colors.Green);
                                    Game.LogTrivial("[Emergency Callouts]: Changed SuspectBlip color to green");
                                }

                                if (line == dialogue.Length)
                                {
                                    GameFiber.Sleep(3000);
                                    Functions.StopCurrentCallout();
                                    //Handle.CalloutEnding();
                                    break;
                                }
                                GameFiber.Sleep(500);
                            }
                            else
                            {
                                if (DialogueStarted == false)
                                {
                                    Game.DisplayHelp("Press ~y~Y~s~ to talk to the ~y~suspect~s~.");
                                }
                            }
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

        private void Scenario4() // Attempted Arson
        {
            #region Scenario 4
            try
            {
                // Retrieve Fire Position
                RetrieveArsonPosition();

                // Give Suspect Weapon
                Suspect.Inventory.GiveNewWeapon("WEAPON_PETROLCAN", -1, true);
                Game.LogTrivial($"[Emergency Callouts]: Added (WEAPON_PETROLCAN) to Suspect inventory");

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (PedFound)
                        {
                            // Clear Suspect Tasks
                            Suspect.Tasks.Clear();
                            Game.LogTrivial($"[Emergency Callouts]: Cleared {SuspectPersona.FullName} (Suspect) tasks");

                            // Put Suspect Hands Up
                            Suspect.Tasks.PutHandsUp(-1, MainPlayer);
                            Game.LogTrivial($"[Emergency Callouts]: {SuspectPersona.FullName} (Suspect) now putting hands up");

                            break;
                        }
                        else
                        {
                            Suspect.Tasks.FireWeaponAt(Suspect.Position, -1, FiringPattern.FullAutomatic); // Fires weapon at Suspect Position, might not work.
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

        private void Scenario5() // Knocked Out Guard
        {
            #region Scenario 5
            try
            {
                // Retrieve Welding Position
                RetrieveWeldingPosition();

                // Change SuspectBlip color
                SuspectBlip.SetColor(Colors.Red);
                Game.LogTrivial("[Emergency Callouts]: Changed SuspectBlip color to red");

                // Guard
                Guard = new Ped("csb_prolsec", CalloutPosition.Around2D(5f), 0f);
                Guard.SetDefaults();
                Log.PedCreation(Guard, PedCategory.Guard);

                // Kill Guard
                if (Guard.Exists()) { Guard.Kill(); }
                Game.LogTrivial("[Emergency Callouts]: Killed Guard");

                // GuardBlip
                GuardBlip = Guard.AttachBlip();
                GuardBlip.SetColor(Colors.Blue);
                GuardBlip.ScaleForPed();
                GuardBlip.Disable();


                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Guard.Position) < 5f && Guard.Exists())
                        {
                            // Enable SuspectBlip
                            GuardBlip.Enable();
                            Game.LogTrivial("[Emergency Callouts]: Enabled GuardBlip");

                            Game.DisplayHelp("Look around for the ~r~suspect");
                            break;
                        }
                    }
                });

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 10f && Suspect.Exists())
                        {
                            // Delete Blips
                            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                            if (SearchArea.Exists()) { SearchArea.Delete(); }
                            if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }
                            Game.LogTrivial("[Emergency Callouts]: Deleted all blips");

                            LHandle pursuit = Functions.CreatePursuit();
                            Game.LogTrivial("[Emergency Callouts]: Created pursuit");

                            Functions.AddPedToPursuit(pursuit, Suspect);
                            Game.LogTrivial($"[Emergency Callouts]: Added {SuspectPersona.FullName} (Suspect) to pursuit");

                            Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                            Game.LogTrivial("[Emergency Callouts]: Set pursuit active for " + PlayerPersona.FullName);

                            Play.CodeFourAudio();
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
                Handle.ManualEnding();
                Handle.AutomaticEnding(Suspect);
                Handle.PreventDistanceCrash(CalloutPosition, PlayerArrived, PedFound);
                Handle.PreventFirstResponderCrash(Suspect, Guard);

                #region PlayerArrived
                if (MainPlayer.Position.DistanceTo(Entrance) < 15f && !PlayerArrived)
                {
                    // Set PlayerArrived
                    PlayerArrived = true;

                    // Display Arriving Subtitle
                    Game.DisplaySubtitle("Find the ~y~trespasser~s~ in the ~y~area~s~.", 10000);

                    // Disable route
                    EntranceBlip.DisableRoute();

                    // Delete EntranceBlip
                    if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

                    // Create SearchArea
                    SearchArea = new Blip(Center, Settings.SearchAreaSize + 25f);
                    SearchArea.SetColor(Colors.Yellow);
                    SearchArea.Alpha = 0.5f;

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has arrived on scene");

                    GameFiber.Sleep(15000);
                    Game.DisplayHelp("The ~y~suspect~s~ is likely hidden somewhere");
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

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has found the suspect");
                }
                #endregion

                #region PedDetained
                if (Suspect.IsPedDetained() && !PedDetained && Suspect.Exists())
                {
                    // Set PedDetained
                    PedDetained = true;
                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has detained the suspect");

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

                #region PlayerClimbing
                if (MainPlayer.IsClimbing)
                {
                    Game.DisplayHelp("~p~Clue~s~: The ~y~susect~s~ has not used a ladder", 5000);
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
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            if (Guard.Exists()) { Guard.Dismiss(); }
            if (GuardBlip.Exists()) { GuardBlip.Delete(); }
            if (SearchArea.Exists()) { SearchArea.Delete(); }
            if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

            Display.HideSubtitle();
            Display.DetachMessage();
            Log.CalloutEnded(CalloutMessage, CalloutScenario);
        }
    }
}