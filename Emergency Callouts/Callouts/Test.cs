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
    [CalloutInfo("Test", CalloutProbability.Medium)]
    public class Test : Callout
    {
        bool PlayerArrived;
        bool PedFound;
        bool PedDetained;

        Vector3 CalloutPosition;

        Ped Suspect;
        Persona SuspectPersona;

        Blip EntranceBlip;
        Blip SearchArea;
        Blip SuspectBlip;

        public override bool OnBeforeCalloutDisplayed()
        {
            World.GetNextPositionOnStreet(MainPlayer.Position.Around2D(100f, 300f)).GetSafePositionForPed(out CalloutPosition);

            CalloutMessage = "Test";
            CalloutArea = World.GetStreetName(CalloutPosition);

            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, Settings.SearchAreaSize / 2.5f);

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
            EntranceBlip = new Blip(CalloutPosition);
            EntranceBlip.EnableRoute();

            Suspect = new Ped(Entity.GetRandomMaleModel(), CalloutPosition, 0f);
            SuspectPersona = Functions.GetPersonaForPed(Suspect);
            Suspect.SetDefaults();

            SuspectBlip = Suspect.AttachBlip();
            SuspectBlip.SetColorYellow();
            SuspectBlip.ScaleForPed();
            SuspectBlip.Disable();

            // Log Creation
            Log.Creation(Suspect, PedCategory.Suspect);

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            try
            {
                Handle.ManualEnding();
                Handle.PreventFirstResponderCrash(Suspect);

                #region PlayerArrived
                if (MainPlayer.Position.DistanceTo(CalloutPosition) < Settings.SearchAreaSize && !PlayerArrived)
                {
                    // Remove EntranceBlip
                    EntranceBlip.Remove();

                    // Create SearchArea
                    SearchArea = new Blip(Suspect.Position.Around(5f, 30f), Settings.SearchAreaSize);
                    SearchArea.SetColorYellow();
                    SearchArea.Alpha = 0.5f;

                    // Display Subtitle
                    Game.DisplaySubtitle("Find the ~r~test ped~s~ in the ~y~area~s~.", 20000);

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
                    SearchArea.Remove();

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has found {SuspectPersona.FullName} (Suspect)");

                    PedFound = true;
                }
                #endregion

                #region PedDetained
                if (Functions.IsPedStoppedByPlayer(Suspect) && !PedDetained && Suspect.Exists())
                {
                    // Remove SuspectBlip
                    SuspectBlip.Remove();

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
                    SearchArea.Remove();

                    // Create EntranceBlip
                    EntranceBlip = new Blip(CalloutPosition);

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

            if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            if (SearchArea.Exists()) { SearchArea.Delete(); }
            if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

            Display.HideSubtitle();
            Display.EndNotification();
        }
    }
}