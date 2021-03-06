using EmergencyCallouts.Essential;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using RAGENativeUI;
using System;
using System.Linq;
using System.Reflection;
using static EmergencyCallouts.Essential.Color;
using static EmergencyCallouts.Essential.Helper;
using static EmergencyCallouts.Essential.Inventory;
using Entity = EmergencyCallouts.Essential.Helper.Entity;

namespace EmergencyCallouts.Callouts
{
    [CalloutInfo("[EC] Domestic Violence", CalloutProbability.Medium)]
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
            new Vector3(1504.92f, 2203.887f, 79.99944f),    // County
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

        // County
        #region Positions
        readonly Vector3[] CountyFightPositions =
        {
            new Vector3(1534.577f, 2228.416f, 77.69907f), // Front Door
            new Vector3(1551.911f, 2228.493f, 77.83331f), // Rear Garden
            new Vector3(1538.637f, 2238.759f, 77.69897f), // Side House
        };

        readonly float[] CountyFightHeadings =
        {
            359.88f,
            3.25f,
            271.88f,
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
                    CalloutArea = World.GetStreetName(loc).Replace("Senora Fwy", "Grand Senora Desert");
                }
            }

            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, Settings.SearchAreaSize / 2.5f);

            CalloutMessage = "Domestic Violence";
            CalloutAdvisory = "Passersby report a male continuingly hitting a female.";
            CalloutScenario = random.Next(1, 4);

            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_DOMESTIC_VIOLENCE IN_OR_ON_POSITION UNITS_RESPOND_CODE_03", CalloutPosition);

            return base.OnBeforeCalloutDisplayed();
        }

        public override void OnCalloutDisplayed()
        {
            if (Other.PluginChecker.IsCalloutInterfaceRunning)
            {
                Other.CalloutInterfaceFunctions.SendCalloutDetails(this, "CODE 3", "");
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
                else if (CalloutPosition == CalloutPositions[3]) // County
                {
                    Center = new Vector3(1550.415f, 2203.19f, 78.74243f);
                    Entrance = new Vector3(1504.92f, 2203.887f, 79.99944f);
                }
                else if (CalloutPosition == CalloutPositions[4]) // Sandy Shores
                {
                    Center = new Vector3(247.4916f, 3169.519f, 42.7863f);
                    Entrance = new Vector3(224.5887f, 3162.886f, 42.3335f);
                }
                else if (CalloutPosition == CalloutPositions[5]) // Grapeseed
                {
                    Center = new Vector3(1672.969f, 4670.249f, 43.40202f);
                    Entrance = new Vector3(1687.845f, 4680.918f, 43.02761f);
                }
                else if (CalloutPosition == CalloutPositions[6]) // Paleto Bay
                {
                    Center = new Vector3(-374.2228f, 6259.589f, 31.48723f);
                    Entrance = new Vector3(-394.975f, 6276.961f, 29.67487f);
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

                // Victim
                Victim = new Ped(Entity.GetRandomFemaleModel(), Vector3.Zero, 0f);
                VictimPersona = Functions.GetPersonaForPed(Victim);
                Victim.IsPersistent = true;
                Victim.BlockPermanentEvents = true;
                Victim.SetInjured(135);

                VictimBlip = Victim.AttachBlip();
                VictimBlip.SetColorOrange();
                VictimBlip.Scale = (float)Settings.PedBlipScale;
                VictimBlip.Alpha = 0f;

                // Suspect
                Suspect = new Ped(Entity.GetRandomMaleModel(), Victim.GetOffsetPositionFront(1f), 0f);
                SuspectPersona = Functions.GetPersonaForPed(Suspect);
                Suspect.IsPersistent = true;
                Suspect.BlockPermanentEvents = true;

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColorRed();
                SuspectBlip.Scale = (float)Settings.PedBlipScale;
                SuspectBlip.Alpha = 0f;

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
            else if (CalloutPosition == CalloutPositions[3]) // County
            {
                int num = random.Next(CountyFightPositions.Length);

                Victim.Position = CountyFightPositions[num];
                Victim.Heading = CountyFightHeadings[num];
                Suspect.Position = Victim.GetOffsetPositionFront(1f);
            }
            else if (CalloutPosition == CalloutPositions[4]) // Sandy Shores
            {
                int num = random.Next(SandyShoresFightPositions.Length);

                Victim.Position = SandyShoresFightPositions[num];
                Victim.Heading = SandyShoresFightHeadings[num];
                Suspect.Position = Victim.GetOffsetPositionFront(1f);
            }
            else if (CalloutPosition == CalloutPositions[5]) // Grapeseed
            {
                int num = random.Next(GrapeseedFightPositions.Length);

                Victim.Position = GrapeseedFightPositions[num];
                Victim.Heading = GrapeseedFightHeadings[num];
                Suspect.Position = Victim.GetOffsetPositionFront(1f);
            }
            else if (CalloutPosition == CalloutPositions[6]) // Paleto Bay
            {
                int num = random.Next(PaletoBayFightPositions.Length);

                Victim.Position = PaletoBayFightPositions[num];
                Victim.Heading = PaletoBayFightHeadings[num];
                Suspect.Position = Victim.GetOffsetPositionFront(1f);
            }

            Log.Creation(Suspect, PedCategory.Suspect);
            Log.Creation(Victim, PedCategory.Victim);
            #endregion
        }

        private void Dialogue()
        {
            #region Dialogue
            try
            {
                string[] dialogueArrested =
                {
                    "~b~You~s~: Ma'am, are you injured?",
                    "~o~Victim~s~: Yes, only a few bruises but that's nothing new.",
                    "~b~You~s~: Okay, is this your property?",
                    "~o~Victim~s~: Thankfully it is, otherwise I'd be homeless tonight",
                    "~b~You~s~: I assume you want to press charges?",
                    "~o~Victim~s~: Yes, and how do I get a restraining order?",
                    "~b~You~s~: You'll need to go to the courthouse and get the necessary forms.",
                    "~o~Victim~s~: Thank you for helping me.",
                    "~b~You~s~: No problem, here is my card if you have any questions or need any help.",
                    "~o~Victim~s~: Thanks, one more thing, how long will he be in jail?",
                    "~b~You~s~: I don't know exactly how long, but it's gonna be long.",
                    "~o~Victim~s~: Good, he's an ex-convict so they'll be harder on him.",
                    "~b~You~s~: I'm gonna have to process him, other officers will help you further.",
                    "~m~dialogue ended",
                };

                string[] dialogueDeceased =
                {
                    "~b~You~s~: Ma'am, are you hurt?",
                    "~o~Victim~s~: Uh, yes I think so...",
                    "~b~You~s~: Okay, is this property yours?",
                    "~o~Victim~s~: Yes it is.",
                    "~b~You~s~: Okay, this is now a crime scene, it will take some time before you enter your house again.",
                    "~o~Victim~s~: Oh, what about the blood?",
                    "~b~You~s~: That will be taken care of by crime scene cleaners.",
                    "~o~Victim~s~: Okay, that's good",
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
                                if (Game.IsKeyDown(Settings.InteractKey) || (Game.IsControllerButtonDown(Settings.ControllerInteractKey) && Settings.AllowController && UIMenu.IsUsingController))
                                {
                                    if (!DialogueStarted)
                                    {
                                        if (!Functions.IsPedKneelingTaskActive(Victim)) { Victim.Tasks.Clear(); }
                                        Game.LogTrivial("[Emergency Callouts]: Dialogue started with " + VictimPersona.FullName);
                                    }

                                    DialogueStarted = true;

                                    // Face the player
                                    if (!Functions.IsPedKneelingTaskActive(Victim)) { Victim.Tasks.AchieveHeading(MainPlayer.Heading - 180f); }

                                    // Victim dialogue
                                    if (Suspect.IsCuffed)
                                    {
                                        Game.DisplaySubtitle(dialogueArrested[line], 15000);
                                        Game.LogTrivial("[Emergency Callouts]: Displayed dialogue line " + line);
                                        line++;

                                        if (line == dialogueArrested.Length)
                                        {
                                            Game.LogTrivial("[Emergency Callouts]: Dialogue Ended");


                                            foreach (Ped ped in World.GetAllPeds())
                                            {
                                                if (Functions.IsPedACop(ped) && ped.IsAlive && Victim.Position.DistanceTo(ped.Position) <= 10f && ped != MainPlayer)
                                                {
                                                    Victim.Tasks.GoStraightToPosition(ped.Position, 1f, 1f, 0f, 0);
                                                }
                                            }

                                            GameFiber.Sleep(3000);
                                            Handle.AdvancedEndingSequence();

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
                                                if (Functions.IsPedACop(ped) && ped.IsAlive && Victim.Position.DistanceTo(ped.Position) <= 10f && ped != MainPlayer) // ped.isstill
                                                {
                                                    Victim.Tasks.GoToOffsetFromEntity(ped, 2f, 0f, 1f);
                                                }
                                            }

                                            GameFiber.Sleep(5000);
                                            Handle.AdvancedEndingSequence();

                                            break;
                                        }
                                    }

                                    // Give officer's card
                                    if (line == 9)
                                    {
                                        MainPlayer.Tasks.GoToOffsetFromEntity(Victim, 1f, 0f, 2f);

                                        Victim.Tasks.ClearImmediately();
                                        Victim.Tasks.PlayAnimation(new AnimationDictionary("mp_common"), "givetake1_b", 5f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask);
                                        MainPlayer.Tasks.PlayAnimation(new AnimationDictionary("mp_common"), "givetake1_b", 5f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask);
                                    }
                                }
                                else if (!DialogueStarted && MainPlayer.Position.DistanceTo(Victim.Position) <= 2f)
                                {
                                    if (Settings.AllowController && UIMenu.IsUsingController)
                                    {
                                        Game.DisplayHelp($"Press ~{Settings.ControllerInteractKey.GetInstructionalId()}~ to talk to the ~o~victim");
                                    }
                                    else
                                    {
                                        Game.DisplayHelp($"Press ~{Settings.InteractKey.GetInstructionalId()}~ to talk to the ~o~victim");
                                    }
                                }
                            }
                        }
                        else if (Victim.IsDead) // Victim is dead
                        {
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

        private void Scenario1() // Assault
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

        private void Scenario2() // Firefight
        {
            #region Scenario 2
            try
            {
                // Retrieve Fight Position
                RetrieveFightPosition();

                // Suspect Position
                GameFiber.Sleep(100);
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
                        if (MainPlayer.Position.DistanceTo(Suspect.Position) <= 15f && PlayerArrived)
                        {
                            Game.DisplaySubtitle("~r~Suspect~s~: YOU SHOULD HAVE NEVER DONE THIS!", 5000);
                            break;
                        }
                    }

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

        private void Scenario3() // Suicide
        {
            #region Scenario 3
            try
            {
                RetrieveFightPosition();

                Suspect.Position = Victim.GetOffsetPositionFront(2f);

                Victim.Kill();

                // Give Random Handgun
                Suspect.GiveRandomHandgun(0, true);

                Suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@code_human_cower@male@base"), "base", -1, 3.20f, -3f, 0, AnimationFlags.Loop);

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 10f && PlayerArrived)
                        {
                            Game.DisplaySubtitle("~r~Suspect~s~: WHAT THE HELL DID I DO!?");
                            GameFiber.Sleep(3000);

                            // Fight Player
                            Suspect.Tasks.PlayAnimation(new AnimationDictionary("mp_suicide"), "pistol", 4f, AnimationFlags.None);
                            GameFiber.Sleep(700);
                            if (Suspect.IsAlive && Suspect.Exists()) { Suspect.Kill(); }

                            // Play gun shot
                            string path = @"lspdfr\audio\scanner\Emergency Callouts Audio\GUNSHOT.wav";
                            System.Media.SoundPlayer player = new System.Media.SoundPlayer(path);
                            if (System.IO.File.Exists(path))
                            {
                                player.Load();
                                player.Play();
                            }
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
                Handle.PreventDistanceCrash(CalloutPosition, PlayerArrived, PedFound);
                Handle.PreventPickupCrash(Suspect, Victim);
                if (Settings.AllowController) { NativeFunction.Natives.xFE99B66D079CF6BC(0, 27, true); }

                #region WithinRange
                if (MainPlayer.Position.DistanceTo(CalloutPosition) <= 200f && !WithinRange)
                {
                    // Set WithinRange
                    WithinRange = true;

                    // Delete Nearby Peds
                    Handle.DeleteNearbyPeds(Suspect, Victim, 40f);

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} is within 200 meters");
                }
                #endregion

                #region PlayerArrived
                if (MainPlayer.Position.DistanceTo(Entrance) < 15f && !PlayerArrived)
                {
                    // Set PlayerArrived
                    PlayerArrived = true;

                    // Gang Attack Fix
                    Handle.BlockPermanentEventsRadius(Center, 200f);

                    // Display Arriving Subtitle
                    Game.DisplaySubtitle("Find the ~o~victim~s~ and the ~r~suspect~s~ in the ~y~area~s~.", 10000);

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

                    // Hide Subtitle
                    Display.HideSubtitle();

                    // Enable SuspectBlip
                    if (SuspectBlip.Exists()) { SuspectBlip.Alpha = 1f; }

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
                    if (VictimBlip.Exists()) { VictimBlip.Alpha = 1f; }

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