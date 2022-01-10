using System;
using System.Reflection;
using System.Windows.Forms;
using Rage;
using LSPD_First_Response.Mod.API;
using Rage.Native;
using static EmergencyCallouts.Essential.Helper;
using System.Net;
using LSPD_First_Response.Engine.Scripting.Entities;
using System.Linq;
using System.IO;

namespace EmergencyCallouts.Essential
{
    internal static class Project
    {
        internal static string Name => Assembly.GetExecutingAssembly().GetName().Name;

        internal static string LocalVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString().Substring(0, 5);

        internal static DateTime DateCreated => File.GetCreationTime("Emergency Callouts.dll");

        internal static string SettingsPath => "Plugins/LSPDFR/Emergency Callouts/Settings.ini";

        internal static string LocalizationPath => "Plugins/LSPDFR/Emergency Callouts/Localization.ini";
    }


    internal static class Helper
    {
        internal static Ped MainPlayer => Game.LocalPlayer.Character;
        internal static Persona PlayerPersona = Functions.GetPersonaForPed(MainPlayer);
        internal static Random random = new Random();

        internal static bool PUBRemoteState;
        internal static bool TRERemoteState;
        internal static bool BURRemoteState;
        internal static bool DOMRemoteState;
        internal static bool SUSRemoteState;

        internal static string CalloutDetails { get; set; }
        internal static string CalloutArea { get; set; }
        internal static int CalloutScenario { get; set; }

        #region GetRandomScenarioNumber
        internal static int GetRandomScenarioNumber(int totalScenarios)
        {
            return random.Next(1, totalScenarios + 1);

        }
        #endregion

        #region Enumerations
        internal enum PedCategory
        {
            Suspect,
            Suspect2,
            Victim,
            Bystander,
            Guard,
            Officer,
            Paramedic,
            Firefighter,
        }
        #endregion

        internal static class Entity
        {
            #region GetRandomMaleModel
            internal static string GetRandomMaleModel()
            {
                string[] maleModels =
                {
                    "a_m_m_afriamer_01", "ig_barry", "u_m_y_baygor", "a_m_o_beach_01", "a_m_y_beach_01",
                    "u_m_m_aldinapoli", "a_m_y_beach_02", "a_m_y_beachvesp_01", "a_m_y_beachvesp_02", "ig_benny",
                    "s_m_y_ammucity_01", "ig_beverly", "a_m_y_bevhills_01", "a_m_m_bevhills_02", "a_m_y_bevhills_02",
                    "cs_carbuyer", "s_m_o_busker_01", "ig_car3guy1", "g_m_m_chigoon_01", "ig_jimmyboston",
                    "u_m_y_antonb", "g_m_m_chigoon_02", "csb_chin_goon", "u_m_y_chip", "ig_claypain",
                    "g_m_m_armboss_01", "s_m_m_cntrybar_01", "csb_customer", "a_m_y_cyclist_01", "ig_dale",
                    "g_m_m_armgoon_01", "ig_davenorton", "s_m_y_dealer_01", "ig_devin", "ig_dom",
                    "g_m_y_armgoon_02", "s_m_y_doorman_01", "a_m_y_downtown_01", "ig_drfriedlander", "a_m_m_eastsa_01",
                    "g_m_m_armlieut_01", "a_m_m_eastsa_02", "a_m_y_eastsa_02", "u_m_m_edtoh", "ig_fabien",
                    "s_m_m_autoshop_01", "g_m_y_famca_01", "g_m_y_famdnf_01", "g_m_y_famfor_01", "a_m_m_farmer_01",
                    "ig_money", "a_m_m_fatlatin_01", "ig_lazlow", "s_m_m_hairdress_01", "a_m_o_ktown_01",
                    "g_m_y_azteca_01", "u_m_o_finguru_01", "csb_fos_rep", "player_one", "ig_g",
                    "g_m_y_ballaeast_01", "csb_g", "a_m_m_genfat_01", "a_m_m_genfat_02", "a_m_y_genstreet_01",
                    "g_m_y_ballaorig_01", "a_m_y_genstreet_02", "u_m_m_glenstank_01", "a_m_m_golfer_01", 
                    "ig_ballasog", "s_m_m_highsec_02", "a_m_y_hipster_02", "csb_hugh", "a_m_m_indian_01",
                    "g_m_y_ballasout_01", "ig_jay_norris", "u_m_m_jesus_01", "u_m_m_jewelsec_01", 
                    "u_m_m_bankman", "u_m_m_jewelthief", "ig_josh", "ig_joeminuteman", "ig_jimmydisanto",
                    "ig_bankman", "cs_johnnyklebitz", "g_m_y_korean_01", "g_m_y_korlieut_01", 
                    "s_m_y_barman_01", "a_m_y_ktown_02", "ig_lamardavis", "a_m_y_latino_01", 
                };

                int num = random.Next(maleModels.Length);

                return maleModels[num];
            }
            #endregion

            #region GetRandomFemaleModel
            internal static string GetRandomFemaleModel()
            {
                string[] femaleModels =
                {
                    "ig_abigail", "ig_amandatownley", "csb_anita", "ig_ashley", "s_f_y_bartender_01",
                    "a_f_m_bevhills_01", "a_f_m_bevhills_02", "a_f_y_bevhills_03", "a_f_y_bevhills_04", "mp_f_boatstaff_01",
                    "a_f_y_business_02", "a_f_m_business_02", "a_f_y_business_01", "a_f_y_business_04", "u_f_y_comjane",
                    "ig_denise", "csb_denise_friend", "cs_debra", "a_f_m_eastsa_01", "a_f_y_eastsa_02",
                    "a_f_y_epsilon_01", "a_f_m_fatwhite_01", "s_f_m_fembarber", "a_f_y_fitness_01", "a_f_y_fitness_02",
                    "a_f_y_genhot_01", "a_f_o_genstreet_01", "a_f_y_golfer_01", "cs_guadalope", "a_f_y_hipster_02",
                    "a_f_y_hipster_03", "a_f_y_hipster_04", "a_f_o_indian_01", "ig_janet", "u_f_y_jewelass_01",
                    "ig_jewelass", "ig_kerrymcintosh", "a_f_o_ktown_01", "a_f_m_ktown_02", "ig_magenta",
                    "ig_maryann", "u_f_y_mistress", "ig_molly", "ig_mrsphillips",
                    "ig_mrs_thornhill", "ig_natalia", "ig_paige", "ig_patricia", "u_f_y_princess",
                    "a_f_m_salton_01", "a_f_o_salton_01", "a_f_y_rurmeth_01", "a_f_y_runner_01", "a_f_y_scdressy_01",
                    "ig_screen_writer", "s_f_m_shop_high", "s_f_y_shop_low", "s_f_y_shop_mid", "a_f_y_skater_01",
                    "a_f_o_soucent_01", "a_f_y_soucent_01", "a_f_m_soucent_02", "a_f_o_soucent_02", "a_f_y_soucent_02",
                    "a_f_y_soucent_03", "a_f_m_soucentmc_01", "u_f_y_spyactress", "s_f_m_sweatshop_01", "s_f_y_sweatshop_01",
                    "ig_tanisha", "a_f_y_tennis_01", "ig_tonya", "a_f_y_tourist_01", "a_f_y_tourist_02",
                    "g_f_y_vagos_01", "a_f_y_vinewood_01", "a_f_y_vinewood_02", "a_f_y_vinewood_03",
                    "a_f_y_vinewood_04", "a_f_y_yoga_01", "a_f_y_femaleagent", "mp_f_chbar_01",
                    "mp_f_counterfeit_01", "mp_f_execpa_01", "mp_f_execpa_02", "ig_jackie", "ig_isldj_04_d_01",
                    "s_f_y_beachbarstaff_01", "ig_patricia_02"
                };

                int num = random.Next(femaleModels.Length);

                return femaleModels[num];

            }
            #endregion
        }

        internal static class Display
        {
            #region AcceptNotification
            internal static void AcceptNotification(string details)
            {
                Game.DisplayNotification("dia_police", "dia_police", Localization.DispatchName, Localization.AcceptNotificationSubtitle, details);
            }
            #endregion

            #region AcceptSubtitle
            internal static void AcceptSubtitle(string calloutMessage, string calloutArea)
            {
                Game.DisplaySubtitle($"{Localization.AcceptSubtitleIntro} ~r~{calloutMessage}~s~ {Localization.AcceptSubtitleAt} ~y~{calloutArea}~s~.", 10000);
            }
            #endregion

            #region OutdatedReminder
            internal static void OutdatedReminder()
            {
                if (UpdateChecker.OnlineVersion != Project.LocalVersion && !Settings.EarlyAccess)
                {
                    Game.DisplayNotification("commonmenu", "mp_alerttriangle", "Emergency Callouts", $"~r~v{Project.LocalVersion} ~c~by Faya", $"Found update ~g~v{UpdateChecker.OnlineVersion} ~s~available for you!");
                }
            }
            #endregion

            #region EndNotification
            internal static void EndNotification()
            {
                Game.DisplayNotification("dia_police", "dia_police", Localization.DispatchName, Localization.EndNotificationSubtitle, Localization.EndNotificationText);
            }
            #endregion

            #region HideSubtitle
            internal static void HideSubtitle()
            {
                Game.DisplaySubtitle(string.Empty);
            }
            #endregion
        }

        internal class Log
        {
            #region OnCalloutAccepted
            internal static void OnCalloutAccepted(string CalloutMessage, int ScenarioNumber)
            {
                Game.LogTrivial($"[Emergency Callouts]: Created callout ({CalloutMessage}, Scenario {ScenarioNumber})");
            }
            #endregion

            #region OnCalloutEnded
            internal static void OnCalloutEnded(string CalloutMessage, int ScenarioNumber)
            {
                Game.LogTrivial($"[Emergency Callouts]: Ended callout ({CalloutMessage}, Scenario {ScenarioNumber})");
            }
            #endregion

            #region Exception
            internal static void Exception(Exception e, string _class, string method)
            {
                // Log Exception
                Game.LogTrivial($"[Emergency Callouts v{Project.LocalVersion}]: {e.Message} At {_class}.{method}()");

                // Refer to bug report form
                Game.DisplayNotification("commonmenu", "mp_alerttriangle", "Emergency Callouts", "~r~Issue detected!", "Please fill in a ~g~bug report form~s~.\nThat can be found on the ~y~Emergency Callouts Page~s~.");
                
                if (!Settings.EarlyAccess)
                {
                    try
                    {
                        // Send hit to remote exception counter
                        WebClient hitUpdater = new WebClient();
                        hitUpdater.DownloadString("https://pastebin.com/raw/Li5KFks3");
                        Game.LogTrivial("[Emergency Callouts]: Sent hit to the remote exception counter");
                    }
                    catch (WebException webEx)
                    {
                        Game.LogTrivial("[Emergency Callouts]: v" + webEx.Message);
                    }
                }
            }
            #endregion

            #region Creation
            internal static void Creation(Ped ped, Enum pedCategory)
            {
                Game.LogTrivial($"[Emergency Callouts]: Created {pedCategory} ({ped.Model.Name}) at {ped.Position}");
            }
            internal static void Creation(Vehicle vehicle, Enum pedCategory)
            {
                Game.LogTrivial($"[Emergency Callouts]: Created {pedCategory}Vehicle ({vehicle.Model.Name}) at {vehicle.Position}");
            }
            #endregion
        }

        internal static class Play
        {
            #region PursuitAudio
            internal static void PursuitAudio()
            {
                Functions.PlayScannerAudio("OFFICERS_REPORT CRIME_RESIST_ARREST");
            }
            #endregion

            #region CodeFourAudio
            internal static void CodeFourAudio()
            {
                Functions.PlayScannerAudio("ACKNOWLEDGE CODE_FOUR NO_UNITS_REQUIRED");
            }
            #endregion
        }

        internal static class Vehicles
        {
            #region GetRandomFourDoor
            internal static string GetRandomFourDoor()
            {
                string[] vehicles = 
                {  
                    "BISON", "BISON2", "BALLER", "BALLER2", "BALLER3", "BALLER4", "CAVALCADE", "CAVALCADE2", "CONTENDER", "DUBSTA", 
                    "GRESLEY", "HABANERO", "BJXL", "CAVALCADE", "HUNTLEY", "LANDSTALKER", "LANDSTALKER2", "MESA", "PRIMO", "FQ2",
                    "EMPEROR", "FUGITIVE", "INTRUDER", "PREMIER", "SURGE", "TAILGATER", "TAILGATER2", "EMPEROR2", "GLENDALE",  
                    "WARRENER", "DUKES", "VIRGO", "BUFFALO", "BUFFALO2", "ASEA", "RANCHERXL", "SULTAN", "DILETTANTE",
                    "SULTAN", "ASTEROPE", "WASHINGTON", "XLS", "REBLA", 
                };

                int num = random.Next(vehicles.Length);
                return vehicles[num];
            }
            #endregion

            #region GetRandomMotorcycle
            internal static string GetRandomMotorcycle()
            {
                string[] motorcycles =
                {
                    "AKUMA", "AVARUS", "BAGGER", "BATI", "BATI2", "BF400", "CARBONRS", "CHIMERA", "CLIFFHANGER", "DAEMON", "DAEMON2", "DIABLOUS",
                    "DIABLOUS2", "DOUBLE", "ENDURO", "ESSKEY", "FCR", "FCR2", "MANCHEZ", "NEMESIS", "PCJ", "RATBIKE", "RUFFIAN", "SANCHEZ", 
                    "SANCTUS", "SOVEREIGN", "THRUST", "VADER", "VINDICATOR", "WOLFSBANE", "ZOMBIEA", "ZOMBIEB", "SANCHEZ2", "DEFILER"
                };

                int num = random.Next(motorcycles.Length);
                return motorcycles[num];
            }
            #endregion

            #region GetRandomVan
            internal static string GetRandomVan()
            {
                string[] vans = { "SPEEDO", "BURRITO", "RUMPO", "RUMPO2", "RUMPO3", "BURRITO2", "BURRITO3", "BURRITO4", "PONY2", "SPEEDO4", "YOUGA" };

                int num = random.Next(vans.Length);
                return vans[num];
            }
            #endregion
        }

        internal static class Handle
        {
            #region ManualEnding
            internal static void ManualEnding()
            {
                if (Game.IsKeyDown(Keys.End))
                {
                    AdvancedEndingSequence();
                }
            }
            #endregion

            #region AutomaticEnding
            internal static void AutomaticEnding(Ped suspect)
            {
                if (suspect.Exists())
                {
                    if (suspect.IsCuffed || suspect.IsDead)
                    {
                        GameFiber.Sleep(4000);
                        AdvancedEndingSequence();
                    }
                }

                if (MainPlayer.IsDead) { Functions.StopCurrentCallout(); }
            }
            internal static void AutomaticEnding(Ped suspect, Ped suspect2)
            {
                if (suspect.Exists())
                {
                    if (suspect.IsCuffed && suspect2.IsCuffed)
                    {
                        GameFiber.Sleep(4000);
                        AdvancedEndingSequence();
                    }
                    else if (suspect.IsDead && suspect2.IsDead)
                    {
                        GameFiber.Sleep(4000);
                        AdvancedEndingSequence();
                    }
                    else if (suspect.IsDead && suspect2.IsCuffed)
                    {
                        GameFiber.Sleep(4000);
                        AdvancedEndingSequence();
                    }
                    else if (suspect.IsCuffed && suspect2.IsDead)
                    {
                        GameFiber.Sleep(4000);
                        AdvancedEndingSequence();
                    }
                }
            }

            internal static void AutomaticEndingVictim(Ped suspect, Ped victim)
            {
                if (suspect.Exists())
                {
                    if (suspect.IsDead && victim.IsDead && MainPlayer.IsInAnyPoliceVehicle)
                    {
                        AdvancedEndingSequence();
                    }
                    else if (suspect.IsCuffed && victim.IsDead)
                    {
                        AdvancedEndingSequence();
                    }
                    else if (suspect.IsCuffed && victim.IsCuffed)
                    {
                        AdvancedEndingSequence();
                    }
                }
            }
            #endregion

            #region AdvancedEndingSequence
            internal static void AdvancedEndingSequence()
            {
                if (MainPlayer.IsOnFoot && !MainPlayer.IsAiming)
                {
                    LSPD_First_Response.Mod.Menus.EPoliceRadioAction customRadioAction = Functions.GetPlayerRadioAction();
                    Functions.PlayPlayerRadioAction(customRadioAction, 3000);
                }

                GameFiber.Sleep(700);
                Game.DisplayNotification(Localization.EndNotificationTransmit);
                GameFiber.Sleep(2700);
                Play.CodeFourAudio();
                GameFiber.Sleep(5000);
                Functions.StopCurrentCallout();
            }
            #endregion

            #region SpookCheck
            internal static void SpookCheck(Vector3 entrance, float distanceFromEntrance)
            {
                if (Settings.EndOnArrivalWithLights && MainPlayer.Position.DistanceTo(entrance) <= distanceFromEntrance && MainPlayer.CurrentVehicle.IsSirenOn && MainPlayer.IsInAnyPoliceVehicle)
                {
                    int chance = random.Next(0, 101);

                    if (chance <= Settings.EndChance)
                    {
                        Game.DisplayHelp(Localization.EndNotificationAlertedPed, 5000);
                        Display.HideSubtitle();
                        GameFiber.Sleep(5000);
                        Functions.StopCurrentCallout();
                        Game.LogTrivial("[Emergency Callouts]: Scared ped, ended callout");
                    }
                }
            }
            #endregion

            #region BlockPermanentEventsRadius
            internal static void BlockPermanentEventsRadius(Vector3 location, float radius)
            {
                foreach (Ped ped in World.GetAllPeds()) // Maybe gang members only?
                {
                    if (ped.Exists() && ped.Position.DistanceTo(location) < radius)
                    {
                        ped.BlockPermanentEvents = true;
                    }
                }
            }
            #endregion

            #region DeleteNearbyPeds
            internal static void DeleteNearbyPeds(Ped mainPed, float radius)
            {
                // Delete Nearby Peds
                foreach (Ped ped in World.GetAllPeds())
                {
                    if (ped && ped.Position.DistanceTo(mainPed) <= radius && ped != mainPed && ped != MainPlayer)
                    {
                        ped.Delete();
                    }
                }
            }
            internal static void DeleteNearbyPeds(Ped mainPed, Ped mainPed2, float radius)
            {
                // Delete Nearby Peds
                foreach (Ped ped in World.GetAllPeds())
                {
                    if (ped && ped.Position.DistanceTo(mainPed) <= radius && ped != mainPed && ped != mainPed2 && ped != MainPlayer)
                    {
                        ped.Delete();
                    }
                }
            }
            #endregion

            #region DeleteNearbyTrailers
            internal static void DeleteNearbyTrailers(Vector3 position)
            {
                string[] trailerList = 
                { 
                    "ARMYTRAILER", "ARMYTRAILER2", "BALETRAILER", "BOATTRAILER", "DOCKTRAILER", "FREIGHTTRAILER", "GRAINTRAILER", "TRAILERLARGE", "TVTRAILER",
                    "PROPTRAILER", "RAKETRAILER", "BOATTRAILER", "TRAILERLOGS", "TRAILERS", "TRAILERS2", "TRAILERS3", "TRAILERS4", "TRAILERSMALL", "TRAILERSMALL2"
                };

                foreach (Vehicle veh in World.GetAllVehicles())
                {
                    if (veh.Position.DistanceTo(position) <= 60f && trailerList.Contains(veh.Model.Name))
                    {
                        if (veh.Exists()) { veh.Delete(); }
                    }
                }

                
            }
            #endregion

            #region PreventDistanceCrash
            internal static void PreventDistanceCrash(Vector3 CalloutPosition, bool PlayerArrived, bool PedFound)
            {
                if (MainPlayer.Position.DistanceTo(CalloutPosition) > 400f && PlayerArrived == true && PedFound == true)
                {
                    Game.LogTrivial("[Emergency Callouts]: Too far from callout position, ending callout to prevent crash");
                    Functions.StopCurrentCallout();
                    Play.CodeFourAudio();
                }
            }
            #endregion

            #region PreventPickupCrash
            internal static void PreventPickupCrash(Ped ped)
            {
                foreach (Vehicle vehicle in World.GetAllVehicles())
                {
                    if (!ped.IsCollisionEnabled && ped.Position.DistanceTo(vehicle.GetOffsetPositionFront(-vehicle.Length + 1f)) <= 2f)
                    {
                        if (vehicle.Model.Name == "AMBULANCE")
                        {
                            Play.CodeFourAudio();
                            Functions.StopCurrentCallout();
                        }
                    }
                }

                foreach (Ped retrievedPed in World.GetAllPeds())
                {
                    if (retrievedPed.Model.Name.ToLower() == "s_m_m_doctor_01" && ped.Position.DistanceTo(retrievedPed.Position) <= 5f && retrievedPed.IsDead)
                    {
                        Play.CodeFourAudio();
                        Functions.StopCurrentCallout();
                    }
                }
            }

            internal static void PreventPickupCrash(Ped ped, Ped ped2)
            {
                foreach (Vehicle vehicle in World.GetAllVehicles())
                {
                    if ((!ped.IsCollisionEnabled && ped.Position.DistanceTo(vehicle.GetOffsetPositionFront(-vehicle.Length + 1f)) <= 2f) || !ped2.IsCollisionEnabled && ped2.Position.DistanceTo(vehicle.GetOffsetPositionFront(-vehicle.Length + 1f)) <= 2f)
                    {
                        if (vehicle.Model.Name == "AMBULANCE")
                        {
                            Play.CodeFourAudio();
                            Functions.StopCurrentCallout();
                        }
                    }
                }

                foreach (Ped retrievedPed in World.GetAllPeds())
                {
                    if (ped.IsDead || ped2.IsDead)
                    {
                        if (retrievedPed.Model.Name.ToLower() == "s_m_m_doctor_01" && (ped.Position.DistanceTo(retrievedPed.Position) <= 5f || ped2.Position.DistanceTo(retrievedPed.Position) <= 5f))
                        {
                            Play.CodeFourAudio();
                            Functions.StopCurrentCallout();
                        }
                    }
                }
            }
            #endregion

            #region RemoteStates
            internal static void RemoteStates()
            {
                try
                {
                    WebClient client = new WebClient();

                    string states = client.DownloadString("https://pastebin.com/raw/hS4WP9w6");

                    Game.LogTrivial("[Emergency Callouts]: Downloaded remote callout states");

                    // Public Intoxication
                    #region StateCheck
                    if (states.Contains("PUB"))
                    {
                        PUBRemoteState = true;
                        Game.LogTrivial("[Emergency Callouts]: PUBRemoteState = true");
                    }
                    else
                    {
                        PUBRemoteState = false;
                        Game.LogTrivial("[Emergency Callouts]: PUBRemoteState = false");
                        Game.DisplayNotification("commonmenu", "mp_alerttriangle", "Emergency Callouts", "~b~Public Intoxication", "has been ~y~remotely disabled~s~.\nThis was ~g~intentionally done~s~ by the developer.");
                    }
                    #endregion

                    // Burglary
                    #region StateCheck
                    if (states.Contains("BUR"))
                    {
                        BURRemoteState = true;
                        Game.LogTrivial("[Emergency Callouts]: BURRemoteState = true");
                    }
                    else
                    {
                        BURRemoteState = false;
                        Game.LogTrivial("[Emergency Callouts]: BURRemoteState = false");
                        Game.DisplayNotification("commonmenu", "mp_alerttriangle", "Emergency Callouts", "~b~Burglary In Progress", "has been ~y~remotely disabled~s~.\nThis was ~g~intentionally done~s~ by the developer.");
                    }
                    #endregion

                    // Trespassing
                    #region StateCheck
                    if (states.Contains("TRE"))
                    {
                        TRERemoteState = true;
                        Game.LogTrivial("[Emergency Callouts]: TRERemoteState = true");
                    }
                    else
                    {
                        TRERemoteState = false;
                        Game.LogTrivial("[Emergency Callouts]: TRERemoteState = false");
                        Game.DisplayNotification("commonmenu", "mp_alerttriangle", "Emergency Callouts", "~b~Trespassing", "has been ~y~remotely disabled~s~.\nThis was ~g~intentionally done~s~ by the developer.");
                    }
                    #endregion

                    // Domestic Violence
                    #region StateCheck
                    if (states.Contains("DOM"))
                    {
                        DOMRemoteState = true;
                        Game.LogTrivial("[Emergency Callouts]: DOMRemoteState = true");
                    }
                    else
                    {
                        DOMRemoteState = false;
                        Game.LogTrivial("[Emergency Callouts]: DOMRemoteState = false");
                        Game.DisplayNotification("commonmenu", "mp_alerttriangle", "Emergency Callouts", "~b~Domestic Violence", "has been ~y~remotely disabled~s~.\nThis was ~g~intentionally done~s~ by the developer.");
                    }
                    #endregion

                    // Suspicious Activity
                    #region StateCheck
                    if (states.Contains("SUS"))
                    {
                        SUSRemoteState = true;
                        Game.LogTrivial("[Emergency Callouts]: SUSRemoteState = true");
                    }
                    else
                    {
                        SUSRemoteState = false;
                        Game.LogTrivial("[Emergency Callouts]: SUSRemoteState = false");
                        Game.DisplayNotification("commonmenu", "mp_alerttriangle", "Emergency Callouts", "~b~Suspicious Activity", "has been ~y~remotely disabled~s~.\nThis was ~g~intentionally done~s~ by the developer.");
                    }
                    #endregion
                }
                catch (WebException)
                {
                    Game.LogTrivial("[Emergency Callouts]: Could not check online callout states, callout states are set to true");

                    PUBRemoteState = true;
                    BURRemoteState = true;
                    TRERemoteState = true;
                    DOMRemoteState = true;
                    SUSRemoteState = true;
                }
            }
            #endregion
        }
    }

    internal static class Color
    {
        internal static void SetColorRed(this Blip blip) => blip.Color = System.Drawing.Color.FromArgb(224, 50, 50);

        internal static void SetColorYellow(this Blip blip) => blip.Color = System.Drawing.Color.FromArgb(240, 200, 80);

        internal static void SetColorBlue(this Blip blip) => blip.Color = System.Drawing.Color.FromArgb(93, 182, 229);

        internal static void SetColorOrange(this Blip blip) => blip.Color = System.Drawing.Color.FromArgb(234, 142, 80);

        internal static void SetColorGreen(this Blip blip) => blip.Color = System.Drawing.Color.FromArgb(114, 204, 114);

        internal static void SetColorPurple(this Blip blip) => blip.Color = System.Drawing.Color.FromArgb(171, 60, 230);
    }

    internal static class Inventory
    {
        #region Melee
        internal static void GiveRandomMeleeWeapon(this Ped ped, short ammoCount, bool equipNow)
        {
            string[] meleeWeapons =
            {
                "WEAPON_DAGGER",
                "WEAPON_BAT",
                "WEAPON_BOTTLE",
                "WEAPON_CROWBAR",
                "WEAPON_UNARMED",
                "WEAPON_FLASHLIGHT",
                "WEAPON_HAMMER",
                "WEAPON_HATCHET",
                "WEAPON_KNUCKLE",
                "WEAPON_KNIFE",
                "WEAPON_MACHETE",
                "WEAPON_SWITCHBLADE",
                "WEAPON_NIGHTSTICK",
                "WEAPON_WRENCH",
                "WEAPON_BATTLEAXE",
            };

            int num = random.Next(meleeWeapons.Length);
            if (ped.Exists()) { ped.Inventory.GiveNewWeapon(meleeWeapons[num], ammoCount, equipNow); }
        }
        #endregion

        #region Handgun
        internal static void GiveRandomHandgun(this Ped ped, short ammoCount, bool equipNow)
        {
            string[] handguns =
            {
                "WEAPON_PISTOL",
                "WEAPON_COMBATPISTOL",
                "WEAPON_PISTOL50",
                "WEAPON_VINTAGEPISTOL",
                "WEAPON_SNSPISTOL",
                "WEAPON_HEAVYPISTOL",
                "WEAPON_CERAMICPISTOL",
            };

            int num = random.Next(handguns.Length);
            if (ped.Exists()) { ped.Inventory.GiveNewWeapon(handguns[num], ammoCount, equipNow); }
        }
        #endregion

        #region Submachine Gun
        internal static void GiveRandomSubmachineGun(this Ped ped, short ammoCount, bool equipNow)
        {
            string[] submachineGuns =
            {
                "WEAPON_MICROSMG",
                "WEAPON_SMG",
                "WEAPON_ASSAULTSMG",
                "WEAPON_MINISMG"
            };

            int num = random.Next(submachineGuns.Length);
            if (ped.Exists()) { ped.Inventory.GiveNewWeapon(submachineGuns[num], ammoCount, equipNow); }
        }
        #endregion

        #region Assault Rifle
        internal static void GiveRandomAssaultRifle(this Ped ped, short ammoCount, bool equipNow)
        {
            string[] rifles =
            {
                "WEAPON_CARBINERIFLE",
                "WEAPON_ASSAULTRIFLE",
                "WEAPON_ADVANCEDRIFLE",
                "WEAPON_SPECIALCARBINE",
                "WEAPON_BULLPUPRIFLE",
            };

            int num = random.Next(rifles.Length);
            if (ped.Exists()) { ped.Inventory.GiveNewWeapon(rifles[num], ammoCount, equipNow); }
        }
        #endregion

        #region Shotgun
        internal static void GiveRandomShotgun(this Ped ped, short ammoCount, bool equipNow)
        {
            string[] shotguns =
            {
                "WEAPON_PUMPSHOTGUN",
                "WEAPON_SAWNOFFSHOTGUN",
                "WEAPON_BULLPUPSHOTGUN",
                "WEAPON_HEAVYSHOTGUN",
                "WEAPON_DBSHOTGUN",
                "WEAPON_COMBATSHOTGUN"
            };

            int num = random.Next(shotguns.Length);
            if (ped.Exists()) { ped.Inventory.GiveNewWeapon(shotguns[num], ammoCount, equipNow); }
        }
        #endregion

        #region Machine Gun
        internal static void GiveRandomMachineGun(this Ped ped, short ammoCount, bool equipNow)
        {
            string[] machineGuns =
            {
                "WEAPON_MG",
                "WEAPON_COMBATMG",
                "WEAPON_GUSENBERG"
            };

            int num = random.Next(machineGuns.Length);
            if (ped.Exists()) { ped.Inventory.GiveNewWeapon(machineGuns[num], ammoCount, equipNow); }
        }
        #endregion

        #region Sniper Rifle
        internal static void GiveRandomSniperRifle(this Ped ped, short ammoCount, bool equipNow)
        {
            string[] sniperRifles =
            {
                "WEAPON_SNIPERRIFLE",
                "WEAPON_HEAVYSNIPER",
                "WEAPON_MARKSMANRIFLE"
            };

            int num = random.Next(sniperRifles.Length);
            if (ped.Exists()) { ped.Inventory.GiveNewWeapon(sniperRifles[num], ammoCount, equipNow); }
        }
        #endregion
    }

    internal static class ExtensionMethods
    {
        #region GetSafePositionForPed
        public static unsafe bool GetSafePositionForPed(this Vector3 CalloutPosition, out Vector3 SafePosition)
        {
            if (!NativeFunction.Natives.GET_SAFE_COORD_FOR_PED<bool>(CalloutPosition.X, CalloutPosition.Y, CalloutPosition.Z, true, out Vector3 TempSpawn, 0))
            {
                TempSpawn = World.GetNextPositionOnStreet(CalloutPosition);
                Rage.Entity NearbyEntity = World.GetClosestEntity(TempSpawn, 25f, GetEntitiesFlags.ConsiderHumanPeds);

                if (NearbyEntity.Exists())
                {
                    TempSpawn = NearbyEntity.Position;
                    SafePosition = TempSpawn;
                    return true;
                }
                else
                {
                    SafePosition = TempSpawn;
                    return false;
                }
            }
            SafePosition = TempSpawn;
            return true;
        }
        #endregion

        #region Enable
        internal static void Enable(this Blip blip)
        {
            if (blip.Exists()) { blip.Alpha = 1f; }
        }
        #endregion

        #region Disable
        internal static void Disable(this Blip blip)
        {
            if (blip.Exists()) { blip.Alpha = 0f; }
        }
        #endregion

        #region EnableRoute
        internal static void EnableRoute(this Blip blip)
        {
            if (blip.Exists()) { blip.IsRouteEnabled = true; }
        }
        #endregion

        #region DisableRoute
        internal static void DisableRoute(this Blip blip)
        {
            if (blip.Exists()) { blip.DisableRoute(); }
        }
        #endregion

        #region SetDefaults
        internal static void SetDefaults(this Ped ped)
        {
            if (ped.Exists())
            {
                ped.BlockPermanentEvents = true;
                ped.IsPersistent = true;
            }
        }
        #endregion

        #region SetIntoxicated
        internal static void SetIntoxicated(this Ped ped)
        {
            AnimationSet animSet = new AnimationSet("move_m@drunk@verydrunk");
            animSet.LoadAndWait();
            ped.MovementAnimationSet = animSet;
        }
        #endregion

        #region SetInjured
        internal static void SetInjured(this Ped ped, int health)
        {
            AnimationSet animSet = new AnimationSet("move_m@injured");
            animSet.LoadAndWait();
            ped.MovementAnimationSet = animSet;

            if (ped.IsAlive) { ped.Health = health; }
        }
        #endregion
    }
}