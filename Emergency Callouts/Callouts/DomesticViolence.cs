using EmergencyCallouts.Essential;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Reflection;
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
        bool FirstTime;
        bool WithinRange;

        Vector3 Entrance;
        Vector3 Center;

        // Main
        #region Positions
        readonly Vector3[] CalloutPositions =
        {
            new Vector3(11.3652f, 545.7453f, 175.8412f),    // Vinewood Hills
            new Vector3(222.883f, -1726.32f, 28.87364f),    // Davis
            new Vector3(-1048.924f, -1018.362f, 2.150359f), // Vespucci
            new Vector3(224.5887f, 3162.886f, 42.3335f),    // Sandy Shores
            new Vector3(1687.845f, 4680.918f, 43.02761f),   // Grapeseed
            new Vector3(-394.975f, 6276.961f, 29.67487f),   // Paleto Bay
        };
        #endregion

        // Vinewood Hills
        #region Positions
        readonly Vector3[] VinewoodHillsFightPositions =
        {
            new Vector3(24.13852f, 520.5587f, 170.2275f),   // Chill Area 1
            new Vector3(-6.628098f, 509.4984f, 170.6278f), // Chill Area 2
        };

        readonly float[] VinewoodHillsFightHeadings =
        {
            24.09f,
            61.64f,
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

        // Vespucci
        #region Positions
        readonly Vector3 VespucciFightPosition = new Vector3(-1058.305f, -995.6418f, 6.410485f); // Front
        readonly float VespucciFightHeading = 205.96f;
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

        // Grapeseed
        #region Positions
        readonly Vector3[] GrapeseedFightPositions =
        {
            new Vector3(1684.434f, 4692.222f, 43.00724f),  // Frontyard
            new Vector3(1661.172f, 4688.735f, 43.20671f),  // Backyard
            new Vector3(1673.626f, 4680.47f, 43.05536f),   // Side
        };

        readonly float[] GrapeseedFightHeadings =
        {
            169.98f,
            180.06f,
            272.81f,
        };
        #endregion

        // Paleto Bay
        #region Positions
        readonly Vector3[] PaletoBayFightPositions =
        {
            new Vector3(-388.4336f, 6255.619f, 31.48756f), // Frontyard
            new Vector3(-374.013f, 6243.474f, 31.48722f),  // Backyard
            new Vector3(-374.2228f, 6259.589f, 31.48723f), // Side
        };

        readonly float[] PaletoBayFightHeadings =
        {
            301.78f,
            144.90f,
            132.13f,
        };
        #endregion

        Ped Suspect;
        Ped Victim;

        Persona SuspectPersona;
        Persona VictimPersona;

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
                    CalloutArea = World.GetStreetName(loc);
                }
            }

            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, Settings.SearchAreaSize / 2.5f);

            CalloutMessage = Localization.DomesticViolence;
            CalloutDetails = Localization.DomesticViolenceDetails;
            CalloutScenario = GetRandomScenarioNumber(5);

            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_DOMESTIC_VIOLENCE IN_OR_ON_POSITION UNITS_RESPOND_CODE_03", CalloutPosition);

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

                // Suspect
                Suspect = new Ped(Entity.GetRandomMaleModel(), CalloutPosition, 0f);
                SuspectPersona = Functions.GetPersonaForPed(Suspect);
                Suspect.SetDefaults();

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColorRed();
                SuspectBlip.Scale = (float)Settings.PedBlipScale;
                SuspectBlip.Disable();

                // Victim
                Victim = new Ped(Entity.GetRandomFemaleModel(), CalloutPosition, 0f);
                VictimPersona = Functions.GetPersonaForPed(Victim);
                Victim.SetDefaults();
                Victim.SetInjured(135);

                VictimBlip = Victim.AttachBlip();
                VictimBlip.SetColorOrange();
                VictimBlip.Scale = (float)Settings.PedBlipScale;
                VictimBlip.Disable();
                
                // 50% Drunk Chance
                int num = random.Next(2);
                if (num == 1)
                {
                    Suspect.SetIntoxicated();
                    Game.LogTrivial("[Emergency Callouts]: Set Suspect intoxicated");
                }

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
                if (CalloutPosition == CalloutPositions[0]) // Vinewood Hills
                {
                    Center = new Vector3(0f, 526.2725f, 175.1643f);
                    Entrance = new Vector3(11.3652f, 545.7453f, 175.8412f);
                }
                else if (CalloutPosition == CalloutPositions[1]) // Davis
                {
                    Center = new Vector3(208.0452f, -1707.766f, 29.65307f);
                    Entrance = new Vector3(222.883f, -1726.32f, 28.87364f);
                }
                else if (CalloutPosition == CalloutPositions[2]) // Vespucci
                {
                    Center = new Vector3(-1058.305f, -995.6418f, 6.410485f);
                    Entrance = new Vector3(-1048.924f, -1018.362f, 2.150359f);
                }
                else if (CalloutPosition == CalloutPositions[3]) // Sandy Shores
                {
                    Center = new Vector3(247.4916f, 3169.519f, 42.7863f);
                    Entrance = new Vector3(224.5887f, 3162.886f, 42.3335f);
                }
                else if (CalloutPosition == CalloutPositions[4]) // Grapeseed
                {
                    Center = new Vector3(1672.969f, 4670.249f, 43.40202f);
                    Entrance = new Vector3(1687.845f, 4680.918f, 43.02761f);
                }
                else if (CalloutPosition == CalloutPositions[5]) // Paleto Bay
                {
                    Center = new Vector3(-374.2228f, 6259.589f, 31.48723f);
                    Entrance = new Vector3(-394.975f, 6276.961f, 29.67487f);
                }

                Suspect.Position = Victim.GetOffsetPositionFront(1f);
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

                // EntranceBlip
                EntranceBlip = new Blip(Entrance);
                EntranceBlip.EnableRoute();
                Game.LogTrivial("[Emergency Callouts]: Enabled route to EntranceBlip");

                // Log Creation
                Log.Creation(Suspect, PedCategory.Suspect);
                Log.Creation(Victim, PedCategory.Victim);
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            }
            #endregion
        }

        private void RetrieveFightPosition()
        {
            #region Positions
            if (CalloutPosition == CalloutPositions[0]) // Vinewood Hills
            {
                int num = random.Next(VinewoodHillsFightPositions.Length);
                
                Victim.Position = VinewoodHillsFightPositions[num];
                Victim.Heading = VinewoodHillsFightHeadings[num];
                Suspect.Position = Victim.GetOffsetPositionFront(1f);
            }
            else if (CalloutPosition == CalloutPositions[1]) // Davis
            {
                int num = random.Next(DavisFightPositions.Length);

                Victim.Position = DavisFightPositions[num];
                Victim.Heading = DavisFightHeadings[num];
                Suspect.Position = Victim.GetOffsetPositionFront(1f);
            }
            else if (CalloutPosition == CalloutPositions[2]) // Vespucci
            {
                Victim.Position = VespucciFightPosition;
                Victim.Heading = VespucciFightHeading;
                Suspect.Position = Victim.GetOffsetPositionFront(1f);
            }
            else if (CalloutPosition == CalloutPositions[3]) // Sandy Shores
            {
                int num = random.Next(SandyShoresFightPositions.Length);

                Victim.Position = SandyShoresFightPositions[num];
                Victim.Heading = SandyShoresFightHeadings[num];
                Suspect.Position = Victim.GetOffsetPositionFront(1f);
            }
            else if (CalloutPosition == CalloutPositions[4]) // Grapeseed
            {
                int num = random.Next(GrapeseedFightPositions.Length);

                Victim.Position = GrapeseedFightPositions[num];
                Victim.Heading = GrapeseedFightHeadings[num];
                Suspect.Position = Victim.GetOffsetPositionFront(1f);
            }
            else if (CalloutPosition == CalloutPositions[5]) // Paleto Bay
            {
                int num = random.Next(PaletoBayFightPositions.Length);

                Victim.Position = PaletoBayFightPositions[num];
                Victim.Heading = PaletoBayFightHeadings[num];
                Suspect.Position = Victim.GetOffsetPositionFront(1f);
            }
            #endregion
        }

        private void Dialogue()
        {
            #region Dialogue

            string[] dialogueArrested =
            {
                "~b~You~s~: Ma'am, are you injured?",
                "~o~Victim~s~: He hit me multiple times, but no need for an ambulance.",
                "~b~You~s~: Okay, is this your property?",
                "~o~Victim~s~: Thankfully it is, otherwise I'd be homeless tonight",
                "~b~You~s~: I assume you want to press charges?",
                "~o~Victim~s~: Yes, and how do I get a restraining order?",
                "~b~You~s~: You'll need to go to the courthouse and get the necessary forms.",
                "~o~Victim~s~: Thank you for helping me.",
                "~b~You~s~: No problem, here is my card if you have any questions or need any help.",
                "~o~Victim~s~: Thanks, one more thing, how long will he be in jail?",
                "~b~You~s~: I'd guess around 10 or 15 years.",
                "~o~Victim~s~: Good, he's an ex-convict so they'll be harder on him.",
                "~b~You~s~: I'm gonna have to process him, other officers will help you further.",
                "~m~dialogue ended",
            };

            string[] dialogueDeceased =
            {
                "~b~You~s~: Ma'am, are you hurt?",
                "~o~Victim~s~: He hit me multiple times, but no need for an ambulance.",
                "~b~You~s~: Okay, is this property yours?",
                "~o~Victim~s~: Yes it is.",
                "~b~You~s~: The body will get moved soon.",
                "~o~Victim~s~: Good, what about the blood?",
                "~b~You~s~: That will be taken care of by crime scene cleaners.",
                "~o~Victim~s~: Okay, thanks",
                "~b~You~s~: Here is my card if you have any questions or need any help.",
                "~o~Victim~s~: Thanks.",
                "~b~You~s~: No problem, I'm gonna have to do some more things, other officers will help you further.",
                "~m~dialogue ended",
            };

            int line = 0;

            GameFiber.StartNew(delegate
            {
                while (CalloutActive)
                {
                    GameFiber.Yield();

                    if (Victim.IsAlive && (Suspect.IsDead || Suspect.IsCuffed))
                    {
                        if (!DialogueStarted && !FirstTime)
                        {
                            GameFiber.Sleep(5000);
                            Game.DisplaySubtitle("Speak to the ~o~victim", 10000);
                            FirstTime = true;
                        }

                        if (MainPlayer.Position.DistanceTo(Victim.Position) < 3f && FirstTime)
                        {
                            if (Game.IsKeyDown(Settings.InteractKey))// && !stopDialogue
                            {
                                if (!DialogueStarted)
                                {
                                    Victim.Tasks.Clear();
                                    Game.LogTrivial("[Emergency Callouts]: Dialogue started with " + VictimPersona.FullName);
                                }

                                DialogueStarted = true;

                                // Face the player
                                Victim.Tasks.AchieveHeading(MainPlayer.Heading - 180f);

                                // Victim dialogue
                                if (Suspect.IsCuffed)
                                {
                                    Game.DisplaySubtitle(dialogueArrested[line], 15000);
                                    Game.LogTrivial("[Emergency Callouts]: Displayed dialogue line " + line);
                                    line++;

                                    if (line == dialogueArrested.Length)
                                    {
                                        Game.LogTrivial("[Emergency Callouts]: Dialogue Ended");
                                        //stopDialogue = true;

                                        foreach (Ped ped in World.GetAllPeds())
                                        {
                                            if (Functions.IsPedACop(ped) && ped.IsAlive && Victim.Position.DistanceTo(ped.Position) <= 20f)
                                            {
                                                Victim.Tasks.GoStraightToPosition(ped.Position, 2f, 1f, 0f, 0);
                                            }
                                        }

                                        break;
                                    }
                                }
                                else if (Suspect.IsDead)
                                {
                                    Game.DisplaySubtitle(dialogueDeceased[line], 15000);
                                    Game.LogTrivial("[Emergency Callouts]: Displayed dialogue line " + line);
                                    line++;

                                    if (line == dialogueDeceased.Length)
                                    {
                                        Game.LogTrivial("[Emergency Callouts]: Dialogue Ended");

                                        foreach (Ped ped in World.GetAllPeds())
                                        {
                                            if (Functions.IsPedACop(ped) && ped.IsAlive && Victim.Position.DistanceTo(ped.Position) <= 20f && ped != MainPlayer)
                                            {
                                                Victim.Tasks.GoToOffsetFromEntity(ped, 1f, 0f, 2f);
                                            }
                                        }

                                        break;
                                    }
                                }

                                // Give your business card
                                if (line == 9)
                                {
                                    MainPlayer.Tasks.GoToOffsetFromEntity(Victim, 1f, 0f, 2f);
                                    //GameFiber.Sleep(500);
                                    
                                    Victim.Tasks.ClearImmediately();
                                    Victim.Tasks.PlayAnimation(new AnimationDictionary("mp_common"), "givetake1_b", 5f, AnimationFlags.SecondaryTask | AnimationFlags.UpperBodyOnly);
                                    MainPlayer.Tasks.PlayAnimation(new AnimationDictionary("mp_common"), "givetake1_b", 5f, AnimationFlags.SecondaryTask | AnimationFlags.UpperBodyOnly);
                                }

                                GameFiber.Sleep(500);
                            }
                            else if (!DialogueStarted && MainPlayer.Position.DistanceTo(Victim.Position) <= 2f)
                            {
                                Game.DisplayHelp($"Press ~y~{Settings.InteractKey}~s~ to talk to the ~o~victim~s~.");
                            }
                        }
                    }
                    else if (Victim.IsDead) // Victim is dead
                    {
                        break;
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

                // Victim Invincible
                Victim.IsInvincible = true;

                // Victim Cowering
                Victim.Tasks.Cower(-1);
                
                // Suspect Fighting Victim
                Suspect.Tasks.FightAgainst(Victim);

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 10f && Suspect.Exists() && PlayerArrived)
                        {
                            // Victim Invincible
                            Victim.IsInvincible = false;

                            // Victim Cowering
                            Victim.Tasks.Cower(-1);

                            break;
                        }
                    }
                });

                Dialogue();
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
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

                // Give Random Handgun
                Suspect.GiveRandomHandgun(-1, true);

                // Aim At Victim
                Suspect.Tasks.AimWeaponAt(Victim, -1);

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (PlayerArrived)
                        {
                            // Husband Fighting Wife
                            Suspect.Tasks.FightAgainst(Victim);

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
                            
                            break;
                        }
                    }
                });

                Dialogue();
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            }
            #endregion
        }

        private void Scenario3() // Suspect Sitting Next To Dead Victim
        {
            #region Scenario 3
            try
            {
                #region Dialogue
                bool stopDialogue = false;

                string[] dialogueSuspect =
                {
                    "~b~You~s~: Why would you do such a thing?",
                    "~r~Suspect~s~: You wouldn't understand.",
                    "~b~You~s~: You're right, animals can't talk to humans.",
                    "~r~Suspect~s~: Keep your mouth shut.",
                    "~b~You~s~: You think you're tough but you're not.",
                    "~r~Suspect~s~: Says the person who needs a gun, tazer, pepperspray and a nightstick.",
                    "~b~You~s~: Yeah I need those, but never for your kind.",
                    "~m~dialogue ended",
                };

                int line = 0;

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (!DialogueStarted && !FirstTime && Suspect.IsCuffed && Suspect.IsAlive)
                        {
                            GameFiber.Sleep(5000);
                            Game.DisplaySubtitle("Speak to the ~r~suspect", 10000);
                            FirstTime = true;
                        }

                        if (Game.IsKeyDown(Settings.InteractKey) && !stopDialogue && Suspect.IsCuffed && MainPlayer.Position.DistanceTo(Suspect.Position) < 3f && FirstTime)
                        {
                            if (!DialogueStarted)
                            {
                                Suspect.Tasks.Clear();

                                Game.LogTrivial("[Emergency Callouts]: Dialogue started with " + SuspectPersona.FullName);
                            }

                            DialogueStarted = true;

                            Suspect.Tasks.AchieveHeading(MainPlayer.Heading - 180f);

                            Game.DisplaySubtitle(dialogueSuspect[line], 15000);
                            line++;
                            Game.LogTrivial("[Emergency Callouts]: Displayed dialogue line " + line);

                            if (line == dialogueSuspect.Length)
                            {
                                Game.LogTrivial("[Emergency Callouts]: Dialogue Ended");
                                stopDialogue = true;
                            }

                            GameFiber.Sleep(500);
                        }
                        else
                        {
                            if (!DialogueStarted && Suspect.IsCuffed)
                            {
                                Game.DisplayHelp($"Press ~y~{Settings.InteractKey}~s~ to talk to the ~r~suspect~s~.");
                            }
                        }
                    }
                });
                #endregion

                // Retrieve Fight Position
                RetrieveFightPosition();

                // Kill Victim
                if (Victim.Exists()) { Victim.Kill(); }

                // Delete VictimBlip
                if (VictimBlip.Exists()) { VictimBlip.Delete(); }
                Suspect.Position = Victim.GetOffsetPositionFront(2f);

                // Suspect Sitting
                Suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@business@bgen@bgen_no_work@"), "sit_phone_idle_03_nowork", 5f, AnimationFlags.Loop);
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
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

                // Suspect Position
                Suspect.Position = Victim.GetOffsetPositionFront(2f);

                // Give Random Handgun
                Suspect.GiveRandomHandgun(-1, true);

                // Aim at Victim
                Suspect.Tasks.AimWeaponAt(Victim, -1);

                // Victim Cowering
                Victim.Tasks.Cower(-1);

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 10f && PlayerArrived)
                        {
                            // Suspect Putting Hands Up
                            Suspect.Tasks.PutHandsUp(-1, MainPlayer);

                            break;
                        }
                    }
                });

                Dialogue();
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
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

                // Suspect Position
                Suspect.Position = Victim.GetOffsetPositionFront(2f);

                // Give Random Handgun
                Suspect.GiveRandomHandgun(-1, true);

                // Aim at Victim
                Suspect.Tasks.AimWeaponAt(Victim, -1);

                // Victim Cowering
                Victim.Tasks.Cower(-1);

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 10f && PlayerArrived)
                        {
                            // Fight Player
                            Suspect.Tasks.FightAgainst(MainPlayer);

                            break;
                        }
                    }
                });

                Dialogue();
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
                Handle.PreventDistanceCrash(CalloutPosition, PlayerArrived, PedFound);
                Handle.PreventPickupCrash(Suspect, Victim);

                #region WithinRange
                if (MainPlayer.Position.DistanceTo(CalloutPosition) <= 200f && !WithinRange)
                {
                    // Set WithinRange
                    WithinRange = true;

                    // Delete Nearby Peds
                    Handle.DeleteNearbyPeds(Suspect, 20f);

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} is within 200 meters");
                }
                #endregion

                #region PlayerArrived
                if (MainPlayer.Position.DistanceTo(Entrance) < 15f && !PlayerArrived)
                {
                    // Set PlayerArrived
                    PlayerArrived = true;

                    // Gang Attack Fix
                    Handle.BlockPermanentEventsRadius(Center, 100f);

                    // Display Arriving Subtitle
                    Game.DisplaySubtitle(Localization.DomesticViolenceSubtitle, 10000);

                    // Disable route
                    EntranceBlip.DisableRoute();

                    // Delete EntranceBlip
                    if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

                    // Create SearchArea
                    SearchArea = new Blip(Suspect.Position.Around2D(5f, 20f), Settings.SearchAreaSize);
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

                    // Hide Subtitle
                    Display.HideSubtitle();

                    // Enable SuspectBlip
                    SuspectBlip.Enable();

                    // Delete SearchArea
                    if (SearchArea.Exists()) { SearchArea.Delete(); }

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has found {SuspectPersona.FullName} (Suspect)");
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
                    if (SearchArea.Exists()) { SearchArea.Delete(); }

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has found {VictimPersona.FullName} (Victim)");
                }
                #endregion

                #region PedDetained
                if (Functions.IsPedStoppedByPlayer(Suspect) && !PedDetained && Suspect.Exists())
                {
                    // Set PedDetained
                    PedDetained = true;
                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has detained {SuspectPersona.FullName} (Suspect)");

                    // Delete SuspectBlip
                    if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                    Game.LogTrivial("[Emergency Callouts]: Deleted SuspectBlip");
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
            if (Victim.Exists()) { Victim.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            if (VictimBlip.Exists()) { VictimBlip.Delete(); }
            if (SearchArea.Exists()) { SearchArea.Delete(); }
            if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

            Display.HideSubtitle();
            Display.EndNotification();
            Log.OnCalloutEnded(CalloutMessage, CalloutScenario);
        }
    }
}