using EmergencyCallouts.Essential;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using RAGENativeUI;
using System;
using System.Reflection;
using static EmergencyCallouts.Essential.Color;
using static EmergencyCallouts.Essential.Helper;

namespace EmergencyCallouts.Callouts
{
    [CalloutInfo("[EC] Grocery Store Fight", CalloutProbability.Medium)]
    public class GroceryStoreFight : Callout
    {
        bool CalloutActive;
        bool PlayerArrived;
        bool PedFound;
        bool PedDetained;

        Ped Customer;
        Ped Cashier;
        Ped Manager;

        Blip CustomerBlip;
        Blip CashierBlip;
        Blip ManagerBlip;
        Blip EntranceBlip;

        Vector3 FightPosition;
        Vector3 CashierPosition;

        private Vector3[] CalloutPositions = new Vector3[] 
        {
            new Vector3(-711.9525f, -921.0374f, 18.60401f), 
            new Vector3(-528.0559f, -1218.164f, 17.85979f), 
            new Vector3(818.0175f, -1037.259f, 26.10594f),
            new Vector3 (-2079.705f, -319.7511f, 12.74332f), 
            new Vector3(53.59156f, 2784.886f, 57.60809f), 
            new Vector3(-90.44811f, 6416.361f, 31.05175f), 
            new Vector3(2565.666f,384.0004f,108.4633f) 
        };

        Vector3[] FightPositions = new Vector3[]
        {
            new Vector3(-708.4945f, -914.0405f, 19.3f), // Little Seoul
            new Vector3(818f, -1037f, 26f),
            new Vector3 (-2080f, -319.8f, 13f),
            new Vector3(53.6f, 2785f, 57.6f),
            new Vector3(-90.5f, 6416.4f, 31f),
            new Vector3(2566f, 384f, 108.5f)
        };
        
        Vector3[] CashierPositions = new Vector3[]
        {
            new Vector3(-705.6636f, -913.5624f, 19.3f), // Little Seoul
            new Vector3(818f, -1037f, 26f),
            new Vector3 (-2080f, -319.8f, 13f),
            new Vector3(53.6f, 2785f, 57.6f),
            new Vector3(-90.5f, 6416.4f, 31f),
            new Vector3(2566f, 384f, 108.5f)
        };

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

            FightPosition = new Vector3(0, 0, 3000);
            foreach (Vector3 loc in FightPositions)
            {
                if (Vector3.Distance(MainPlayer.Position, loc) < Vector3.Distance(MainPlayer.Position, FightPosition))
                {
                    FightPosition = loc;
                }
            }

            CashierPosition = new Vector3(0, 0, 3000);
            foreach (Vector3 loc in CashierPositions)
            {
                if (Vector3.Distance(MainPlayer.Position, loc) < Vector3.Distance(MainPlayer.Position, CashierPosition))
                {
                    CashierPosition = loc;
                }
            }

            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, Settings.SearchAreaSize / 2.5f);

            CalloutArea = World.GetStreetName(CalloutPosition);
            CalloutMessage = "Grocery Store Fight";
            CalloutAdvisory = "The cashier called for a fight between a customer and the manager.";
            CalloutScenario = random.Next(1, 4);

            Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT CRIME_ASSAULT IN_OR_ON_POSITION", CalloutPosition);

            return base.OnBeforeCalloutDisplayed();
        }

        public override void OnCalloutDisplayed()
        {
            if (Other.PluginChecker.IsCalloutInterfaceRunning)
            {
                Other.CalloutInterfaceFunctions.SendCalloutDetails(this, "CODE-3", "");
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
                // Callout Accepted
                Log.OnCalloutAccepted(CalloutMessage, CalloutScenario);

                // Accepting Messages
                Display.AcceptSubtitle(CalloutMessage, CalloutArea);
                Display.OutdatedReminder();

                // Customer
                Customer = new Ped(FightPosition);
                Customer.IsPersistent = true;
                Customer.BlockPermanentEvents = true;

                CustomerBlip = Customer.AttachBlip();
                CustomerBlip.SetColorRed();
                CustomerBlip.Scale = (float)Settings.PedBlipScale;
                CustomerBlip.Alpha = 0f;

                // Cashier
                Cashier = new Ped(FightPosition);
                Cashier.IsPersistent = true;
                Cashier.BlockPermanentEvents = true;

                CashierBlip = Cashier.AttachBlip();
                CashierBlip.SetColorGreen();
                CashierBlip.Scale = (float)Settings.PedBlipScale;
                CashierBlip.Alpha = 0f;

                // Manager
                Manager = new Ped(FightPosition);
                Manager.IsPersistent = true;
                Manager.BlockPermanentEvents = true;

                ManagerBlip = Manager.AttachBlip();
                ManagerBlip.SetColorRed();
                ManagerBlip.Scale = (float)Settings.PedBlipScale;
                ManagerBlip.Alpha = 0f;

                // Entrance
                EntranceBlip = new Blip(CalloutPosition);
                EntranceBlip.SetColorYellow();
                EntranceBlip.IsRouteEnabled = true;

                Manager.Tasks.FightAgainst(Customer);
                Customer.Tasks.FightAgainst(Manager);

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

        private void Scenario1()
        {
            #region Scenario 1
            try
            {
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            }
            #endregion
        }

        private void Scenario2()
        {
            #region Scenario 2
            try
            {
            }
            catch (Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            }
            #endregion
        }

        private void Scenario3()
        {
            #region Scenario 3
            try
            {
            }
            catch(Exception e)
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
                Handle.PreventPickupCrash(Customer);
                if (Settings.AllowController) { NativeFunction.Natives.xFE99B66D079CF6BC(0, 27, true); }

                #region PlayerArrived
                if (MainPlayer.Position.DistanceTo(CalloutPosition) <= 30f && !PlayerArrived)
                {
                    // Set PlayerArrived
                    PlayerArrived = true;

                    // Display Arriving Subtitle
                    Game.DisplaySubtitle("Enter the ~p~store~s~.", 10000);

                    // Delete EntranceBlip
                    if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

                    Game.LogTrivial($"[Emergency Callouts]: {PlayerPersona.FullName} has arrived on scene");
                }
                #endregion

                #region PedFound
                if (MainPlayer.Position.DistanceTo(Customer.Position) < 5f && !PedFound && PlayerArrived && Customer.Exists())
                {
                    // Set PedFound
                    PedFound = true;

                    // Hide Subtitle
                    Display.HideSubtitle();

                    // Enable SuspectBlip
                    if (CustomerBlip.Exists()) { CustomerBlip.Alpha = 1f; }
                }
                #endregion

                #region PedDetained
                if (Functions.IsPedStoppedByPlayer(Customer) && !PedDetained && Customer.Exists())
                {
                    // Set PedDetained
                    PedDetained = true;

                    // Delete SuspectBlip
                    if (CustomerBlip.Exists()) { CustomerBlip.Delete(); }
                    Game.LogTrivial("[Emergency Callouts]: Deleted CustomerBlip");
                }
                #endregion

                #region PlayerLeft
                if (MainPlayer.Position.DistanceTo(CalloutPosition) > Settings.SearchAreaSize * 3.5f && PlayerArrived && !PedFound)
                {
                    // Set PlayerArrived
                    PlayerArrived = false;

                    // Disable SuspectBlip
                    if (CustomerBlip.Exists()) { CustomerBlip.Alpha = 0f; }

                    // Create EntranceBlip
                    EntranceBlip = new Blip(CalloutPosition);

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

            if (Customer.Exists()) { Customer.Dismiss(); }
            if (CustomerBlip.Exists()) { CustomerBlip.Delete(); }
            if (Cashier.Exists()) { Cashier.Delete(); }
            if (CashierBlip.Exists()) { CashierBlip.Delete(); }
            if (Manager.Exists()) { Manager.Delete(); }
            if (ManagerBlip.Exists()) { ManagerBlip.Delete(); }
            if (EntranceBlip.Exists()) { EntranceBlip.Delete(); }

            Display.HideSubtitle();
            Display.EndNotification();
            Log.OnCalloutEnded(CalloutMessage, CalloutScenario);
        }
    }
}