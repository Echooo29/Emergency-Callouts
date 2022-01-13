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

namespace EmergencyCallouts.Callouts
{
    [CalloutInfo("Public Intoxication", CalloutProbability.Medium)]
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
            while (!World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(200f, Settings.MaxCalloutDistance)).GetSafePositionForPed(out CalloutPosition))
            {
                GameFiber.Yield();

                count++;
                if (count >= 15) { return false; }
            }

            CalloutMessage = Localization.PublicIntoxication;
            CalloutDetails = Localization.PublicIntoxicationDetails;
            CalloutArea = World.GetStreetName(CalloutPosition);
            CalloutScenario = GetRandomScenarioNumber(5);

            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, Settings.SearchAreaSize / 2.5f);
            AddMinimumDistanceCheck(30f, CalloutPosition);

            Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT CRIME_PUBLIC_INTOXICATION IN_OR_ON_POSITION UNITS_RESPOND_CODE_02", CalloutPosition);

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
            // Callout Accepted
            Log.OnCalloutAccepted(CalloutMessage, CalloutScenario);
            
            // Accept Messages
            Display.AcceptNotification(CalloutDetails);
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
                    case 4:
                        Scenario4();
                        break;
                    case 5:
                        Scenario5();
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
                bool stopDialogue = false;

                string timeOfDay;
                if (World.TimeOfDay.Hours >= 6 && World.TimeOfDay.Hours < 12)
                {
                    timeOfDay = " so early?";
                }
                else if (World.TimeOfDay.Hours >= 12 && World.TimeOfDay.Hours <= 21)
                {
                    timeOfDay = " in the middle of the day?";
                }
                else
                {
                    timeOfDay = ", shouldn't you go home?";
                }

                string[] dialogue =
                {
                    "~b~You~s~: Hey you, come here for a second.",
                    "~y~Suspect~s~: Leave...me...ALONE!",
                    $"~b~You~s~: Calm down sir, Just have a talk with me...",
                    "~y~Suspect~s~: FINE!",
                    $"~b~You~s~: So what are you doing here being drunk{timeOfDay}",
                    "~y~Suspect~s~: Who cares what I do here, I'm not harming anyone right?",
                    "~b~You~s~: Well I didn't get any assault calls yet.",
                    "~y~Suspect~s~: You assume I did something? So much for innocent until proven guilty...",
                    "~b~You~s~: That wasn't what I meant, sorry.",
                    "~y~Suspect~s~: Well you got of lucky this time haha.",
                    "~r~Arrest~s~ or ~g~dismiss~s~ the person.",
                };

                int line = 0;

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 5f && Suspect.IsAlive && MainPlayer.IsOnFoot)
                        {
                            if (Game.IsKeyDown(Settings.InteractKey))
                            {
                                if (!DialogueStarted)
                                {
                                    Suspect.Tasks.Clear();

                                    Game.LogTrivial("[Emergency Callouts]: Dialogue started with " + SuspectPersona.FullName);
                                }

                                DialogueStarted = true;

                                Suspect.Tasks.AchieveHeading(MainPlayer.Heading - 180f);

                                Game.DisplaySubtitle(dialogue[line], 15000);
                                if (!stopDialogue) { line++; }

                                Game.LogTrivial("[Emergency Callouts]: Displayed dialogue line " + line);

                                if (line == dialogue.Length)
                                {
                                    stopDialogue = true;
                                    Game.LogTrivial("[Emergency Callouts]: Dialogue Ended");

                                    GameFiber.Sleep(1500);

                                    if (HasBottle)
                                    {
                                        Game.DisplayHelp(Localization.InteractionDismissAndConfiscate);
                                    }
                                    else
                                    {
                                        Game.DisplayHelp(Localization.InteractionDismiss);
                                    }

                                    while (CalloutActive)
                                    {
                                        GameFiber.Yield();
                                        if (Game.IsKeyDown(Keys.N))
                                        {
                                            if (HasBottle)
                                            {
                                                MainPlayer.Tasks.GoToOffsetFromEntity(Suspect, 1f, 0f, 2f);
                                                GameFiber.Sleep(500);
                                                Suspect.Tasks.PlayAnimation(new AnimationDictionary("mp_common"), "givetake1_b", 5f, AnimationFlags.SecondaryTask);
                                                MainPlayer.Tasks.PlayAnimation(new AnimationDictionary("mp_common"), "givetake1_b", 5f, AnimationFlags.SecondaryTask);

                                                GameFiber.Sleep(1000);
                                                Suspect.Inventory.Weapons.Clear();
                                                GameFiber.Sleep(4000);
                                            }
                                            Handle.AdvancedEndingSequence();
                                            break;
                                        }
                                    }
                                }

                                GameFiber.Sleep(500);
                            }
                            else
                            {
                                if (!DialogueStarted)
                                {
                                    Game.DisplayHelp($"{Localization.InteractionDialogueIntro} ~y~{Settings.InteractKey}~s~ {Localization.InteractionDialoguePromptSuspect2}");
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

        private void Scenario1()
        {
            #region Default
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

        private void Scenario2()
        {
            #region Hostile
            try
            {
                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 10f && MainPlayer.IsOnFoot && PlayerArrived)
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

        private void Scenario3()
        {
            #region Bottle
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

        private void Scenario4()
        {
            #region Bottle & Hostile
            try
            {
                Suspect.Inventory.GiveNewWeapon("WEAPON_BOTTLE", -1, true);

                while (CalloutActive)
                {
                    GameFiber.Yield();

                    if (MainPlayer.Position.DistanceTo(Suspect.Position) < 10f && MainPlayer.IsOnFoot && PlayerArrived)
                    {
                        Suspect.Tasks.FightAgainst(MainPlayer);

                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            }
            #endregion
        }

        private void Scenario5()
        {
            #region Pass out
            try
            {
                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 5f && MainPlayer.IsOnFoot && PlayerArrived)
                        {
                            Game.DisplaySubtitle(Localization.PassOutLine, 2500);
                            GameFiber.Sleep(2500);
                            if (Suspect.Exists()) { Suspect.Kill(); }

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

                #region PlayerArrived
                if (MainPlayer.Position.DistanceTo(CalloutPosition) < Settings.SearchAreaSize && !PlayerArrived)
                {
                    // Remove EntranceBlip
                    if (EntranceBlip.Exists()) {EntranceBlip.Delete(); }

                    // Create SearchArea
                    SearchArea = new Blip(Suspect.Position.Around(5f, 30f), Settings.SearchAreaSize);
                    SearchArea.SetColorYellow();
                    SearchArea.Alpha = 0.5f;

                    // Display Subtitle
                    Game.DisplaySubtitle(Localization.PublicIntoxicationSubtitle, 10000);

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
                    if (SuspectBlip.Exists()) {SuspectBlip.Delete(); }

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has detained {SuspectPersona.FullName} (Suspect)");

                    PedDetained = true;
                }
                #endregion

                #region PlayerLeft
                if (MainPlayer.Position.DistanceTo(CalloutPosition) > Settings.SearchAreaSize * 3f && PlayerArrived)
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

                    SearchArea = new Blip(Suspect.Position.Around(10f, 30f), Settings.SearchAreaSize);
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