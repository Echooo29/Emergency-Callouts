using EmergencyCallouts.Essential;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using RAGENativeUI;
using System;
using static EmergencyCallouts.Essential.Color;
using static EmergencyCallouts.Essential.Helper;
using static EmergencyCallouts.Essential.Inventory;
using Entity = EmergencyCallouts.Essential.Helper.Entity;

namespace EmergencyCallouts.Callouts
{
    [CalloutInfo("Domestic Violence", CalloutProbability.Medium)]
    public class DomesticViolence : Callout
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

        // CalloutPositions (Entrance)
        readonly Vector3[] CalloutPositions =
        {
            new Vector3(11.3652f, 545.7453f, 175.8412f), // Vinewood Hills
            new Vector3(222.883f, -1726.32f, 28.87364f), // Davis
            new Vector3(224.5887f, 3162.886f, 42.3335f), // Sandy Shores
        };
        #endregion

        // Vinewood Hills
        #region Positions
        readonly Vector3[] VinewoodHillsFightPositions =
        {
            new Vector3(23.723f, 523.2088f, 170.2274f),   // Chill Area 1
            new Vector3(-6.617259f, 509.189f, 170.6275f), // Chill Area 2
        };

        readonly float[] VinewoodHillsFightHeadings =
        {
            0f,
            0f,
        };
        #endregion

        // Davis
        #region Positions
        readonly Vector3[] DavisFightPositions =
        {
            new Vector3(203.894f, -1706.848f, 29.30457f), // Backyard
            new Vector3(210.6721f, -1720.645f, 29.2917f), // Front
        };

        readonly float[] DavisFightHeadings =
        {
            310f,
            34f,
        };
        #endregion

        // Sandy Shores
        #region Positions
        readonly Vector3[] SandyShoresFightPositions =
        {
            new Vector3(245.5203f, 3169.379f, 42.8357f),  // Frontyard
            new Vector3(264.1511f, 3176.024f, 42.52968f), // Backyard
            new Vector3(250.4642f, 3192.325f, 43.07049f), // Side
        };

        readonly float[] SandyShoresFightHeadings =
        {
            186f,
            0f,
            0f,
        };
        #endregion

        Ped Suspect;
        Ped Victim;

        Blip SuspectBlip;
        Blip VictimBlip;
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
                }
            }

            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, Settings.SearchAreaSize / 2.5f);

            CalloutMessage = "Domestic Violence";
            CalloutDetails = "A ~o~wife~s~ called about her ~r~husband~s~, claims ~y~he's hurting her~s~, shortly after she ~y~hung up abrubtly~s~.";
            CalloutScenario = GetRandomScenarioNumber(5);

            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_DOMESTIC_VIOLENCE IN_OR_ON_POSITION UNITS_RESPOND_CODE_03", CalloutPosition);

            return base.OnBeforeCalloutDisplayed();
        }

        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("[Emergency Callouts]: Callout not accepted");
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
                Game.LogTrivial("[Emergency Callouts]: Created EntranceBlip");

                // Suspect
                Suspect = new Ped(Entity.GetRandomMaleModel(), CalloutPosition, 0f);
                Suspect.SetDefaults();
                Game.LogTrivial($"[Emergency Callouts]: Created Suspect ({Suspect.Model.Name}) at " + Suspect.Position);

                // SuspectBlip
                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColor(Colors.Red);
                SuspectBlip.ScaleForPed();
                SuspectBlip.Disable();

                // Victim
                Victim = new Ped(Entity.GetRandomFemaleModel(), CalloutPosition, 0f);
                Victim.SetDefaults();
                Victim.Health = 135;
                Game.LogTrivial($"[Emergency Callouts]: Created Victim ({Victim.Model.Name}) at " + Victim.Position);

                // VictimBlip
                VictimBlip = Victim.AttachBlip();
                VictimBlip.SetColor(Colors.Orange);
                VictimBlip.ScaleForPed();
                VictimBlip.Disable();

                // 50% Drunk Chance
                int num = new Random().Next(2);
                if (num == 1)
                {
                    Suspect.SetIntoxicated();
                    Game.LogTrivial("[Emergency Callouts]: Set Suspect intoxicated");
                }

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
                if (CalloutPosition == CalloutPositions[0]) // Vinewood Hills
                {
                    Center = new Vector3(0f, 526.2725f, 175.1643f);
                    Entrance = new Vector3(11.3652f, 545.7453f, 175.8412f);
                    EntranceBlip.Position = Entrance;
                }
                else if (CalloutPosition == CalloutPositions[1]) // Davis
                {
                    Center = new Vector3(208.0452f, -1707.766f, 29.65307f);
                    Entrance = new Vector3(222.883f, -1726.32f, 28.87364f);
                    EntranceBlip.Position = Entrance;
                }
                else if (CalloutPosition == CalloutPositions[2]) // Sandy Shores
                {
                    Center = new Vector3(247.4916f, 3169.519f, 42.7863f);
                    Entrance = new Vector3(224.5887f, 3162.886f, 42.3335f);
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
                Game.LogTrivial("[Emergency Callouts]: Enabled route to EntranceBlip");
            }
            catch (Exception e)
            {
                Log.CalloutException(this, "CalloutHandler", e);
            }
            #endregion
        }

        private void RetrieveFightPosition()
        {
            #region Positions
            if (CalloutPosition == CalloutPositions[0]) // Vinewood Hills
            {
                int num = new Random().Next(VinewoodHillsFightPositions.Length);
                
                Victim.Position = VinewoodHillsFightPositions[num];
                Victim.Heading = VinewoodHillsFightHeadings[num];
                Suspect.Position = Victim.GetOffsetPositionFront(1f);
            }
            else if (CalloutPosition == CalloutPositions[1]) // Davis
            {
                int num = new Random().Next(DavisFightPositions.Length);
                Victim.Position = DavisFightPositions[num];
                Victim.Heading = DavisFightHeadings[num];
                Suspect.Position = Victim.GetOffsetPositionFront(1f);
            }
            else if (CalloutPosition == CalloutPositions[2]) // Sandy Shores
            {
                int num = new Random().Next(SandyShoresFightPositions.Length);
                Victim.Position = SandyShoresFightPositions[num];
                Victim.Heading = SandyShoresFightHeadings[num];
                Suspect.Position = Victim.GetOffsetPositionFront(1f);
            }
            #endregion
        }

        private void Dialogue()
        {
            #region Dialogue
            string[] dialogue =
            {
                "~b~You~s~: M'am, are you injured?",
                "~g~Victim~s~: Yes, I'm hurt alot.",
                "~b~You~s~: Okay, I'm gonna get an ambulance over here for you okay?.",
                "~g~Victim~s~: Okay, but I'm pretty sure I'm gonna go unconscious...",
                "~b~You~s~: Try to relax, positive thoughts only okay?",
                "~g~Victim~s~: Okay, I can do th..."
            };

            int line = 0;

            GameFiber.StartNew(delegate
            {
                while (CalloutActive)
                {
                    GameFiber.Yield();

                    if (MainPlayer.Position.DistanceTo(Victim.Position) < 3f && Suspect.IsDead && Victim.IsAlive)
                    {
                        if (Game.IsKeyDown(Settings.TalkKey))
                        {
                            Victim.Tasks.Clear();
                            Game.LogTrivial("[Emergency Callouts]: Cleared Victim tasks");

                            DialogueStarted = true;
                            Game.LogTrivial("[Emergency Callouts]: Dialogue Started");

                            Victim.Tasks.AchieveHeading(MainPlayer.Heading - 180);
                            Game.LogTrivial("[Emergency Callouts]: Victim achieved player heading");

                            Game.DisplaySubtitle(dialogue[line], 99999);
                            Game.LogTrivial("[Emergency Callouts]: Displayed dialogue line " + line);

                            line++;

                            if (line == dialogue.Length)
                            {
                                GameFiber.Sleep(500);
                                Entity.Kill(Victim);

                                GameFiber.Sleep(1000);
                                Display.HideSubtitle();

                                Game.LogTrivial("[Emergency Callouts]: Dialogue Ended");
                            }
                            GameFiber.Sleep(500);
                        }
                        else
                        {
                            if (DialogueStarted == false)
                            {
                                Game.DisplayHelp("Press ~y~Y~s~ to talk to the ~o~Victim~s~.");
                            }
                        }
                    }
                }
            });
            #endregion
        }

        private void Scenario1() // Suspect Keeps Fighting Victim
        {
            #region Scenario 1
            try
            {
                // Retrieve Fight Position
                RetrieveFightPosition();
                Game.LogTrivial("[Emergency Callouts]: Retrieved fight position");

                // Victim Invincible
                Victim.IsInvincible = true;
                Game.LogTrivial("Emergency Callouts: Set Victim invincible");

                // Victim Cowering
                Victim.Tasks.Cower(-1);
                Game.LogTrivial("Emergency Callouts: Victim cowering");
                
                // Suspect Fighting Victim
                Suspect.Tasks.FightAgainst(Victim);
                Game.LogTrivial("Emergency Callouts: Suspect fighting Victim");

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (PedFound == true)
                        {
                            // Victim Invincible
                            Victim.IsInvincible = false;

                            // Victim Cowering
                            Victim.Tasks.Cower(-1);
                            Game.LogTrivial("Emergency Callouts: Assigned Victim to cower");

                            break;
                        }
                    }
                });

                Dialogue();
            }
            catch (Exception e)
            {
                Log.CalloutException(this, "Scenario1", e);
            }
            #endregion
        }

        private void Scenario2() // Suspect Shoots Victim Then Fights Player
        {
            #region Scenario 2
            try
            {
                // Retrieve Fight Spot
                RetrieveFightPosition();
                Game.LogTrivial("[Emergency Callouts]: Retrieved fight position");

                // Lower Victim health
                Victim.Health = 130;

                // Give Random Handgun
                Suspect.GiveRandomWeapon(WeaponType.Handgun, -1, true);
                Game.LogTrivial($"[Emergency Callouts]: Assigned new Random() handgun to Suspect inventory");

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (PlayerArrived == true)
                        {
                            // Husband Fighting Wife
                            Suspect.Tasks.FightAgainst(Victim);
                            Game.LogTrivial("[Emergency Callouts]: Assigned Suspect to fight Victim");

                            break;
                        }
                    }

                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (Victim.IsDead && Victim.Exists())
                        {
                            // Husband Fighting Player
                            Suspect.Tasks.FightAgainst(MainPlayer);
                            Game.LogTrivial("[Emergency Callouts]: Assigned Suspect to fight player");
                            
                            break;
                        }
                    }
                });

                Dialogue();
            }
            catch (Exception e)
            {
                Log.CalloutException(this, "Scenario2", e);
            }
            #endregion
        }

        private void Scenario3() // Suspect Sitting Next To Dead Victim
        {
            #region Scenario 3
            try
            {
                // Retrieve Fight Position
                RetrieveFightPosition();
                Game.LogTrivial("Emergency Callouts: Retrieved fight position");

                // Kill Victim
                Entity.Kill(Victim);
                Game.LogTrivial("Emergency Callouts: Killed Victim");

                // Delete VictimBlip
                Entity.Delete(VictimBlip);
                Game.LogTrivial("Emergency Callouts: Deleted VictimBlip");

                // Suspect Sitting
                Suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@business@bgen@bgen_no_work@"), "sit_phone_idle_03_nowork", 5f, AnimationFlags.Loop);
                Game.LogTrivial("Emergency Callouts: Assigned Suspect to play animation");
            }
            catch (Exception e)
            {
                Log.CalloutException(this, "Scenario3", e);
            }
            #endregion
        }

        private void Scenario4() // Victim at gunpoint, surrender
        {
            #region Scenario 4
            try
            {
                // Retrieve Fight Position
                RetrieveFightPosition();
                Game.LogTrivial("[Emergency Callouts]: Retrieved fight position");

                // Suspect Position
                Suspect.Position = Victim.GetOffsetPositionFront(2f);
                Game.LogTrivial("[Emergency Callouts]: Changed Suspect position");

                // Give Random Handgun
                Suspect.GiveRandomWeapon(WeaponType.Handgun, -1, true);
                Game.LogTrivial($"[Emergency Callouts]: Assigned new Random() handgun to Suspect inventory");

                // Aim at Victim
                Suspect.Tasks.AimWeaponAt(Victim, -1);
                Game.LogTrivial("[Emergency Callouts]: Assigned Suspect to aim weapon at Victim");

                // Victim Cowering
                Victim.Tasks.Cower(-1);
                Game.LogTrivial("[Emergency Callouts]: Assigned Victim to cower");

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 10f)
                        {
                            // Suspect Putting Hands Up
                            Suspect.Tasks.PutHandsUp(-1, MainPlayer);
                            Game.LogTrivial("[Emergency Callouts]: Husband putting hands up");

                            break;
                        }
                    }
                });

                Dialogue();
            }
            catch (Exception e)
            {
                Log.CalloutException(this, "Scenario4", e);
            }
            #endregion
        }

        private void Scenario5() // Victim at gunpoint, firefight
        {
            #region Scenario 5
            try
            {
                // Retrieve Fight Position
                RetrieveFightPosition();
                Game.LogTrivial("[Emergency Callouts]: Retrieved fight position");

                // Suspect Position
                Suspect.Position = Victim.GetOffsetPositionFront(2f);
                Game.LogTrivial("[Emergency Callouts]: Changed Suspect position");

                // Give Random Handgun
                Suspect.GiveRandomWeapon(WeaponType.Handgun, -1, true);
                Game.LogTrivial($"[Emergency Callouts]: Assigned new Random() handgun to Suspect inventory");

                // Aim at Victim
                Suspect.Tasks.AimWeaponAt(Victim, -1);
                Game.LogTrivial("[Emergency Callouts]: Assigned Suspect to aim weapon at Victim");

                // Victim Cowering
                Victim.Tasks.Cower(-1);
                Game.LogTrivial("[Emergency Callouts]: Assigned Victim to cower");

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 10f)
                        {
                            // Fight Player
                            Suspect.Tasks.FightAgainst(MainPlayer);
                            Game.LogTrivial("[Emergency Callouts]: Assigned Suspect to fight player");

                            break;
                        }
                    }
                });

                Dialogue();
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
                Handle.PreventFirstResponderCrash(Suspect, Victim);

                #region PlayerArrived
                if (MainPlayer.Position.DistanceTo(Entrance) < 15f && PlayerArrived == false)
                {
                    // Set PlayerArrived
                    PlayerArrived = true;

                    // Display Arriving Subtitle
                    Game.DisplaySubtitle("Find the ~o~victim~s~ and ~r~husband~s~ in the ~y~area~s~.", 10000);

                    // Disable route
                    EntranceBlip.DisableRoute();

                    // Delete EntranceBlip
                    Entity.Delete(EntranceBlip);

                    // Create SearchArea
                    SearchArea = new Blip(Center, 85f);
                    SearchArea.SetColor(Colors.Yellow);
                    SearchArea.Alpha = 0.5f;

                    Game.LogTrivial("[Emergency Callouts]: Player arrived on scene");
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
                    Entity.Delete(SearchArea);

                    Game.LogTrivial("[Emergency Callouts]: Player found suspect");
                }

                if (MainPlayer.Position.DistanceTo(Victim.Position) < 5f && !Ped2Found && PlayerArrived && Victim.Exists())
                {
                    // Set Ped2Found
                    Ped2Found = true;

                    // Hide Subtitle
                    Display.HideSubtitle();

                    // Enable VictimBlip
                    VictimBlip.Enable();

                    // Delete SearchArea
                    Entity.Delete(SearchArea);

                    Game.LogTrivial("[Emergency Callouts]: Player found victim");
                }
                #endregion

                #region PedDetained
                if (Suspect.IsPedDetained() == true && PedDetained == false && Suspect.Exists())
                {
                    // Set PedDetained
                    PedDetained = true;
                    Game.LogTrivial("[Emergency Callouts]: Suspect detained");

                    // Delete SuspectBlip
                    Entity.Delete(SuspectBlip);
                    Game.LogTrivial("[Emergency Callouts]: Deleted SuspectBlip");
                }
                #endregion

                #region PlayerLeft
                if (MainPlayer.Position.DistanceTo(CalloutPosition) > Settings.SearchAreaSize * 3.5f && PlayerArrived == true)
                {
                    // Set PlayerArrived
                    PlayerArrived = false;

                    // Disable SuspectBlip
                    SuspectBlip.Disable();

                    // Delete SearchArea
                    Entity.Delete(SearchArea);

                    // Create EntranceBlip
                    EntranceBlip = new Blip(Entrance);

                    // Enable Route
                    EntranceBlip.EnableRoute();

                    Game.LogTrivial("[Emergency Callouts]: Player left callout position");
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
            Entity.Dismiss(Victim);
            Entity.Delete(SuspectBlip);
            Entity.Delete(VictimBlip);
            Entity.Delete(SearchArea);
            Entity.Delete(EntranceBlip);

            Display.HideSubtitle();
            Display.DetachMessage();
            Log.CalloutEnded(CalloutMessage, CalloutScenario);
        }
    }
}