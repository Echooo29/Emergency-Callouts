using EmergencyCallouts.Essential;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using System;
using System.Reflection;
using static EmergencyCallouts.Essential.Color;
using static EmergencyCallouts.Essential.Helper;
using static EmergencyCallouts.Essential.Inventory;

namespace EmergencyCallouts.Callouts
{
    [CalloutInfo("Burglary", CalloutProbability.Medium)]
    public class Burglary : Callout
    {
        bool CalloutActive;
        bool PlayerArrived;
        bool PedFound;
        bool PedDetained;
        bool StopChecking;
        bool WithinRange;
        bool BarriersDeleted;
        bool VehicleUsed;
        bool DialogueStarted;
        bool FirstTime;
        bool CheckedForDamage;
        bool Damage;

        string DamageLine;
        string DamageLine2;

        Vector3 Entrance;
        Vector3 Center;
        Vector3 DamagedProperty;

        // Main
        #region Positions
        readonly Vector3[] CalloutPositions =
        {
            new Vector3(916.261f, -623.7192f, 58.052020f),  // Mirror Park
            new Vector3(-663.6192f, -1358.232f, 10.4971f),  // La Puerta
            new Vector3(1300.166f, -1719.278f, 54.04285f),  // El Burro
            new Vector3(2652.853f, 4308.485f, 44.393880f),  // Grapeseed
            new Vector3(1207.165f, 2694.605f, 37.823690f),  // Harmony
            new Vector3(194.8364f, 6576.915f, 31.820280f),  // Paleto Bay
        };
        #endregion

        // Mirror Park
        #region Positions
        readonly Vector3[] MirrorParkBreakInPositions =
        {
            new Vector3(880.1386f, -610.4592f, 58.44222f), // Backdoor
            new Vector3(905.5065f, -632.9874f, 58.04898f), // Shed 1
            new Vector3(869.7964f, -607.5421f, 58.21951f), // Shed 2
        };

        readonly float[] MirrorParkBreakInHeadings =
        {
            313.57f,
            212f,
            39.6f,
        };
        #endregion

        // La Puerta
        #region Positions
        readonly Vector3[] LaPuertaBreakInPositions =
        {
            new Vector3(-759.6483f, -1515.452f, 4.976925f), // Building Door
            new Vector3(-721.8896f, -1513.393f, 5.000525f), // Building Door 2
        };

        readonly float[] LaPuertaBreakInHeadings =
        {
            174.41f,
            104.47f,
        };
        #endregion

        // El Burro
        #region Positions
        readonly Vector3[] ElBurroBreakInPositions =
        {
            new Vector3(1283.446f, -1699.925f, 55.47572f), // Back Door
            new Vector3(1295.854f, -1697.502f, 55.07866f), // Shed
            new Vector3(1267.995f, -1713.858f, 54.65507f), // Building 2 Back Door
        };

        readonly float[] ElBurroBreakInHeadings =
        {
            175.15f,
            285.86f,
            313.77f,
        };
        #endregion

        // Grapeseed
        #region Positions
        readonly Vector3[] GrapeseedBreakInPositions =
        {
            new Vector3(2641.462f, 4235.202f, 45.49297f), // Small House
            new Vector3(2709.033f, 4316.569f, 46.15852f), // Small House 2
            new Vector3(2736.02f, 4279.527f, 48.49361f),  // Connected Trailers
        };

        readonly float[] GrapeseedBreakInHeadings =
        {
            51f,
            83f,
            283.19f,
        };
        #endregion

        // Harmony
        #region Positions
        readonly Vector3[] HarmonyBreakInPositions =
        {
            new Vector3(1194.485f, 2721.754f, 38.81226f), // Clothing Store
            new Vector3(1233.377f, 2737.641f, 38.0054f),  // RV Store
            new Vector3(1258.049f, 2740.197f, 38.70864f), // Abandoned Home
        };

        readonly float[] HarmonyBreakInHeadings =
        {
            209.57f,
            86.06f,
            342.01f,
        };
        #endregion

        // Paleto Bay
        #region Positions
        readonly Vector3[] PaletoBayBreakInPositions =
        {
            new Vector3(125.4187f, 6643.836f, 31.79918f), // Toilet Building
            new Vector3(174.3788f, 6642.977f, 31.57312f), // Don's Country Store
            new Vector3(156.7488f, 6657.068f, 31.56969f), // Pop's Pills
        };

        readonly float[] PaletoBayBreakInHeadings =
        {
            231.12f,
            138.59f,
            216.67f,
        };
        #endregion

        readonly Rage.Object Clipboard = new Rage.Object(new Model("p_amb_clipboard_01"), new Vector3(0, 0, 0));
        readonly Rage.Object Pencil = new Rage.Object(new Model("prop_pencil_01"), new Vector3(0, 0, 0));

        Vehicle SuspectVehicle;

        Ped Suspect;

        Persona SuspectPersona;

        Blip SuspectBlip;
        Blip EntranceBlip;
        Blip SearchArea;
        Blip DamagedPropertyBlip;

        float DamagedPropertyHeading;

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

            CalloutMessage = Localization.Burglary;
            CalloutDetails = Localization.BurglaryDetails;
            CalloutScenario = GetRandomScenarioNumber(5);

            Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT CRIME_BURGLARY IN_OR_ON_POSITION", CalloutPosition);

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
                if (CalloutPosition == CalloutPositions[0]) // Mirror Park
                {
                    Center = new Vector3(888.6841f, -625.1655f, 58.04898f);
                    Entrance = new Vector3(916.261f, -623.7192f, 58.05202f);
                }
                else if (CalloutPosition == CalloutPositions[1]) // La Puerta
                {
                    Center = new Vector3(-741.3954f, -1453.013f, 5.000523f);
                    Entrance = new Vector3(-663.6192f, -1358.232f, 10.49708f);
                }
                else if (CalloutPosition == CalloutPositions[2]) // El Burro
                {
                    Center = new Vector3(1281.405f, -1710.742f, 55.05928f);
                    Entrance = new Vector3(1300.166f, -1719.278f, 54.04285f);
                }
                else if (CalloutPosition == CalloutPositions[3]) // Grapeseed
                {
                    Center = new Vector3(2685.283f, 4256.731f, 45.41756f);
                    Entrance = new Vector3(2652.853f, 4308.485f, 44.39388f);
                }
                else if (CalloutPosition == CalloutPositions[4]) // Harmony
                {
                    Center = new Vector3(1223.067f, 2719.288f, 38.00484f);
                    Entrance = new Vector3(1207.165f, 2694.605f, 37.82369f);
                }
                else if (CalloutPosition == CalloutPositions[5]) // Paleto Bay
                {
                    Center = new Vector3(126.4832f, 6640.071f, 31.81017f);
                    Entrance = new Vector3(194.8364f, 6576.915f, 31.82028f);
                }
                #endregion

                // Callout Accepted
                Log.OnCalloutAccepted(CalloutMessage, CalloutScenario);

                // Accepting Messages
                Display.AcceptNotification(CalloutDetails);
                Display.AcceptSubtitle(CalloutMessage, CalloutArea);
                Display.OutdatedReminder();

                // EntranceBlip
                EntranceBlip = new Blip(Entrance);
                EntranceBlip.EnableRoute();

                // Suspect
                Suspect = new Ped(Helper.Entity.GetRandomMaleModel(), Vector3.Zero, 0f);
                SuspectPersona = Functions.GetPersonaForPed(Suspect);
                Suspect.IsPersistent = true;
                Suspect.BlockPermanentEvents = true;

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.SetColorRed();
                SuspectBlip.Scale = (float)Settings.PedBlipScale;
                SuspectBlip.Disable();

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

        private void RetrievePedPositions()
        {
            #region Positions
            if (CalloutPosition == CalloutPositions[0]) // Mirror Park
            {
                int num = random.Next(MirrorParkBreakInPositions.Length);
                Suspect.Position = MirrorParkBreakInPositions[num];
                Suspect.Heading = MirrorParkBreakInHeadings[num];
                DamagedProperty = MirrorParkBreakInPositions[num];
                DamagedPropertyHeading = MirrorParkBreakInHeadings[num];
            }
            else if (CalloutPosition == CalloutPositions[1]) // La Puerta
            {
                int num = random.Next(LaPuertaBreakInPositions.Length);
                Suspect.Position = LaPuertaBreakInPositions[num];
                Suspect.Heading = LaPuertaBreakInHeadings[num];
                DamagedProperty = LaPuertaBreakInPositions[num];
                DamagedPropertyHeading = LaPuertaBreakInHeadings[num];
            }
            else if (CalloutPosition == CalloutPositions[2]) // El Burro
            {
                int num = random.Next(ElBurroBreakInPositions.Length);
                Suspect.Position = ElBurroBreakInPositions[num];
                Suspect.Heading = ElBurroBreakInHeadings[num];
                DamagedProperty = ElBurroBreakInPositions[num];
                DamagedPropertyHeading = ElBurroBreakInHeadings[num];
            }
            else if (CalloutPosition == CalloutPositions[3]) // Grapeseed
            {
                int num = random.Next(GrapeseedBreakInPositions.Length);
                Suspect.Position = GrapeseedBreakInPositions[num];
                Suspect.Heading = GrapeseedBreakInHeadings[num];
                DamagedProperty = GrapeseedBreakInPositions[num];
                DamagedPropertyHeading = GrapeseedBreakInHeadings[num];
            }
            else if (CalloutPosition == CalloutPositions[4]) // Harmony
            {
                int num = random.Next(HarmonyBreakInPositions.Length);
                Suspect.Position = HarmonyBreakInPositions[num];
                Suspect.Heading = HarmonyBreakInHeadings[num];
                DamagedProperty = HarmonyBreakInPositions[num];
                DamagedPropertyHeading = HarmonyBreakInHeadings[num];
            }
            else if (CalloutPosition == CalloutPositions[5]) // Paleto Bay
            {
                int num = random.Next(PaletoBayBreakInPositions.Length);
                Suspect.Position = PaletoBayBreakInPositions[num];
                Suspect.Heading = PaletoBayBreakInHeadings[num];
                DamagedProperty = PaletoBayBreakInPositions[num];
                DamagedPropertyHeading = PaletoBayBreakInHeadings[num];
            }

            // Lockpick Animation
            Suspect.Tasks.PlayAnimation(new AnimationDictionary("mp_common_heist"), "pick_door", 5f, AnimationFlags.Loop);
            #endregion
        }

        private void RetrieveVehiclePositions()
        {
            #region Positions
            SuspectVehicle = new Vehicle(Vehicles.GetRandomVan(), CalloutPosition, 0f);
            SuspectVehicle.IsPersistent = true;

            if (CalloutPosition == CalloutPositions[0]) // Mirror Park
            {
                SuspectVehicle.Position = new Vector3(909.9557f, -624.8691f, 57.66842f);
                SuspectVehicle.Heading = 318.71f;
            }
            else if (CalloutPosition == CalloutPositions[1]) // La Puerta
            {
                SuspectVehicle.Position = new Vector3(-723.9453f, -1491.7f, 4.61949f);
                SuspectVehicle.Heading = 347.67f;
            }
            else if (CalloutPosition == CalloutPositions[2]) // El Burro
            {
                SuspectVehicle.Position = new Vector3(1308.245f, -1716.298f, 54.03547f);
                SuspectVehicle.Heading = 296.54f;
            }
            else if (CalloutPosition == CalloutPositions[3]) // Grapeseed
            {
                SuspectVehicle.Position = new Vector3(2716.37f, 4263.91f, 46.86611f);
                SuspectVehicle.Heading = 166.61f;
            }
            else if (CalloutPosition == CalloutPositions[4]) // Harmony
            {
                SuspectVehicle.Position = new Vector3(1234.011f, 2722.458f, 38.02638f);
                SuspectVehicle.Heading = 137.17f;
            }
            else if (CalloutPosition == CalloutPositions[5]) // Paleto Bay
            {
                SuspectVehicle.Position = new Vector3(130.7242f, 6666.58f, 31.65008f);
                SuspectVehicle.Heading = 158.69f;
            }

            VehicleDoor[] vehDoors = SuspectVehicle.GetDoors();
            vehDoors[2].Open(false);
            vehDoors[3].Open(false);
            #endregion
        }

        private void Dialogue()
        {
            #region Dialogue
            try
            {
                bool stopDialogue = false;

                if (Damage == true)
                {
                    DamageLine = "Anyway, you also left some dagage behind.";
                    DamageLine2 = "Bro that was already there when I came here!";
                }
                else
                {
                    DamageLine = "Luckily for you I didn't find any damage.";
                    DamageLine2 = "Nah man I'm a pro, I don't leave anything behind.";
                }

                string[] dialogue =
                {
                    "~b~You~s~: So, why did you do it?",
                    "~r~Suspect~s~: For the money...",
                    "~b~You~s~: So you don't have a job?",
                    "~r~Suspect~s~: Yeah, I don't.",
                    "~b~You~s~: " + DamageLine,
                    "~r~Suspect~s~: " + DamageLine2,
                    "~b~You~s~: You expect me to believe that?",
                    "~r~Suspect~s~: Nah, cops only hear what they want to hear.",
                    "~r~Suspect~s~: I'm staying silent until I can speak to my lawyer.",
                    "~b~You~s~: Not a problem.",
                    "~m~dialogue ended",
                };

                int line = 0;

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (Suspect.IsCuffed && Suspect.IsAlive && CheckedForDamage)
                        {
                            if (!DialogueStarted && !FirstTime)
                            {
                                GameFiber.Sleep(3000);
                                Game.DisplaySubtitle(Localization.InteractionDialogueSubtitlePromptSuspect, 10000);
                                FirstTime = true;
                            }

                            if (MainPlayer.Position.DistanceTo(Suspect.Position) <= 2f)
                            {
                                if (Game.IsKeyDown(Settings.InteractKey) && FirstTime)
                                {
                                    if (!DialogueStarted)
                                    {
                                        Suspect.Tasks.Clear();

                                        Game.LogTrivial("[Emergency Callouts]: Dialogue started with " + SuspectPersona.FullName);
                                    }

                                    DialogueStarted = true;

                                    Suspect.Tasks.AchieveHeading(MainPlayer.Heading - 180f);

                                    Game.DisplaySubtitle(dialogue[line], 15000);
                                    line++;
                                    Game.LogTrivial("[Emergency Callouts]: Displayed dialogue line " + line);

                                    if (line == dialogue.Length)
                                    {
                                        Game.LogTrivial("[Emergency Callouts]: Dialogue Ended");
                                        Handle.AdvancedEndingSequence();
                                        break;
                                    }

                                    GameFiber.Sleep(500);
                                }
                                else if (!DialogueStarted)
                                {
                                    Game.DisplayHelp($"{Localization.InteractionDialogueIntro} ~y~{Settings.InteractKey}~s~ {Localization.InteractionDialoguePromptSuspect}");
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

        private void CheckForDamage()
        {
            #region CheckForDamage
            try
            {
                string property;

                if (VehicleUsed)
                {
                    property = Localization.CheckForDamageVehicle;
                }
                else
                {
                    property = Localization.CheckForDamageDoor;
                }

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (Suspect.IsCuffed)
                        {
                            GameFiber.Sleep(7500);

                            Game.DisplaySubtitle($"{Localization.CheckForDamageSubtitleIntro} ~p~{property}~s~ {Localization.CheckForDamageSubtitleRest}", 10000);

                            DamagedPropertyBlip = new Blip(DamagedProperty);
                            DamagedPropertyBlip.SetColorPurple();
                            DamagedPropertyBlip.Scale = 0.6f;
                            DamagedPropertyBlip.Flash(500, -1);
                            break;
                        }
                    }

                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(DamagedProperty) <= 3f)
                        {
                            Game.DisplayHelp($"{Localization.InteractionDialogueIntro} ~y~{Settings.InteractKey}~s~ {Localization.CheckForDamagePrompt}");

                            if (Game.IsKeyDown(Settings.InteractKey))
                            {
                                // Play Animation
                                MainPlayer.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@business@bgen@bgen_inspecting@"), "inspecting_high_idle_02_inspector", -1, 2f, -1f, 0, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask | AnimationFlags.Loop);

                                // Attach Clipboard
                                int lhBoneIndex = NativeFunction.Natives.GET_PED_BONE_INDEX<int>(MainPlayer, (int)PedBoneId.LeftPhHand);
                                NativeFunction.Natives.ATTACH_ENTITY_TO_ENTITY(Clipboard, MainPlayer, lhBoneIndex, 0f, 0f, 0.009f, -90f, 0f, 0f, true, true, false, false, 2, 1);

                                // Attach Pencil
                                int rhBoneIndex = NativeFunction.Natives.GET_PED_BONE_INDEX<int>(MainPlayer, (int)PedBoneId.RightPhHand);
                                NativeFunction.Natives.ATTACH_ENTITY_TO_ENTITY(Pencil, MainPlayer, rhBoneIndex, 0f, 0f, 0f, 0f, 0f, 0f, true, true, false, false, 2, 1);

                                // Chance of damage
                                int chance = random.Next(0, 101);
                                if (chance <= Settings.ChanceOfPropertyDamage) // Damage
                                {
                                    GameFiber.Sleep(15000);
                                    Game.DisplayHelp($"{Localization.CheckForDamageFound} ~p~{property}~s~.");
                                    GameFiber.Sleep(3000);
                                    MainPlayer.Tasks.Clear();
                                    GameFiber.Sleep(1000);
                                    if (Clipboard.Exists()) { Clipboard.Delete(); }
                                    if (Pencil.Exists()) { Pencil.Delete(); }
                                    if (DamagedPropertyBlip.Exists()) { DamagedPropertyBlip.Delete(); }
                                    CheckedForDamage = true;
                                    Damage = true;
                                }
                                else // No Damage
                                {
                                    GameFiber.Sleep(15000);
                                    Game.DisplayHelp($"{Localization.CheckForDamageNotFound} ~p~{property}~s~.");
                                    GameFiber.Sleep(3000);
                                    MainPlayer.Tasks.Clear();
                                    GameFiber.Sleep(1000);
                                    if (Clipboard.Exists()) { Clipboard.Delete(); }
                                    if (Pencil.Exists()) { Pencil.Delete(); }
                                    if (DamagedPropertyBlip.Exists()) { DamagedPropertyBlip.Delete(); }
                                    CheckedForDamage = true;
                                    Damage = false;
                                }
                                break;
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

        private void Scenario1() // Attack
        {
            #region Scenario 1
            try
            {
                // Retrieve Ped Position
                RetrievePedPositions();

                CheckForDamage();
                Dialogue();

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 10f && Suspect.Exists() && PlayerArrived)
                        {
                            int num = random.Next(2);
                            if (num == 0)
                            {
                                Suspect.GiveRandomMeleeWeapon(-1, true);
                            }
                            else Suspect.GiveRandomHandgun(-1, true);

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
        
        private void Scenario2() // Pursuit
        {
            #region Scenario 2
            try
            {
                // Retrieve Ped Positions
                RetrievePedPositions();

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 10f && Suspect.Exists() && PlayerArrived)
                        {
                            StopChecking = true;

                            // Delete Blips
                            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                            if (SearchArea.Exists()) { SearchArea.Delete(); }
                            if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

                            // Create Pursuit
                            LHandle pursuit = Functions.CreatePursuit();

                            // Add Suspect To Pursuit
                            Functions.AddPedToPursuit(pursuit, Suspect);

                            // Set Pursuit Active
                            Functions.SetPursuitIsActiveForPlayer(pursuit, true);

                            // Play Pursuit Audio
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

        private void Scenario3() // Surrender
        {
            #region Scenario 3
            try
            {
                // Retrieve Ped Positions
                RetrievePedPositions();

                CheckForDamage();
                Dialogue();

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();
                        
                        if (MainPlayer.Position.DistanceTo(Suspect.Position) <= 10f && Suspect.Exists() && PlayerArrived)
                        {
                            // Clipping Through Wall Fix
                            Suspect.Tasks.ClearImmediately();
                            Suspect.Tasks.GoStraightToPosition(MainPlayer.Position, 1f, DamagedPropertyHeading - 180, 0f, 30);
                            GameFiber.Sleep(30);

                            // Put Suspect's Hands up
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

        private void Scenario4() // Attack
        {
            #region Scenario 4
            try
            {
                // Retrieve Ped Positions
                RetrievePedPositions();

                CheckForDamage();
                Dialogue();

                // Give Weapon
                Suspect.Inventory.GiveNewWeapon("WEAPON_CROWBAR", -1, true);

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 10f && Suspect.Exists() && PlayerArrived)
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

        private void Scenario5() // Putting Items In Vehicle
        {
            #region Scenario 5
            try
            {
                // Retrieve Vehicle Positions
                RetrieveVehiclePositions();

                // Set VehicleUsed
                VehicleUsed = true;

                // Suspect Resistance Chance
                Functions.SetPedResistanceChance(Suspect, 40f);

                // Suspect Position
                Suspect.Position = SuspectVehicle.GetOffsetPositionFront(-SuspectVehicle.Length + 1.9f);

                // Suspect Heading
                Suspect.Heading = SuspectVehicle.Heading;

                // Search Animation
                Suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@gangops@facility@servers@bodysearch@"), "player_search", 5f, AnimationFlags.UpperBodyOnly | AnimationFlags.Loop);

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) < 15f && Suspect.Exists() && PlayerArrived)
                        {
                            StopChecking = true;

                            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                            if (SearchArea.Exists()) { SearchArea.Delete(); }
                            if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

                            // Create Pursuit
                            LHandle pursuit = Functions.CreatePursuit();

                            // Add Suspect to pursuit
                            Functions.AddPedToPursuit(pursuit, Suspect);

                            // Set Pursuit Active
                            Functions.SetPursuitIsActiveForPlayer(pursuit, true);

                            // Play pursuit audio
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

        public override void Process() 
        {
            base.Process();
            try
            {
                Handle.ManualEnding();
                Handle.SpookCheck(Entrance, 10f);
                Handle.PreventPickupCrash(Suspect);

                #region WithinRange
                if (MainPlayer.Position.DistanceTo(CalloutPosition) <= 200f && !WithinRange)
                {
                    // Set WithinRange
                    WithinRange = true;

                    // Delete Nearby Peds
                    Handle.DeleteNearbyPeds(Suspect, 40f);

                    // Delete Nearby Trailers
                    Handle.DeleteNearbyTrailers(Center);

                    // Delete Barriers
                    //if (!BarriersDeleted && CalloutPosition == CalloutPositions[0]!!)
                    //{
                    //    foreach (Rage.Object obj in World.GetAllObjects())
                    //    {
                    //        if (obj.Position.DistanceTo(new Vector3(-708.7535f, -1382.413f, 5.281574f)) <= 15f && (obj.Model.Name == "0x18396ad2" || obj.Model.Name == "0x0e76574c"))
                    //        {
                    //            if (obj.Exists()) { obj.Delete(); }
                    //            GameFiber.Sleep(500);
                    //            BarriersDeleted = true;
                    //        }
                    //    }
                    //}
                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} is within 200 meters");
                }
                #endregion

                #region PlayerArrived
                if (MainPlayer.Position.DistanceTo(Entrance) < 15f && !PlayerArrived)
                {
                    // Set PlayerArrived
                    PlayerArrived = true;

                    // Display Arriving Subtitle
                    Game.DisplaySubtitle(Localization.BurglarySubtitle, 10000);

                    // Disable route
                    EntranceBlip.DisableRoute();

                    // Delete EntranceBlip
                    if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

                    // Create SearchArea
                    SearchArea = new Blip(Suspect.Position.Around2D(5f, 20f), Settings.SearchAreaSize);
                    SearchArea.SetColorYellow();
                    SearchArea.Alpha = 0.5f;

                    // SpookCheck
                    Handle.SpookCheck(Entrance, 10f);
                    
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
            if (DamagedPropertyBlip.Exists()) { DamagedPropertyBlip.Delete(); }
            if (Clipboard.Exists()) { Clipboard.Delete(); }
            if (Pencil.Exists()) { Pencil.Delete(); }

            Display.HideSubtitle();
            Display.EndNotification();
            Log.OnCalloutEnded(CalloutMessage, CalloutScenario);
        }
    }
}