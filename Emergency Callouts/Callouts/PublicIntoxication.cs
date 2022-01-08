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

            CalloutMessage = "Public Intoxication";
            CalloutDetails = "There are multiple reports of a person under the influence of alcohol.";
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
            
            EntranceBlip = new Blip(CalloutPosition);
            EntranceBlip.EnableRoute();

            Suspect = new Ped(Entity.GetRandomMaleModel(), CalloutPosition, 0f);
            SuspectPersona = Functions.GetPersonaForPed(Suspect);
            Suspect.SetDefaults();
            Suspect.SetIntoxicated();
            
            SuspectBlip = Suspect.AttachBlip();
            SuspectBlip.SetColorYellow();
            SuspectBlip.Scale = (float)Settings.PedBlipScale;
            SuspectBlip.Disable();

            Suspect.Tasks.Wander();

            CalloutHandler();

            return base.OnCalloutAccepted();
        }

        private void CalloutHandler()
        {
            try
            {
                CalloutActive = true;
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

                // Log Creation
                Log.Creation(Suspect, PedCategory.Suspect);
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Dialogue()
        {
            #region Dialogue

            bool stopDialogue = false;

            string timeOfDay;
            if (World.TimeOfDay.Hours <= 6 && World.TimeOfDay.Hours >= 12)
            {
                timeOfDay = " so early?";
            }
            else if (World.TimeOfDay.Hours >= 12 && World.TimeOfDay.Hours <= 18)
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
                        if (Game.IsKeyDown(Settings.TalkKey))
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
                                    Game.DisplayHelp("Press ~y~N~s~ to ~g~dismiss~s~ the ~y~suspect~s~ and ~o~confiscate~s~ the bottle");
                                }
                                else
                                {
                                    Game.DisplayHelp("Press ~y~N~s~ to ~g~dismiss~s~ the ~y~suspect");
                                }

                                while (CalloutActive)
                                {
                                    GameFiber.Yield();
                                    if (Game.IsKeyDown(Keys.N))
                                    {
                                        if (HasBottle)
                                        {
                                            Suspect.Tasks.PlayAnimation(new AnimationDictionary("mp_common"), "givetake1_b", 5f, AnimationFlags.SecondaryTask);
                                            MainPlayer.Tasks.PlayAnimation(new AnimationDictionary("mp_common"), "givetake1_b", 5f, AnimationFlags.SecondaryTask);

                                            GameFiber.Sleep(1000);
                                            Suspect.Inventory.Weapons.Clear();
                                            GameFiber.Sleep(4000);
                                        }
                                        MainPlayer.Tasks.PlayAnimation(new AnimationDictionary("random@arrests"), "generic_radio_enter", 5f, AnimationFlags.SecondaryTask | AnimationFlags.UpperBodyOnly);
                                        GameFiber.Sleep(1000);
                                        Game.DisplayNotification("~b~You~s~: Dispatch, call is code 4.");
                                        GameFiber.Sleep(2700);
                                        Play.CodeFourAudio();
                                        GameFiber.Sleep(5000);
                                        Functions.StopCurrentCallout();
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
                                Game.DisplayHelp("Press ~y~Y~s~ to talk to the ~y~suspect~s~.");
                            }
                        }
                    }
                }
            });
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
                            Game.DisplaySubtitle("~y~Suspect~s~: I'm drunk! Big deal ri...!", 2500);
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
                Handle.AutomaticEnding(Suspect);
                Handle.PreventPickupCrash(Suspect);
                Handle.PreventDistanceCrash(CalloutPosition, PlayerArrived, PedFound);

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
                    Game.DisplaySubtitle("Find the ~r~drunk person~s~ in the ~y~area~s~.", 20000);

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
                    SuspectBlip.Enable();

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
                    SuspectBlip.Disable();

                    // Delete SearchArea
                    if (SearchArea.Exists()) { SearchArea.Delete(); }

                    // Create EntranceBlip
                    EntranceBlip = new Blip(CalloutPosition);

                    // Enable Route
                    EntranceBlip.EnableRoute();

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