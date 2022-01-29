using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static EmergencyCallouts.Essential.Helper;

namespace EmergencyCallouts.Other
{
    internal class CalloutInterfaceExtension
    {
        /// <summary>
        /// Send details about the callout to the MDT.  Call this in OnCalloutDisplayed.
        /// </summary>
        /// <param name="sender">The originating callout.</param>
        /// <param name="priority">The priority of the callout (e.g. CODE 2, CODE 3).</param>
        /// <param name="agency">The agency that should be handling the callout (e.g. LSPD, LSSD).</param>
        public static void SendCalloutDetails(LSPD_First_Response.Mod.Callouts.Callout sender, string priority, string agency = "")
        {
            try
            {
                CalloutInterface.API.Functions.SendCalloutDetails(sender, priority, agency);
            }
            catch (System.Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Sends a message to the MDT.
        /// </summary>
        /// <param name="sender">The sending callout.</param>
        /// <param name="message">The message.</param>
        public static void SendMessage(LSPD_First_Response.Mod.Callouts.Callout sender, string message)
        {
            try
            {
                CalloutInterface.API.Functions.SendMessage(sender, message);
            }
            catch (System.Exception e)
            {
                Log.Exception(e, MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
