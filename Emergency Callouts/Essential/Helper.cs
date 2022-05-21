using Rage;
using System.Reflection;

namespace EmergencyCallouts.Essential
{
    internal static class Helper
    {
        internal static string LocalVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        internal static Ped MainPlayer => Game.LocalPlayer.Character;
    }
}