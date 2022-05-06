//using EmergencyCallouts.Essential;
//using LSPD_First_Response.Engine.Scripting.Entities;
//using LSPD_First_Response.Mod.API;
//using LSPD_First_Response.Mod.Callouts;
//using Rage;
//using System;
//using System.Reflection;
//using System.Windows.Forms;
//using static EmergencyCallouts.Essential.Color;
//using static EmergencyCallouts.Essential.Helper;
//using Entity = EmergencyCallouts.Essential.Helper.Entity;
//using RAGENativeUI;
//using Rage.Native;

//namespace EmergencyCallouts.Callouts
//{
//    [CalloutInfo("[EC] Hijacked Ambulance", CalloutProbability.Low)]
//    public class HijackedAmbulance : Callout
//    {
//        Ped Danny;
//        Ped Will;
//        Ped Cam;
//        Ped Zach;
//        Persona DannyPersona;
//        Persona WillPersona;
//        Persona CamPersona;
//        Persona ZachPersona;

//        Blip EntranceBlip;
//        Blip SearchArea;
//        Blip DannyBlip;
//        Blip WillBlip;
//        Blip CamBlip;
//        Blip ZachBlip;

//        public override bool OnBeforeCalloutDisplayed()
//        {
//            CalloutMessage = "Hijacked Ambulance, 2 hostages taken";
//            CalloutAdvisory = "Two bank robbers are using an ambulance to escape a bank heist";
//            CalloutPosition = World.GetNextPositionOnStreet(MainPlayer.Position.Around(200f, Settings.MaxCalloutDistance));

//            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, Settings.SearchAreaSize / 2.5f);
//            AddMinimumDistanceCheck(30f, CalloutPosition);

//            Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT CRIME_PUBLIC_INTOXICATION IN_OR_ON_POSITION UNITS_RESPOND_CODE_02", CalloutPosition); // Update scanner audio

//            return base.OnBeforeCalloutDisplayed();
//        }

//        public override void OnCalloutDisplayed()
//        {
//            if (Other.PluginChecker.IsCalloutInterfaceRunning)
//            {
//                Other.CalloutInterfaceFunctions.SendCalloutDetails(this, "CODE 3", "");
//            }
//            base.OnCalloutDisplayed();
//        }

//        public override void OnCalloutNotAccepted()
//        {
//            Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} ignored the callout");
//            if (!Other.PluginChecker.IsCalloutInterfaceRunning)
//            {
//                Functions.PlayScannerAudio("PED_RESPONDING_DISPATCH");
//            }

//            base.OnCalloutNotAccepted();
//        }

//        public override bool OnCalloutAccepted()
//        {
//            // Callout Accepted
//            Log.OnCalloutAccepted(CalloutMessage);

//            // Accept Messages
//            Display.AcceptSubtitle(CalloutMessage, CalloutArea);
//            Display.OutdatedReminder();

//            // EntranceBlip
//            EntranceBlip = new Blip(CalloutPosition);
//            if (EntranceBlip.Exists()) { EntranceBlip.IsRouteEnabled = true; }

//            // Danny Sharp
//            Danny = new Ped(Entity.GetRandomMaleModel(), CalloutPosition, 0f);
//            DannyPersona = new Persona("Danny", "Sharp", LSPD_First_Response.Gender.Male);
//            Danny.IsPersistent = true;
//            Danny.BlockPermanentEvents = true;
//            Functions.SetPersonaForPed(Danny, DannyPersona);
//            Log.Creation(Danny, PedCategory.Suspect);

//            DannyBlip = Danny.AttachBlip();
//            DannyBlip.SetColorRed();
//            DannyBlip.Scale = (float)Settings.PedBlipScale;
//            DannyBlip.Alpha = 0f;

//            // Will Sharp
//            Will = new Ped(Entity.GetRandomMaleModel(), CalloutPosition, 0f);
//            WillPersona = new Persona("Will", "Sharp", LSPD_First_Response.Gender.Male);
//            Will.IsPersistent = true;
//            Will.BlockPermanentEvents = true;
//            Functions.SetPersonaForPed(Will, WillPersona);
//            Log.Creation(Will, PedCategory.Suspect);

//            WillBlip = Will.AttachBlip();
//            WillBlip.SetColorRed();
//            WillBlip.Scale = (float)Settings.PedBlipScale;
//            WillBlip.Alpha = 0f;

//            // Cam González (EMT)
//            Cam = new Ped(Entity.GetRandomFemaleModel(), CalloutPosition, 0f);
//            CamPersona = new Persona("Cam", "González", LSPD_First_Response.Gender.Female);
//            Cam.IsPersistent = true;
//            Cam.BlockPermanentEvents = true;
//            Functions.SetPersonaForPed(Cam, CamPersona);
//            Log.Creation(Cam, PedCategory.Paramedic);

//            CamBlip = Cam.AttachBlip();
//            CamBlip.SetColorGreen();
//            CamBlip.Scale = (float)Settings.PedBlipScale;
//            CamBlip.Alpha = 0f;

//            // Zach
//            Zach = new Ped(Entity.GetRandomMaleModel(), CalloutPosition, 0f);
//            ZachPersona = new Persona("Zach", "West", LSPD_First_Response.Gender.Male);
//            Zach.IsPersistent = true;
//            Zach.BlockPermanentEvents = true;
//            Functions.SetPersonaForPed(Zach, ZachPersona);
//            Log.Creation(Zach, PedCategory.Officer);

//            ZachBlip = Zach.AttachBlip();
//            ZachBlip.SetColorBlue();
//            ZachBlip.Scale = (float)Settings.PedBlipScale;
//            ZachBlip.Alpha = 0f;

//            CalloutHandler();

//            return base.OnCalloutAccepted();
//        }

//        private void CalloutHandler()
//        {
//            #region CalloutHandler
//            try
//            {
//                CalloutActive = true;

//                // Scenario Deciding
//                switch (CalloutScenario)
//                {
//                    case 1:
//                        Scenario1();
//                        break;
//                    case 2:
//                        Scenario2();
//                        break;
//                    case 3:
//                        Scenario3();
//                        break;
//                }
//            }
//            catch (Exception e)
//            {
//                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
//            }
//            #endregion
//        }

//        private void Dialogue()
//        {
//            #region Dialogue
//            try
//            {
//                bool stopDialogue = false;

//                string timeOfDay;
//                if (World.TimeOfDay.TotalHours >= 6 && World.TimeOfDay.TotalHours < 12)
//                {
//                    timeOfDay = "so early?";
//                }
//                else if (World.TimeOfDay.TotalHours >= 12 && World.TimeOfDay.TotalHours <= 21)
//                {
//                    timeOfDay = "in the middle of the day?";
//                }
//                else
//                {
//                    timeOfDay = "right now, shouldn't you go home?";
//                }

//                string[] dialogue =
//                {
//                    "~b~You~s~: Hey you, come here for a second.",
//                    "~y~Suspect~s~: Leave...me...ALONE!",
//                    $"~b~You~s~: Calm down sir, Just have a talk with me...",
//                    "~y~Suspect~s~: FINE!",
//                    $"~b~You~s~: So what are you doing here being drunk {timeOfDay}",
//                    "~y~Suspect~s~: Who cares what I do here, I'm not harming anyone right?",
//                    "~b~You~s~: Well I didn't get any assault calls yet.",
//                    "~y~Suspect~s~: You assume I did something? So much for innocent until proven guilty...",
//                    "~b~You~s~: That wasn't what I meant, sorry.",
//                    "~y~Suspect~s~: Well you got off lucky this time haha.",
//                    "~b~You~s~: Uhuh...",
//                    "~r~Arrest~s~ or ~g~dismiss~s~ the person.",
//                };

//                int line = 0;

//                GameFiber.StartNew(delegate
//                {
//                    while (CalloutActive)
//                    {
//                        GameFiber.Yield();

//                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 5f && Suspect.IsAlive && MainPlayer.IsOnFoot)
//                        {
//                            if (Game.IsKeyDown(Settings.InteractKey) || (Game.IsControllerButtonDown(Settings.ControllerInteractKey) && Settings.AllowController && UIMenu.IsUsingController))
//                            {
//                                if (!DialogueStarted)
//                                {
//                                    Suspect.Tasks.Clear();

//                                    Game.LogTrivial("[Emergency Callouts]: Dialogue started with " + SuspectPersona.FullName);
//                                }

//                                DialogueStarted = true;

//                                Suspect.Tasks.AchieveHeading(MainPlayer.Heading - 180f);

//                                Game.DisplaySubtitle(dialogue[line], 15000);
//                                if (!stopDialogue) { line++; }

//                                Game.LogTrivial("[Emergency Callouts]: Displayed dialogue line " + line);

//                                if (line == dialogue.Length)
//                                {
//                                    stopDialogue = true;
//                                    Game.LogTrivial("[Emergency Callouts]: Dialogue ended");

//                                    GameFiber.Sleep(1500);

//                                    if (HasBottle)
//                                    {
//                                        if (Settings.AllowController && UIMenu.IsUsingController)
//                                        {
//                                            Game.DisplayHelp($"Press ~{ControllerButtons.DPadLeft.GetInstructionalId()}~ to ~g~dismiss~s~ the ~y~suspect~s~ and ~o~confiscate~s~ the bottle");
//                                        }
//                                        else
//                                        {
//                                            Game.DisplayHelp($"Press ~{Keys.N.GetInstructionalId()}~ to ~g~dismiss~s~ the ~y~suspect~s~ and ~o~confiscate~s~ the bottle");
//                                        }
//                                    }
//                                    else
//                                    {
//                                        if (Settings.AllowController && UIMenu.IsUsingController)
//                                        {
//                                            Game.DisplayHelp($"Press ~{ControllerButtons.DPadLeft.GetInstructionalId()}~ to ~g~dismiss~s~ the ~y~suspect");
//                                        }
//                                        else
//                                        {
//                                            Game.DisplayHelp($"Press ~{Keys.N.GetInstructionalId()}~ to ~g~dismiss~s~ the ~y~suspect");
//                                        }
//                                    }

//                                    while (CalloutActive)
//                                    {
//                                        GameFiber.Yield();
//                                        if (Game.IsKeyDown(Keys.N) || (Game.IsControllerButtonDown(ControllerButtons.DPadLeft) && Settings.AllowController && UIMenu.IsUsingController))
//                                        {
//                                            if (HasBottle)
//                                            {
//                                                Game.DisplaySubtitle("~b~You~s~: I'm letting you go, I will need that bottle from you though.");
//                                                GameFiber.Sleep(3000);
//                                                MainPlayer.Tasks.GoToOffsetFromEntity(Suspect, 1f, 0f, 2f);
//                                                GameFiber.Sleep(500);
//                                                Suspect.Tasks.PlayAnimation(new AnimationDictionary("mp_common"), "givetake1_b", 5f, AnimationFlags.SecondaryTask);
//                                                MainPlayer.Tasks.PlayAnimation(new AnimationDictionary("mp_common"), "givetake1_b", 5f, AnimationFlags.SecondaryTask);

//                                                GameFiber.Sleep(1000);
//                                                Suspect.Inventory.Weapons.Clear();
//                                                GameFiber.Sleep(4000);
//                                            }
//                                            Handle.AdvancedEndingSequence();
//                                            break;
//                                        }
//                                        else if (Suspect.IsCuffed)
//                                        {
//                                            GameFiber.Sleep(3000);
//                                            Handle.AdvancedEndingSequence();
//                                            break;
//                                        }
//                                    }
//                                }
//                            }
//                            else
//                            {
//                                if (!DialogueStarted)
//                                {
//                                    if (Settings.AllowController && UIMenu.IsUsingController)
//                                    {
//                                        Game.DisplayHelp($"Press ~{Settings.ControllerInteractKey.GetInstructionalId()}~ to talk to the ~y~suspect");
//                                    }
//                                    else
//                                    {
//                                        Game.DisplayHelp($"Press ~{Settings.InteractKey.GetInstructionalId()}~ to talk to the ~y~suspect");
//                                    }
//                                }
//                            }
//                        }
//                    }
//                });
//            }
//            catch (Exception e)
//            {
//                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
//            }
//            #endregion
//        }

//        private void Scenario1() // Standard
//        {
//            #region Scenario 1
//            try
//            {
//                Dialogue();
//            }
//            catch (Exception e)
//            {
//                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
//            }
//            #endregion
//        }

//        private void Scenario2() // Bottle
//        {
//            #region Scenario 2
//            try
//            {
//                Suspect.Inventory.GiveNewWeapon("WEAPON_BOTTLE", -1, true);
//                HasBottle = true;
//                Dialogue();
//            }
//            catch (Exception e)
//            {
//                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
//            }
//            #endregion
//        }

//        private void Scenario3() // Pass out
//        {
//            #region Scenario 3
//            try
//            {
//                GameFiber.StartNew(delegate
//                {
//                    while (CalloutActive)
//                    {
//                        GameFiber.Yield();

//                        if (MainPlayer.Position.DistanceTo(Suspect.Position) <= 7f && Suspect.IsAlive && MainPlayer.IsOnFoot && PlayerArrived)
//                        {
//                            Game.DisplaySubtitle("~y~Suspect~s~: I'm drunk, sooo wha...", 10000);
//                            GameFiber.Sleep(1250);
//                            if (Suspect.Exists()) { Suspect.Kill(); }
//                            GameFiber.Sleep(5000);
//                            Game.DisplaySubtitle("Request an ~g~ambulance~s~.", 7500);
//                            break;
//                        }
//                    }
//                });
//            }
//            catch (Exception e)
//            {
//                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
//            }
//            #endregion
//        }

//        public override void Process()
//        {
//            base.Process();

//            try
//            {
//                Handle.ManualEnding();
//                Handle.PreventPickupCrash(Suspect);
//                if (Settings.AllowController) { NativeFunction.Natives.xFE99B66D079CF6BC(0, 27, true); }

//                #region PlayerArrived
//                if (MainPlayer.Position.DistanceTo(CalloutPosition) < Settings.SearchAreaSize && !PlayerArrived)
//                {
//                    // Remove EntranceBlip
//                    if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

//                    // Create SearchArea
//                    SearchArea = new Blip(Suspect.Position.Around2D(30f), Settings.SearchAreaSize);
//                    SearchArea.SetColorYellow();
//                    SearchArea.Alpha = 0.5f;

//                    // Display Subtitle
//                    Game.DisplaySubtitle("Find the ~y~drunk person~s~ in the ~y~area~s~.", 10000);

//                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has arrived on scene");

//                    PlayerArrived = true;
//                }
//                #endregion

//                #region PedFound
//                if (MainPlayer.Position.DistanceTo(Suspect.Position) < 5f && !PedFound && PlayerArrived && Suspect)
//                {
//                    // Hide Subtitle
//                    Display.HideSubtitle();

//                    // Enable SuspectBlip
//                    if (SuspectBlip.Exists()) { SuspectBlip.Alpha = 1f; }

//                    // Remove SearchArea
//                    if (SearchArea.Exists()) { SearchArea.Delete(); }

//                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has found {SuspectPersona.FullName} (Suspect)");

//                    PedFound = true;
//                }
//                #endregion

//                #region PedDetained
//                if (Functions.IsPedStoppedByPlayer(Suspect) && !PedDetained && Suspect.Exists())
//                {
//                    // Remove SuspectBlip
//                    if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }

//                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has detained {SuspectPersona.FullName} (Suspect)");

//                    PedDetained = true;
//                }
//                #endregion

//                #region PlayerLeft
//                if (MainPlayer.Position.DistanceTo(CalloutPosition) > Settings.SearchAreaSize * 3f && PlayerArrived && !PedFound)
//                {
//                    // Set OnScene
//                    PlayerArrived = false;

//                    // Disable SuspectBlip
//                    if (SuspectBlip.Exists()) { SuspectBlip.Alpha = 0f; }

//                    // Delete SearchArea
//                    if (SearchArea.Exists()) { SearchArea.Delete(); }

//                    // Create EntranceBlip
//                    EntranceBlip = new Blip(CalloutPosition);

//                    // Enable Route
//                    if (EntranceBlip.Exists()) { EntranceBlip.IsRouteEnabled = true; }

//                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has left the scene");
//                }
//                #endregion

//                #region RefreshSearchArea
//                if (!PedFound)
//                {
//                    if (Suspect.Position.DistanceTo(CalloutPosition) < Settings.SearchAreaSize)
//                    {
//                        NeedsRefreshing = false;
//                    }
//                    else
//                    {
//                        NeedsRefreshing = true;
//                    }
//                }

//                if (Suspect.Position.DistanceTo(CalloutPosition) > Settings.SearchAreaSize && NeedsRefreshing)
//                {
//                    CalloutPosition = Suspect.Position;
//                    if (SearchArea.Exists()) { SearchArea.Delete(); }

//                    SearchArea = new Blip(Suspect.Position.Around2D(30f), Settings.SearchAreaSize);
//                    SearchArea.SetColorYellow();
//                    SearchArea.Alpha = 0.5f;
//                    Game.LogTrivial("[Emergency Callouts]: Refreshed SearchArea");

//                    Functions.PlayScannerAudioUsingPosition("SUSPECT_LAST_SEEN IN_OR_ON_POSITION", Suspect.Position);
//                }
//                #endregion
//            }
//            catch (Exception e)
//            {
//                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
//                End();
//            }
//        }

//        public override void End()
//        {
//            base.End();

//            CalloutActive = false;

//            if (Suspect.Exists()) { Suspect.Dismiss(); }
//            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
//            if (SearchArea.Exists()) { SearchArea.Delete(); }
//            if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

//            Display.HideSubtitle();
//            Display.EndNotification();
//            Log.OnCalloutEnded(CalloutMessage, CalloutScenario);
//        }
//    }
//}