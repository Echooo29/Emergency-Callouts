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
using System.Media;

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

        SoundPlayer soundPlayer;


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
                Log.Exception(e, Project.CurrentClass, Project.CurrentMethod);
            }
            #endregion
        }

        private void Scenario1() // Standard
        {
            #region Scenario 1
            try
            {
                // Already handled
            }
            catch (Exception e)
            {
                Log.Exception(e, Project.CurrentClass, Project.CurrentMethod);
            }
            #endregion
        }

        private void Scenario2() // Crazy
        {
            #region Scenario 2
            try
            {
                GameFiber.StartNew(delegate
                {
                    while (true)
                    {
                        GameFiber.Yield();

                        if (Suspect.Exists() && MainPlayer.Position.DistanceTo(Suspect.Position) <= 7f && Suspect.IsAlive && MainPlayer.IsAlive)
                        {
                            // Do backflip
                            Suspect.Tasks.PlayAnimation(new AnimationDictionary(""), "", 5f, AnimationFlags.None);

                            // Scream Frantically
                            soundPlayer = new SoundPlayer($@"Emergency Callouts Audio\FRANTIC_SCREAM_01");
                            soundPlayer.Play();
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Log.Exception(e, Project.CurrentClass, Project.CurrentMethod);
            }
            #endregion
        }

        private void Scenario3() // Throw Baseball
        {
            #region Scenario 3
            try
            {
                if (Suspect.Exists()) { Suspect.Inventory.GiveNewWeapon("WEAPON_BASEBALL", 3, true); }

                GameFiber.StartNew(delegate
                {
                    while (CalloutActive)
                    {
                        GameFiber.Yield();

                        if (MainPlayer.Position.DistanceTo(Suspect.Position) <= 7f && Suspect.IsAlive && MainPlayer.IsOnFoot && PlayerArrived)
                        {
                            Suspect.Tasks.FightAgainst(MainPlayer);

                            // Play FUCK_YOU audio file
                            int rand = new Random().Next(1, 3);
                            soundPlayer = new SoundPlayer($@"Emergency Callouts Audio\FUCK_YOU_0{rand}.wav");
                            soundPlayer.Play();

                            break;
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Log.Exception(e, Project.CurrentClass, Project.CurrentMethod);
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
                if (Settings.AllowController) { NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 27, true); }

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
                Log.Exception(e, Project.CurrentClass, Project.CurrentMethod);
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