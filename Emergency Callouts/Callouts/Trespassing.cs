using EmergencyCallouts.Essential;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using RAGENativeUI;
using System;
using System.Reflection;
using static EmergencyCallouts.Essential.Color;
using static EmergencyCallouts.Essential.Helper;

namespace EmergencyCallouts.Callouts
{
    [CalloutInfo("[EC] Trespassing", CalloutProbability.Medium)]
    public class Trespassing : Callout
    {
        bool CalloutActive;
        bool PlayerArrived;
        bool PedFound;
        bool PedDetained;
        bool DialogueStarted;

        Vector3 Entrance;
        Vector3 Center;

        // Main
        #region Positions
        readonly Vector3[] CalloutPositions =
        {
            new Vector3(512.01f, -610.720f, 24.4312f),    // La Mesa Railyard
            new Vector3(-1106.7f, -1975.50f, 24.562f),    // LSIA Scrapyard
            new Vector3(1225.66f, -2923.435f, 9.4783f),   // Terminal
            new Vector3(808.5509f, 1275.401f, 359.9711f), // County
            new Vector3(2165.78f, 4758.762f, 42.0235f),   // McKenzie Airstrip
            new Vector3(191.53f, 2840.427f, 44.50375f),   // Joshua Road Loading Dock
            new Vector3(426.6624f, 6549.066f, 27.601f),   // Paleto Barn
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
        #endregion

        // LSC Scrapyard
        #region Positions
        readonly Vector3[] ScrapyardHidingPositions =
        {
            new Vector3(-1155.449f, -2030.024f, 13.16065f), // Lifter
            new Vector3(-1178.147f, -2083.881f, 13.41349f), // Red Small Container
            new Vector3(-1180.712f, -2072.301f, 14.45590f), // Garbage container
            new Vector3(-1168.165f, -2052.083f, 14.43985f), // Inside bus
            new Vector3(-1181.182f, -2046.646f, 13.92571f), // Outside bus
            new Vector3(-1154.528f, -2052.879f, 13.91131f), // Inside bus water tower
        };

        readonly float[] ScrapyardHidingPositionsHeadings =
        {
            1.42f,
            346f,
            270f,
            66.24f,
            156f,
            102.71f,
        };

        readonly Vector3[] ScrapyardManagerPositions =
        {
            new Vector3(-1161.296f, -2060.376f, 13.81086f), // Huge Gas Containers
            new Vector3(-1157.456f, -2033.004f, 13.16054f), // Industrial Crane
            new Vector3(-1179.293f, -2059.016f, 14.13962f), // Casual Spot
        };

        readonly float[] ScrapyardManagerHeadings =
        {
            45.99f,
            2.16f,
            74.74f,
        };
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
        #endregion

        // County
        #region Positions
        readonly Vector3[] CountyHidingPositions =
        {
            new Vector3(762.8389f, 1316.628f, 359.9371f),  // Boards
            new Vector3(752.77f, 1317.433f, 359.8556f),    // Behind Container
            new Vector3(720.8849f, 1296.344f, 360.2961f),  // Office
            new Vector3(664.7129f, 1287.845f, 360.2961f),  // Back Boards
            new Vector3(757.0978f, 1265.797f, 360.2964f),  // Middle
        };

        readonly float[] CountyHidingPositionsHeadings =
        {
            104.90f,
            101.08f,
            63.67f,
            163.50f,
            259.55f,
        };

        readonly Vector3[] CountyManagerPositions =
        {
            new Vector3(744.5788f, 1306.545f, 360.0878f),  // Office
            new Vector3(718.0433f, 1291.299f, 360.2962f),  // Office 2
            new Vector3(686.3436f, 1285.599f, 360.2962f),  // Back
        };

        readonly float[] CountyManagerHeadings =
        {
            190.66f,
            188.67f,
            88.56f,
        };
        #endregion

        // McKenzie Field
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
            new Vector3(2139.753f, 4791.316f, 40.97028f), // Pile of Boxes 
            new Vector3(2135.525f, 4772.93f, 40.97032f),  // Red Tool Storage
            new Vector3(2144.859f, 4779.579f, 40.97027f), // Pile of Boxes 2
        };

        readonly float[] AirstripManagerHeadings =
        {
            282.48f,
            187.58f,
            234.06f,
        };

        readonly Vector3[] AirstripArsonPositions =
        {
            new Vector3(2144.962f, 4776.65f, 40.97034f), // Pile of Boxes
            new Vector3(2108.356f, 4762.68f, 41.04375f), // Gas Tank
            new Vector3(2125.861f, 4774.83f, 40.97033f), // Pile of Boxes 2
        };
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
        #endregion

        // Zancudo Grain Growers
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
            new Vector3(409.8339f, 6493.519f, 28.12436f), // Generator
            new Vector3(430.6472f, 6502.231f, 28.71397f), // Shed
            new Vector3(425.288f, 6467.4321f, 28.79181f), // Barn
        };

        readonly float[] BarnManagerHeadings =
        {
            327.94f,
            97.09f,
            19.64f,
        };

        readonly Vector3 BarnArsonPosition = new Vector3(419.651f, 6467.322f, 28.82159f);
        #endregion

        readonly Rage.Object WeldingDevice = new Rage.Object(new Model("prop_weld_torch"), new Vector3(0, 0, 0));
        readonly Rage.Object Clipboard = new Rage.Object(new Model("p_amb_clipboard_01"), new Vector3(0, 0, 0));
        readonly Rage.Object Phone = new Rage.Object(new Model("prop_police_phone"), new Vector3(0, 0, 0));

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
                    CalloutArea = World.GetStreetName(loc).Replace("Great Ocean Hwy", "Zancudo Grain Growers"); ;
                }
            }

            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, Settings.SearchAreaSize / 2.5f);

            CalloutMessage = "Trespassing";
            CalloutAdvisory = "Reports of a person trespassing on private property.";
            CalloutScenario = random.Next(1, 4);

            Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT CRIME_TRESPASSING IN_OR_ON_POSITION", CalloutPosition);

            return base.OnBeforeCalloutDisplayed();
        }

        public override void OnCalloutDisplayed()
        {
            if (Other.PluginChecker.IsCalloutInterfaceRunning)
            {
                Other.CalloutInterfaceFunctions.SendCalloutDetails(this, "CODE 2", "");
            }

            base.OnCalloutDisplayed();
        }

        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} ignored the callout");
            if (!Other.PluginChecker.IsCalloutInterfaceRunning)
            {
                Functions.PlayScannerAudio("PED_RESPONDING_DISPATCH");
            }

            base.OnCalloutNotAccepted();
        }

        public override bool OnCalloutAccepted()
        {
            try
            {
                // Positioning
                #region Positioning
                if (CalloutPosition == CalloutPositions[0]) // La Mesa Railyard
                {
                    Center = new Vector3(512f, -610.72f, 24.43f);
                    Entrance = new Vector3(510.59f, -666.95f, 24.40f);
                }
                else if (CalloutPosition == CalloutPositions[1]) // LSC Scrapyard
                {
                    Center = new Vector3(-1170.024f, -2045.655f, 14.22536f);
                    Entrance = new Vector3(-1156.879f, -1988.801f, 13.16036f);
                }
                else if (CalloutPosition == CalloutPositions[2]) // Terminal
                {
                    Center = new Vector3(1254.056f, -2948.477f, 9.319256f);
                    Entrance = new Vector3(1218.99f, -2915.958f, 5.866064f);
                }
                else if (CalloutPosition == CalloutPositions[3]) // County
                {
                    Center = new Vector3(737.6351f, 1285.04f, 359.7698f);
                    Entrance = new Vector3(808.5509f, 1275.401f, 359.9711f);
                }
                else if (CalloutPosition == CalloutPositions[4]) // McKenzie Airstrip
                {
                    Center = new Vector3(2118.948f, 4802.422f, 41.19594f);
                    Entrance = new Vector3(2165.78f, 4758.762f, 42f);
                }
                else if (CalloutPosition == CalloutPositions[5]) // Joshua Road Loading Dock
                {
                    Center = new Vector3(195.43f, 2786.759f, 45.65519f);
                    Entrance = new Vector3(191.53f, 2840.427f, 44.50375f);
                }
                else if (CalloutPosition == CalloutPositions[6]) // Zancudo Grain Growers
                {
                    Center = new Vector3(424.5334f, 6508.625f, 27.75672f);
                    Entrance = new Vector3(426.6624f, 6549.066f, 27.6012f);
                }
                #endregion

                // Callout Accepted
                Log.OnCalloutAccepted(CalloutMessage, CalloutScenario);

                // Accept Messages
                Display.AcceptSubtitle(CalloutMessage, CalloutArea);
                Display.OutdatedReminder();

                // EntranceBlip
                EntranceBlip = new Blip(Entrance);
                if (EntranceBlip.Exists()) { EntranceBlip.IsRouteEnabled = true; }

                // Suspect
                Suspect = new Ped(Helper.Entity.GetRandomMaleModel(), Vector3.Zero, 0f);
                SuspectPersona = Functions.GetPersonaForPed(Suspect);
                Suspect.IsPersistent = true;
                Suspect.BlockPermanentEvents = true;

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColorRed();
                SuspectBlip.Scale = (float)Settings.PedBlipScale;
                SuspectBlip.Alpha = 0f;

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
                    case 3:
                        Scenario3();
                        break;
                }

                Log.Creation(Suspect, PedCategory.Suspect);
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            }
            #endregion
        }

        private void RetrieveHidingPosition(Ped suspect)
        {
            #region Positions
            if (CalloutPosition == CalloutPositions[0]) // La Mesa Railyard
            {
                int RailyardHidingSpotNum = random.Next(RailyardHidingPositions.Length);
                suspect.Position = RailyardHidingPositions[RailyardHidingSpotNum];
                suspect.Heading = RailyardHidingPositionsHeadings[RailyardHidingSpotNum];
            }
            else if (CalloutPosition == CalloutPositions[1]) // LSC Scrapyard
            {
                int ScrapyardHidingSpotNum = random.Next(ScrapyardHidingPositions.Length);
                suspect.Position = ScrapyardHidingPositions[ScrapyardHidingSpotNum];
                suspect.Heading = ScrapyardHidingPositionsHeadings[ScrapyardHidingSpotNum];
            }
            else if (CalloutPosition == CalloutPositions[2]) // Terminal
            {
                int AirstripHidingSpotNum = random.Next(TerminalHidingPositions.Length);
                suspect.Position = TerminalHidingPositions[AirstripHidingSpotNum];
                suspect.Heading = TerminalHidingPositionsHeadings[AirstripHidingSpotNum];
            }
            else if (CalloutPosition == CalloutPositions[3]) // County
            {
                int CountyHidingSpotNum = random.Next(CountyHidingPositions.Length);
                suspect.Position = CountyHidingPositions[CountyHidingSpotNum];
                suspect.Heading = CountyHidingPositionsHeadings[CountyHidingSpotNum];
            }
            else if (CalloutPosition == CalloutPositions[4]) // McKenzie Airstrip
            {
                int AirstripHidingSpotNum = random.Next(AirstripHidingPositions.Length);
                suspect.Position = AirstripHidingPositions[AirstripHidingSpotNum];
                suspect.Heading = AirstripHidingPositionsHeadings[AirstripHidingSpotNum];
            }
            else if (CalloutPosition == CalloutPositions[5]) // Joshua Road Loading Dock
            {
                int AirstripHidingSpotNum = random.Next(LoadingDockHidingPositions.Length);
                suspect.Position = LoadingDockHidingPositions[AirstripHidingSpotNum];
                suspect.Heading = LoadingDockHidingHeadings[AirstripHidingSpotNum];
            }
            else if (CalloutPosition == CalloutPositions[6]) // Paleto Bay Barn
            {
                int AirstripHidingSpotNum = random.Next(BarnHidingPositions.Length);
                suspect.Position = BarnHidingPositions[AirstripHidingSpotNum];
                suspect.Heading = BarnHidingHeadings[AirstripHidingSpotNum];
            }

            suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@inspect@crouch@male_a@base"), "base", 4f, AnimationFlags.StayInEndFrame);
            #endregion
        }

        private void RetrieveManagerPosition()
        {
            #region Positions
            if (Suspect.Exists()) { Suspect.Delete(); }

            if (CalloutPosition == CalloutPositions[0]) // La Mesa Railyard
            {
                Suspect = new Ped("ig_lifeinvad_01", CalloutPosition, 0f);

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColorYellow();
                SuspectBlip.Scale = (float)Settings.PedBlipScale;
                if (SuspectBlip.Exists()) { SuspectBlip.Alpha = 0f; }

                int ManagerPositionNum = random.Next(RailyardManagerPositions.Length);
                Suspect.Position = RailyardManagerPositions[ManagerPositionNum];
                Suspect.Heading = RailyardManagerHeadings[ManagerPositionNum];
            }
            else if (CalloutPosition == CalloutPositions[1]) // LSC Scrapyard
            {
                Suspect = new Ped("ig_chef", CalloutPosition, 0f);

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColorYellow();
                SuspectBlip.Scale = (float)Settings.PedBlipScale;
                if (SuspectBlip.Exists()) { SuspectBlip.Alpha = 0f; }

                int ManagerPositionNum = random.Next(ScrapyardManagerPositions.Length);
                Suspect.Position = ScrapyardManagerPositions[ManagerPositionNum];
                Suspect.Heading = ScrapyardManagerHeadings[ManagerPositionNum];
            }
            else if (CalloutPosition == CalloutPositions[2]) // Terminal
            {
                Suspect = new Ped("mp_m_boatstaff_01", CalloutPosition, 0f);
                Suspect.IsPersistent = true;
                Suspect.BlockPermanentEvents = true;

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColorYellow();
                SuspectBlip.Scale = (float)Settings.PedBlipScale;
                if (SuspectBlip.Exists()) { SuspectBlip.Alpha = 0f; }

                int ManagerPositionNum = random.Next(TerminalManagerPositions.Length);
                Suspect.Position = TerminalManagerPositions[ManagerPositionNum];
                Suspect.Heading = TerminalManagerHeadings[ManagerPositionNum];
            }
            else if (CalloutPosition == CalloutPositions[3]) // McKenzie Field
            {
                Suspect = new Ped("player_two", CalloutPosition, 0f);

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColorYellow();
                SuspectBlip.Scale = (float)Settings.PedBlipScale;
                if (SuspectBlip.Exists()) { SuspectBlip.Alpha = 0f; }

                int ManagerPositionNum = random.Next(AirstripManagerPositions.Length);
                Suspect.Position = AirstripManagerPositions[ManagerPositionNum];
                Suspect.Heading = AirstripManagerHeadings[ManagerPositionNum];
            }
            else if (CalloutPosition == CalloutPositions[4]) // Joshua Road Loading Dock
            {
                Suspect = new Ped("ig_barry", CalloutPosition, 0f);

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColorYellow();
                SuspectBlip.Scale = (float)Settings.PedBlipScale;
                if (SuspectBlip.Exists()) { SuspectBlip.Alpha = 0f; }

                int ManagerPositionNum = random.Next(LoadingDockManagerPositions.Length);
                Suspect.Position = LoadingDockManagerPositions[ManagerPositionNum];
                Suspect.Heading = LoadingDockManagerHeadings[ManagerPositionNum];
            }
            else if (CalloutPosition == CalloutPositions[5]) // Zancudo Grain Growers
            {
                Suspect = new Ped("csb_oscar", CalloutPosition, 0f);

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColorYellow();
                SuspectBlip.Scale = (float)Settings.PedBlipScale;
                if (SuspectBlip.Exists()) { SuspectBlip.Alpha = 0f; }

                int ManagerPositionNum = random.Next(BarnManagerPositions.Length);
                Suspect.Position = BarnManagerPositions[ManagerPositionNum];
                Suspect.Heading = BarnManagerHeadings[ManagerPositionNum];
            }

            Suspect.IsPersistent = true;
            Suspect.BlockPermanentEvents = true;

            Suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@inspect@crouch@male_a@base"), "base", 4f, AnimationFlags.StayInEndFrame);
            #endregion
        }

        private void SuspectDialogue()
        {
            #region Dialogue
            try
            {
                bool stopDialogue = false;
                bool stopDialogue2 = false;
                bool CompletedSuspectDialogue = false;

                // Get Time Of Day Line
                string timeOfDay;
                if (World.TimeOfDay.TotalHours >= 6 && World.TimeOfDay.TotalHours < 12)
                {
                    timeOfDay = "so early?";
                }
                else if (World.TimeOfDay.TotalHours >= 12 && World.TimeOfDay.TotalHours <= 21)
                {
                    timeOfDay = "in the middle of the day?";
                }
                else
                {
                    timeOfDay = "in the middle of the night?";
                }

                // Chance of declining to call property owner
                string playerAnswer = string.Empty;
                string suspectAnswer = string.Empty;


                bool acceptsSuggestion = false;

                int chanceAllow = random.Next(1, 101);

                if (chanceAllow <= Settings.ChanceOfCallingOwner)
                {
                    string[] playerAnswers = new[] { "Ofcourse not, what are you thinking?", "No that'd be unprofessional.", "No?", "Uhm, I'm not even gonna answer that." };
                    string[] suspectAnswers = new[] { "Screw you man, we'll see in court if he presses charges.", "Well I guess that's that.", "That's just great.", "Ofcourse that's your answer", "Ughhhhhh" };

                    int playerAnswerRandom = random.Next(playerAnswers.Length);
                    int suspectAnswerRandom = random.Next(suspectAnswers.Length);

                    playerAnswer = playerAnswers[playerAnswerRandom];
                    suspectAnswer = suspectAnswers[suspectAnswerRandom];

                    acceptsSuggestion = false;
                }
                else
                {
                    string[] playerAnswers = new[] { "Hmm... okay then.", "You know what? Fine.", "No?", "Uhm, I'm not even gonna answer that." };
                    string[] suspectAnswers = new[] { "We need more officers like you sir!", "Hell yeah!", "Thank god that you are the responding officer!", "I knew it :)", "YESSS!" };

                    int playerAnswerRandom = random.Next(playerAnswers.Length);
                    int suspectAnswerRandom = random.Next(suspectAnswers.Length);

                    playerAnswer = playerAnswers[playerAnswerRandom];
                    suspectAnswer = suspectAnswers[suspectAnswerRandom];

                    acceptsSuggestion = true;
                }

                #region Suspect's Dialogue
                string[] suspectLine1 = { "So, what are you doing here ", "What were you doing here ", "Why are you here " };
                string[] suspectLine2 = { "Do you have permission to be here?", "Are you allowed to be here?", "You're not supposed to be here are you?", "You're obviously not allowed to trespass.", "You have no business here right?" };
                string[] suspectLine3 = { "No, but I know the owner.. we chill man, don't ruin my friendship, at least don't tell him!", "Hey please, I know the owner I'm sure he and I can work something out!", "Hey man, I know I messed up but I know the owner and we're pretty chill! Can he and I figure something out?" };
                string[] suspectLine4 = { "I'll be notifying the owner soon, I can tell he's not gonna be happy to hear that you're stealing from him.", "I'll obviously be contacting the owner and it's up to him.", "It's up to the owner if he wants to press charges, not me.", "It's not up to me to decide that." };
                string[] suspectLine5 = { "Can't you just call him?", "Please just call him!", "Oh no... can you please call him for me?", "Please call him for me!" };

                int suspectLine1Random = random.Next(0, suspectLine1.Length);
                int suspectLine2Random = random.Next(0, suspectLine2.Length);
                int suspectLine3Random = random.Next(0, suspectLine3.Length);
                int suspectLine4Random = random.Next(0, suspectLine4.Length);
                int suspectLine5Random = random.Next(0, suspectLine5.Length);


                string[] dialogueSuspect =
                {
                    "~b~You~s~: " + suspectLine1[suspectLine1Random] + timeOfDay,
                    "~b~You~s~: " + suspectLine2[suspectLine2Random],
                    "~y~Suspect~s~: " + suspectLine3[suspectLine3Random],
                    "~b~You~s~: " + suspectLine4[suspectLine4Random],
                    "~y~Suspect~s~: " + suspectLine5[suspectLine5Random],
                    "~b~You~s~: " + playerAnswer,
                    "~y~Suspect~s~: " + suspectAnswer,
                    "~m~Suspect Dialogue Ended",
                };
                #endregion

                #region Owner's Dialogue
                string[] ownerDialogueLine2 = { "Hello, how can I help?", "Good day officer, how can I help you?", "Uh-oh, uhmm... what is it?", "Oh that's not good, what happened?", "Police on the line is never good, what happened?", "Hello, what happened?", "Hi, so what happened?" };
                string[] ownerDialogueLine3 = { "Hi, we caught a person trespassing on your property.", "Hello, I just caught a person trespassing on your property.", "Hello, I just apprehended someone trespassing on your property." };
                string[] ownerDialogueLine4 = { "I don't know what his intentions were, but the suspect says he knows you.", "He says that he knows you.", "The person said that you might help him get out of this." };
                string[] ownerDialogueLine5 = { "What's his name?", "Okay uhm, what's his name?", "What's the name of the person?", "Do you have a name for me?" };
                string[] ownerDialogueLine6 = { "Give me a second. Hey you, what's your name?", "I'll ask him. Hey what's your name?", "I'll get his name real quick..." };
                string[] ownerDialogueLine7 = { "It's ", "My name is ", "The name is ", "That would be " };
                string[] ownerDialogueLine8 = { "His name is ", "The name is ", "It's " };
                string[] ownerDialogueLine9 = { "Okay, then I'm going ahead and do that, have a nice day sir.", "I'll go do that then, have a nice day sir.", "Okay then, have a good day sir." };
                string[] ownerDialogueLine10 = { "You too Officer... uhh...", "You too, and what was your name again?", "Thanks, what was your name again?" };
                string[] ownerDialogueLine11 = { "It's Officer ", "I'm Officer " };
                string[] ownerDialogueLine12 = { "Okay, then I'm going ahead and do that, have a nice day sir.", "I'll go do that then, have a nice day sir.", "Okay then, have a good day sir." };

                int ownerLine2Random = random.Next(0, ownerDialogueLine2.Length);
                int ownerLine3Random = random.Next(0, ownerDialogueLine3.Length);
                int ownerLine4Random = random.Next(0, ownerDialogueLine4.Length);
                int ownerLine5Random = random.Next(0, ownerDialogueLine5.Length);
                int ownerLine6Random = random.Next(0, ownerDialogueLine6.Length);
                int ownerLine7Random = random.Next(0, ownerDialogueLine7.Length);
                int ownerLine8Random = random.Next(0, ownerDialogueLine8.Length);
                int ownerLine9Random = random.Next(0, ownerDialogueLine9.Length);
                int ownerLine10Random = random.Next(0, ownerDialogueLine10.Length);
                int ownerLine11Random = random.Next(0, ownerDialogueLine11.Length);
                int ownerLine12Random = random.Next(0, ownerDialogueLine12.Length);

                #region Owner Answer
                string[] ownerAnswer;

                int chanceCharges = random.Next(1, 101);

                if (chanceCharges <= Settings.ChanceOfPressingCharges)
                {
                    ownerAnswer = new[] {
                        $"{SuspectPersona.Forename}? Yeah screw that guy, you can arrest that person Officer {PlayerPersona.Surname}.",
                        "He got caught this time! Good job, have fun with him!",
                        $"{SuspectPersona.Forename}? Doesn't ring a bell, I'd like to press charges, I'll come by the station ASAP.",
                        "Damn it! Let him rot please.",
                    };
                }
                else
                {
                    ownerAnswer = new[] {
                        $"Ugh, I don't have time for this, you can let that person go Officer {PlayerPersona.Surname}",
                        "I made a few mistakes in the past too, I'll give him a second chance.",
                        "I used to be like him back in the day, turned my life around, you can let him go officer.",
                        $"{SuspectPersona.Forename}? You know what? I'll let it slide this time.",
                    };

                }
                #endregion

                string[] dialogueOwner =
                {
                    $"~b~You~s~: Hello sir, my name is {PlayerPersona.FullName}, I'm with the police department.",
                    "~g~Owner~s~: " + ownerDialogueLine2[ownerLine2Random],
                    "~b~You~s~: " + ownerDialogueLine3[ownerLine3Random],
                    "~b~You~s~: " + ownerDialogueLine4[ownerLine4Random],
                    "~g~Owner~s~: " + ownerDialogueLine5[ownerLine5Random],
                    "~b~You~s~: " + ownerDialogueLine6[ownerLine6Random],
                    $"~r~Suspect~s~: " + ownerDialogueLine7[ownerLine7Random] + SuspectPersona.Forename,
                    $"~b~You~s~: " + ownerDialogueLine8[ownerLine8Random] + SuspectPersona.Forename,
                    "~g~Owner~s~: " + ownerAnswer,
                    "~b~You~s~: " + ownerDialogueLine9[ownerLine9Random],
                    "~g~Owner~s~: " + ownerDialogueLine10[ownerLine10Random],
                    $"~b~You~s~: " + ownerDialogueLine11[ownerLine11Random] + PlayerPersona.Surname,
                    "~g~Owner~s~: " + ownerDialogueLine12[ownerLine12Random],
                    "~m~Call Ended",
                };
                #endregion

                int lineSuspectCount = 0;
                int lineOwnerCount = 0;

                #region Suspect Dialogue
                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (Suspect.IsCuffed)
                        {
                            GameFiber.Sleep(5000);
                            break;
                        }
                    }

                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 5f && Suspect.IsCuffed && Suspect.IsAlive && MainPlayer.IsOnFoot && !CompletedSuspectDialogue)
                        {
                            if (Game.IsKeyDown(Settings.InteractKey) || (Game.IsControllerButtonDown(Settings.ControllerInteractKey) && Settings.AllowController && UIMenu.IsUsingController))
                            {
                                if (!DialogueStarted)
                                {
                                    if (!Functions.IsPedKneelingTaskActive(Suspect)) { Suspect.Tasks.Clear(); }

                                    Game.LogTrivial("[Emergency Callouts]: Dialogue started with " + SuspectPersona.FullName);
                                }

                                DialogueStarted = true;

                                
                                if (!Functions.IsPedKneelingTaskActive(Suspect)) { Suspect.Tasks.AchieveHeading(MainPlayer.Heading - 180f); }

                                Game.DisplaySubtitle(dialogueSuspect[lineSuspectCount], 15000);
                                if (!stopDialogue) { lineSuspectCount++; }

                                Game.LogTrivial("[Emergency Callouts]: Displayed dialogue line " + lineSuspectCount);

                                if (lineSuspectCount == dialogueSuspect.Length)
                                {
                                    stopDialogue = true;
                                    Game.LogTrivial("[Emergency Callouts]: Suspect dialogue ended");

                                    CompletedSuspectDialogue = true;
                                    DialogueStarted = false;
                                    break;
                                }
                            }
                            else
                            {
                                if (!DialogueStarted)
                                {
                                    if (Settings.AllowController && UIMenu.IsUsingController)
                                    {
                                        Game.DisplayHelp($"Press ~{Settings.ControllerInteractKey.GetInstructionalId()}~ to talk to the ~y~suspect");
                                    }
                                    else
                                    {
                                        Game.DisplayHelp($"Press ~{Settings.InteractKey.GetInstructionalId()}~ to talk to the ~y~suspect");
                                    }
                                }
                            }
                        }
                    }
                });
                #endregion

                #region Owner Dialogue
                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (acceptsSuggestion)
                        {
                            if (MainPlayer.Position.DistanceTo(Suspect.Position) < 5f && Suspect.IsCuffed && Suspect.IsAlive && MainPlayer.IsOnFoot && CompletedSuspectDialogue)
                            {
                                if (Game.IsKeyDown(Settings.InteractKey))
                                {
                                    if (!DialogueStarted)
                                    {
                                        GameFiber.Sleep(4000);
                                        Game.LogTrivial("[Emergency Callouts]: Dialogue started with Owner");

                                        // Attach phone to player's hand
                                        int boneIndex = NativeFunction.Natives.GET_PED_BONE_INDEX<int>(MainPlayer, (int)PedBoneId.RightPhHand);
                                        NativeFunction.Natives.ATTACH_ENTITY_TO_ENTITY(Phone, MainPlayer, boneIndex, 0f, 0f, 0f, 0f, 0f, 0f, true, true, false, false, 2, 1);
                                        MainPlayer.Tasks.PlayAnimation("cellphone@", "cellphone_call_listen_base", -1, 2f, -2f, 0, AnimationFlags.Loop | AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask);

                                        // Play phone ringing sound
                                        string path = @"lspdfr\audio\scanner\Emergency Callouts Audio\PHONE_RINGING.wav";
                                        System.Media.SoundPlayer player = new System.Media.SoundPlayer(path);
                                        if (System.IO.File.Exists(path))
                                        {
                                            player.Load();
                                            player.Play();
                                        }

                                        GameFiber.Sleep(12000);
                                        Game.DisplaySubtitle($"~g~Owner~s~: Hello? Who's this?", 15000);
                                        DialogueStarted = true;
                                    }
                                    else
                                    {
                                        Game.DisplaySubtitle(dialogueOwner[lineOwnerCount], 15000);
                                        if (!stopDialogue2) { lineOwnerCount++; }

                                        Game.LogTrivial("[Emergency Callouts]: Displayed dialogue line " + lineOwnerCount);

                                        if (lineOwnerCount == dialogueOwner.Length)
                                        {
                                            stopDialogue2 = true;
                                            Game.LogTrivial("[Emergency Callouts]: Owner Dialogue Ended");

                                            MainPlayer.Tasks.Clear();
                                            if (Phone.Exists()) { Phone.Delete(); }

                                            GameFiber.Sleep(3000);
                                            Handle.AdvancedEndingSequence();
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else if (CompletedSuspectDialogue)
                        {
                            GameFiber.Sleep(3000);
                            Handle.AdvancedEndingSequence();
                            break;
                        }
                    }
                });
                #endregion
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            }
            #endregion
        }

        private void Scenario1() // Surrender
        {
            #region Scenario 1
            try
            {
                // Retrieve Hiding Position
                RetrieveHidingPosition(Suspect);

                // Set Dialogue Active
                SuspectDialogue();

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) <= 5f && Suspect.Exists() && PlayerArrived)
                        {
                            // Clear Suspect Tasks
                            Suspect.Tasks.Clear();

                            // Suspect Achieve Player Heading
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

        private void Scenario2() // Manager
        {
            #region Scenario 2
            try
            {
                // Retrieve Manager Position
                RetrieveManagerPosition();

                string[] dialogue =
                {
                    "~y~Person~s~: Can I help you sir? I'm the person in charge.",
                    $"~b~You~s~: Yes, we're looking for a person matching your description, do you have anything to prove that you work here?",
                    "~y~Person~s~: Yes ofcourse, here it is.",
                    $"~b~You~s~: Okay, looks fine to me, when did you last enter?",
                    "~g~Person~s~: A few minutes ago, when my shift started.",
                    $"~b~You~s~: Then the caller must've made a mistake.",
                    "~g~Person~s~: Well, I'm glad he called, we actually have alot of kids sneaking around here.",
                    $"~b~You~s~: Okay, I'm going to have a look around and make sure there is no-one else.",
                    "~g~Person~s~: Okay, bye.",
                    $"~b~You~s~: Goodbye."
                };

                int line = 0;
                int num = random.Next(RailyardManagerPositions.Length);

                int day = random.Next(1, 31);
                int month = random.Next(1, 13);
                int year = random.Next(DateTime.Now.Year, DateTime.Now.Year + 5);

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColorYellow();
                SuspectBlip.Scale = (float)Settings.PedBlipScale;
                if (SuspectBlip.Exists()) { SuspectBlip.Alpha = 0f; }

                // Clipboard
                int boneIndex = NativeFunction.Natives.GET_PED_BONE_INDEX<int>(Suspect, (int)PedBoneId.LeftPhHand);
                NativeFunction.Natives.ATTACH_ENTITY_TO_ENTITY(Clipboard, Suspect, boneIndex, 0f, 0f, 0f, 0f, 0f, 0f, true, true, false, false, 2, 1);

                // Inspect animation
                Suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_clipboard@male@base"), "base", 5f, AnimationFlags.Loop);

                Functions.SetPedCantBeArrestedByPlayer(Suspect, true);

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 3f && PlayerArrived && Suspect.IsAlive)
                        {
                            if (Game.IsKeyDown(Settings.InteractKey) || (Game.IsControllerButtonDown(Settings.ControllerInteractKey) && Settings.AllowController && UIMenu.IsUsingController))
                            {
                                if (!DialogueStarted)
                                {
                                    if (Clipboard.Exists()) { Clipboard.Delete(); }
                                    if (!Functions.IsPedKneelingTaskActive(Suspect)) { Suspect.Tasks.Clear(); }

                                    Game.LogTrivial("[Emergency Callouts]: Dialogue started with " + SuspectPersona.FullName);
                                }

                                DialogueStarted = true;

                                if (!Functions.IsPedKneelingTaskActive(Suspect)) { Suspect.Tasks.AchieveHeading(MainPlayer.Heading - 180f); }

                                Game.DisplaySubtitle(dialogue[line], 15000);
                                line++;

                                if (line == 3)
                                {
                                    Suspect.Tasks.PlayAnimation(new AnimationDictionary("mp_common"), "givetake1_b", 5f, AnimationFlags.None).WaitForCompletion();

                                    if (CalloutPosition == CalloutPositions[0]) // La Mesa Railyard
                                    {
                                        Game.DisplayNotification("char_rickie", "char_rickie", "Go Loco Railroad", $"~y~{SuspectPersona.FullName}", $"~b~Position~s~: Manager \n~g~Location~s~: La Mesa \n~c~Valid until {month}/{day}/{year}");
                                    }
                                    else if (CalloutPosition == CalloutPositions[1]) // LSC Scrapyard
                                    {
                                        Game.DisplayNotification("char_chef", "char_chef", "Los Santos Customs", $"~y~{SuspectPersona.FullName}", $"~b~Position~s~: Manager \n~g~Location~s~: Los Santos Int'l \n~c~Valid until {month}/{day}/{year}");
                                    }
                                    else if (CalloutPosition == CalloutPositions[2]) // Terminal
                                    {
                                        Game.DisplayNotification("char_boatsite2", "char_boatsite2", "Daisy-Lee", $"~y~{SuspectPersona.FullName}", $"~b~Position~s~: Captain \n~g~Ship~s~: Daisy-Lee \n~c~Valid until {month}/{day}/{year}");
                                    }
                                    else if (CalloutPosition == CalloutPositions[3]) // McKenzie Airstrip
                                    {
                                        SuspectPersona.Forename = "Trevor";
                                        SuspectPersona.Surname = "Philips";
                                        SuspectPersona.Wanted = true;
                                        Game.DisplayNotification("hush_trevor", "hush_trevor", "Trevor Philips Industries", $"~y~{SuspectPersona.FullName}", "~b~Position~s~: CEO \n~g~Location~s~: Grapeseed \n~c~The best drugs you can buy!");
                                    }
                                    else if (CalloutPosition == CalloutPositions[4]) // Joshua Road Loading Dock
                                    {
                                        Game.DisplayNotification("char_barry", "char_barry", "VTA Shipping Company", $"~y~{SuspectPersona.FullName}", $"~b~Position~s~: Manager \n~g~Location~s~: Blaine County \n~c~Valid until {month}/{day}/{year}");
                                    }
                                    else if (CalloutPosition == CalloutPositions[5]) // Paleto Barn
                                    {
                                        Game.DisplayNotification("char_oscar", "char_oscar", "Wildflower Fields", $"~y~{SuspectPersona.FullName}", $"~b~Position~s~: Owner \n~g~Location~s~: Paleto Bay \n~c~Valid until {month}/{day}/{year}");
                                    }

                                    Game.LogTrivial($"[Emergency Callouts]: Displayed {SuspectPersona.FullName}'s credentials");
                                }

                                if (line == 4)
                                {
                                    MainPlayer.Tasks.GoToOffsetFromEntity(Suspect, 1f, 0f, 2f);
                                    GameFiber.Sleep(500);

                                    Suspect.Tasks.PlayAnimation(new AnimationDictionary("mp_common"), "givetake1_b", 5f, AnimationFlags.None);
                                    MainPlayer.Tasks.PlayAnimation(new AnimationDictionary("mp_common"), "givetake1_b", 5f, AnimationFlags.None);
                                    GameFiber.Sleep(2000);
                                    SuspectBlip.SetColorGreen();
                                }

                                if (line == dialogue.Length)
                                {
                                    GameFiber.Sleep(3000);
                                    Handle.AdvancedEndingSequence();
                                    break;
                                }
                            }
                            else
                            {
                                if (DialogueStarted == false)
                                {
                                    if (Settings.AllowController && UIMenu.IsUsingController)
                                    {
                                        Game.DisplayHelp($"Press ~{Settings.ControllerInteractKey.GetInstructionalId()}~ to talk to the ~y~suspect");
                                    }
                                    else
                                    {
                                        Game.DisplayHelp($"Press ~{Settings.InteractKey.GetInstructionalId()}~ to talk to the ~y~suspect");
                                    }
                                }
                            }
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

        private void Scenario3() // Pursuit
        {
            #region Scenario 3
            try
            {
                // Retrieve Hiding Position
                RetrieveHidingPosition(Suspect);

                // Set Dialogue Active
                SuspectDialogue();

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) <= 7f && Suspect.Exists() && PlayerArrived)
                        {
                            // Clear Suspect Tasks
                            Suspect.Tasks.Clear();

                            // Start Pursuit
                            LHandle pursuit = Functions.CreatePursuit();
                            Functions.AddPedToPursuit(pursuit, Suspect);
                            Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                            Play.PursuitAudio();

                            // Delete blips
                            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                            if (SearchArea.Exists()) { SearchArea.Delete(); }
                            if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

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
                Handle.PreventPickupCrash(Suspect);
                if (Settings.AllowController) { NativeFunction.Natives.xFE99B66D079CF6BC(0, 27, true); }

                #region PlayerArrived
                if (MainPlayer.Position.DistanceTo(Entrance) < 15f && !PlayerArrived)
                {
                    // Set PlayerArrived
                    PlayerArrived = true;

                    // Delete Nearby Peds
                    Handle.DeleteNearbyPeds(Suspect, 30f);

                    // Display Arriving Subtitle
                    Game.DisplaySubtitle("Find the ~r~trespasser~s~ in the ~y~area~s~.", 10000);

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
                if (MainPlayer.Position.DistanceTo(Suspect.Position) <= 5f && !PedFound && PlayerArrived && Suspect.Exists())
                {
                    // Set PedFound
                    PedFound = true;

                    // Hide Subtitle
                    Display.HideSubtitle();

                    // Enable SuspectBlip
                    if (SuspectBlip.Exists()) { SuspectBlip.Alpha = 1f; }

                    // Delete SearchArea
                    if (SearchArea.Exists()) { SearchArea.Delete(); }

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has found {SuspectPersona.FullName} (Suspect)");
                }
                #endregion

                #region PedDetained
                if (Functions.IsPedStoppedByPlayer(Suspect) && !PedDetained && Suspect.Exists())
                {
                    // Set PedDetained
                    PedDetained = true;
                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has detained {SuspectPersona.FullName} (Suspect)");

                    // Delete Suspect Blips
                    if (SuspectBlip.Exists()) { SuspectBlip.Delete(); Game.LogTrivial("[Emergency Callouts]: Deleted SuspectBlip"); }
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

                #region PlayerClimbing
                if (MainPlayer.IsClimbing && !PedFound)
                {
                    Game.DisplayHelp("~p~Clue~s~: The ~r~suspect~s~ has not climbed anything");
                    GameFiber.Sleep(5000);
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

            Functions.SetPedCantBeArrestedByPlayer(Suspect, false);

            if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            if (SearchArea.Exists()) { SearchArea.Delete(); }
            if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }
            if (WeldingDevice.Exists()) { WeldingDevice.Delete(); }
            if (Clipboard.Exists()) { Clipboard.Delete(); }
            if (Phone.Exists()) { Phone.Delete(); }

            Display.HideSubtitle();
            Display.EndNotification();
            Log.OnCalloutEnded(CalloutMessage, CalloutScenario);
        }
    }
}