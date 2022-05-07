using EmergencyCallouts.Essential;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Reflection;
using System.Windows.Forms;
using static EmergencyCallouts.Essential.Color;
using static EmergencyCallouts.Essential.Helper;
using Entity = EmergencyCallouts.Essential.Helper.Entity;
using RAGENativeUI;
using Rage.Native;

namespace EmergencyCallouts.Callouts
{
    [CalloutInfo("[EC] Public Intoxication", CalloutProbability.Medium)]
    public class PublicIntoxication : Callout
    {
        bool PlayerArrived;
        bool PedFound;
        bool PedDetained;
        bool NeedsRefreshing;
        bool CalloutActive;
        bool DialogueStarted;
        bool HasBottle;

        new Vector3 CalloutPosition;

        Ped Suspect;
        Persona SuspectPersona;

        Blip EntranceBlip;
        Blip SearchArea;
        Blip SuspectBlip;

        public override bool OnBeforeCalloutDisplayed()
        {
            int count = 0;

            CalloutMessage = "Public Intoxication";
            CalloutAdvisory = "Reports of a person under the influence of alcohol.";
            CalloutScenario = random.Next(1, 4);

            while (!World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around2D(200f, Settings.MaxCalloutDistance)).GetSafePositionForPed(out CalloutPosition))
            {
                GameFiber.Yield();

                count++;
                if (count >= 10) { CalloutPosition = World.GetNextPositionOnStreet(MainPlayer.Position.Around2D(200f, Settings.MaxCalloutDistance)); }
                CalloutArea = World.GetStreetName(CalloutPosition);
            }

            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, Settings.SearchAreaSize / 2.5f);
            AddMinimumDistanceCheck(30f, CalloutPosition);

            Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT CRIME_PUBLIC_INTOXICATION IN_OR_ON_POSITION UNITS_RESPOND_CODE_02", CalloutPosition);

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
            // Callout Accepted
            Log.OnCalloutAccepted(CalloutMessage, CalloutScenario);

            // Accept Messages
            Display.AcceptSubtitle(CalloutMessage, CalloutArea);
            Display.OutdatedReminder();

            // EntranceBlip
            EntranceBlip = new Blip(CalloutPosition);
            if (EntranceBlip.Exists()) { EntranceBlip.IsRouteEnabled = true; }

            // Suspect
            Suspect = new Ped(Entity.GetRandomMaleModel(), CalloutPosition, 0f);
            SuspectPersona = Functions.GetPersonaForPed(Suspect);
            Suspect.IsPersistent = true;
            Suspect.BlockPermanentEvents = true;
            Suspect.SetIntoxicated();
            Log.Creation(Suspect, PedCategory.Suspect);

            SuspectBlip = Suspect.AttachBlip();
            SuspectBlip.SetColorYellow();
            SuspectBlip.Scale = (float)Settings.PedBlipScale;
            SuspectBlip.Alpha = 0f;

            Suspect.Tasks.Wander();

            CalloutHandler();

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
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            }
            #endregion
        }

        private void Dialogue()
        {
            #region Dialogue
            try
            {
                string timeOfDay;
                bool stopDialogue = false;
                int line = 0;

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
                    timeOfDay = "right now, shouldn't you go home?";
                }

                string[] line1 = { "Hey you, come here for a second.", "Hey, why don't you come over here?", "Hello, let's have a talk okay?", "Hi sir how are you doing today?" };
                string[] line2 = { "Leave me alone.", "Let me be!", "Gimme some privacy you piggy.", "I clearly don't want to!", "You people always harassing me out here!", "Nuh-uh, stranger danger!", "Hell no, stranger equals danger remember!?", "No thanks and... do you guys live in piggy banks by any chance?", "Unless you have some beer we have nothing to talk about!" };
                string[] line3 = { "Calm down sir, just have talk with me okay?", "Calm down sir, we don't want things to escalate.", "Sir, if you keep this up it will be annoying for both of us.", "Let's just get this over with okay?" };
                string[] line4 = { "FINE!", "Alrighty!", "Alright then.", "OK!", "Okay.", "Sure.", "Okay buddy, fine.", "Yes sir!", "Yes mom!" };
                string[] line5 = { "So what are you doing here being drunk ", "Why are you drunk ", "Now is not the time to be roaming the streets " };
                string[] line6 = { "Who cares what or when I do things?", "Who cares? I aint hurting people!", "Who gives a damn!", "Who cares?" };
                string[] line7 = { "I do.", "I care.", "I care about what you do here sir.", "I just don't want anyone to get hurt sir." };
                string[] line8 = { "Okay, what now?", "Sure, so what now?", "Alright, what now?", "Ok so... what do we do now?", "Okay, and what exactly are we going to do now?" };

                int line1Random = random.Next(0, line1.Length);
                int line2Random = random.Next(0, line2.Length);
                int line3Random = random.Next(0, line3.Length);
                int line4Random = random.Next(0, line4.Length);
                int line5Random = random.Next(0, line5.Length);
                int line6Random = random.Next(0, line6.Length);
                int line7Random = random.Next(0, line7.Length);
                int line8Random = random.Next(0, line8.Length);

                string[] dialogue =
                {
                    "~b~You~s~: " + line1[line1Random],
                    "~y~Suspect~s~: " + line2[line2Random],
                    $"~b~You~s~: " + line3[line3Random],
                    "~y~Suspect~s~: " + line4[line4Random],
                    $"~b~You~s~: " + line5[line5Random] + timeOfDay,
                    "~y~Suspect~s~: " + line6[line6Random],
                    "~b~You~s~: " + line7[line7Random],
                    "~y~Suspect~s~: " + line8[line8Random],
                    "~r~Arrest~s~ or ~g~dismiss~s~ the person.",
                };


                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 5f && Suspect.IsAlive && MainPlayer.IsOnFoot)
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

                                Game.DisplaySubtitle(dialogue[line], 15000);
                                if (!stopDialogue) { line++; }

                                Game.LogTrivial("[Emergency Callouts]: Displayed dialogue line " + line);

                                if (line == dialogue.Length)
                                {
                                    stopDialogue = true;
                                    Game.LogTrivial("[Emergency Callouts]: Dialogue ended");

                                    GameFiber.Sleep(1500);

                                    if (HasBottle)
                                    {
                                        if (Settings.AllowController && UIMenu.IsUsingController)
                                        {
                                            Game.DisplayHelp($"Press ~{ControllerButtons.DPadLeft.GetInstructionalId()}~ to ~g~dismiss~s~ the ~y~suspect~s~ and ~o~confiscate~s~ the bottle");
                                        }
                                        else
                                        {
                                            Game.DisplayHelp($"Press ~{Keys.N.GetInstructionalId()}~ to ~g~dismiss~s~ the ~y~suspect~s~ and ~o~confiscate~s~ the bottle");
                                        }
                                    }
                                    else
                                    {
                                        if (Settings.AllowController && UIMenu.IsUsingController)
                                        {
                                            Game.DisplayHelp($"Press ~{ControllerButtons.DPadLeft.GetInstructionalId()}~ to ~g~dismiss~s~ the ~y~suspect");
                                        }
                                        else
                                        {
                                            Game.DisplayHelp($"Press ~{Keys.N.GetInstructionalId()}~ to ~g~dismiss~s~ the ~y~suspect");
                                        }
                                    }

                                    while (CalloutActive)
                                    {
                                        GameFiber.Yield();

                                        if (Game.IsKeyDown(Keys.N) || (Game.IsControllerButtonDown(ControllerButtons.DPadLeft) && Settings.AllowController && UIMenu.IsUsingController))
                                        {
                                            if (HasBottle)
                                            {
                                                Game.DisplaySubtitle("~b~You~s~: I'm letting you go, I will need that bottle from you though. You will also head straight to home.");
                                                GameFiber.Sleep(3000);
                                                MainPlayer.Tasks.GoToOffsetFromEntity(Suspect, 1f, 0f, 2f);
                                                GameFiber.Sleep(500);
                                                Suspect.Tasks.PlayAnimation(new AnimationDictionary("mp_common"), "givetake1_b", 5f, AnimationFlags.SecondaryTask);
                                                MainPlayer.Tasks.PlayAnimation(new AnimationDictionary("mp_common"), "givetake1_b", 5f, AnimationFlags.SecondaryTask);

                                                GameFiber.Sleep(1000);
                                                Suspect.Inventory.Weapons.Clear();
                                                GameFiber.Sleep(4000);
                                            }
                                            else
                                            {
                                                Game.DisplaySubtitle("~b~You~s~: So this is what you're going to do, you're gonna head straight to home.");
                                            }

                                            Handle.AdvancedEndingSequence();

                                            break;
                                        }
                                        else if (Suspect.IsCuffed)
                                        {
                                            GameFiber.Sleep(3000);
                                            Handle.AdvancedEndingSequence();
                                            break;
                                        }
                                    }
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
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            }
            #endregion
        }

        private void Scenario1() // Standard
        {
            #region Scenario 1
            try
            {
                Dialogue();
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            }
            #endregion
        }

        private void Scenario2() // Bottle
        {
            #region Scenario 2
            try
            {
                Suspect.Inventory.GiveNewWeapon("WEAPON_BOTTLE", -1, true);
                HasBottle = true;
                Dialogue();
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            }
            #endregion
        }

        private void Scenario3() // Pass out
        {
            #region Scenario 3
            try
            {
                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) <= 7f && Suspect.IsAlive && MainPlayer.IsOnFoot && PlayerArrived)
                        {
                            Game.DisplaySubtitle("~y~Suspect~s~: I'm drunk, sooo wha...", 10000);
                            GameFiber.Sleep(1250);
                            if (Suspect.Exists()) { Suspect.Kill(); }
                            GameFiber.Sleep(5000);
                            Game.DisplaySubtitle("Request an ~g~ambulance~s~.", 7500);
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
                if (MainPlayer.Position.DistanceTo(CalloutPosition) < Settings.SearchAreaSize && !PlayerArrived)
                {
                    // Remove EntranceBlip
                    if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

                    // Create SearchArea
                    SearchArea = new Blip(Suspect.Position.Around2D(30f), Settings.SearchAreaSize);
                    SearchArea.SetColorYellow();
                    SearchArea.Alpha = 0.5f;

                    // Display Subtitle
                    Game.DisplaySubtitle("Find the ~y~drunk person~s~ in the ~y~area~s~.", 10000);

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has arrived on scene");

                    PlayerArrived = true;
                }
                #endregion

                #region PedFound
                if (MainPlayer.Position.DistanceTo(Suspect.Position) < 5f && !PedFound && PlayerArrived && Suspect)
                {
                    // Hide Subtitle
                    Display.HideSubtitle();

                    // Enable SuspectBlip
                    if (SuspectBlip.Exists()) { SuspectBlip.Alpha = 1f; }

                    // Remove SearchArea
                    if (SearchArea.Exists()) { SearchArea.Delete(); }

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has found {SuspectPersona.FullName} (Suspect)");

                    PedFound = true;
                }
                #endregion

                #region PedDetained
                if (Functions.IsPedStoppedByPlayer(Suspect) && !PedDetained && Suspect.Exists())
                {
                    // Remove SuspectBlip
                    if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has detained {SuspectPersona.FullName} (Suspect)");

                    PedDetained = true;
                }
                #endregion

                #region PlayerLeft
                if (MainPlayer.Position.DistanceTo(CalloutPosition) > Settings.SearchAreaSize * 3f && PlayerArrived && !PedFound)
                {
                    // Set OnScene
                    PlayerArrived = false;

                    // Disable SuspectBlip
                    if (SuspectBlip.Exists()) { SuspectBlip.Alpha = 0f; }

                    // Delete SearchArea
                    if (SearchArea.Exists()) { SearchArea.Delete(); }

                    // Create EntranceBlip
                    EntranceBlip = new Blip(CalloutPosition);

                    // Enable Route
                    if (EntranceBlip.Exists()) { EntranceBlip.IsRouteEnabled = true; }

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has left the scene");
                }
                #endregion

                #region RefreshSearchArea
                if (!PedFound)
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

                if (Suspect.Position.DistanceTo(CalloutPosition) > Settings.SearchAreaSize && NeedsRefreshing)
                {
                    CalloutPosition = Suspect.Position;
                    if (SearchArea.Exists()) { SearchArea.Delete(); }

                    SearchArea = new Blip(Suspect.Position.Around2D(30f), Settings.SearchAreaSize);
                    SearchArea.SetColorYellow();
                    SearchArea.Alpha = 0.5f;
                    Game.LogTrivial("[Emergency Callouts]: Refreshed SearchArea");

                    Functions.PlayScannerAudioUsingPosition("SUSPECT_LAST_SEEN IN_OR_ON_POSITION", Suspect.Position);
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