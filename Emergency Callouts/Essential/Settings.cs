using Rage;
using System.Windows.Forms;

namespace EmergencyCallouts.Essential
{
    internal static class Settings
    {
        // Callouts
        internal static bool PublicIntoxication = true;
        internal static bool Trespassing = true;
        internal static bool DomesticViolence = true;
        internal static bool GroceryStoreFight = true;

        // Measurements
        internal static int SearchAreaSize = 60;
        internal static int MaxCalloutDistance = 1000;
        internal static double PedBlipScale = 0.75f;

        // Keys
        internal static Keys InteractKey = Keys.Y;
        internal static ControllerButtons ControllerInteractKey = ControllerButtons.DPadLeft;
        internal static Keys EndCalloutKey = Keys.End;
        internal static bool AllowController = true;

        // Chances
        internal static int ChanceOfPressingCharges = 50;
        internal static int ChanceOfCallingOwner = 50;

        // Back end
        internal static bool EarlyAccess = true;

        internal static void Initialize()
        {
            // Create the INI file
            var iniFile = new InitializationFile(Project.SettingsPath);
            iniFile.Create();

            // Callouts
            PublicIntoxication = iniFile.ReadBoolean("Callouts", "PublicIntoxication", PublicIntoxication);
            Trespassing = iniFile.ReadBoolean("Callouts", "Trespassing", Trespassing);
            DomesticViolence = iniFile.ReadBoolean("Callouts", "DomesticViolence", DomesticViolence);
            GroceryStoreFight = iniFile.ReadBoolean("Callouts", "GroceryStoreFight", GroceryStoreFight);

            // Measurements
            SearchAreaSize = iniFile.ReadInt32("Measurements", "SearchAreaSize", SearchAreaSize);
            MaxCalloutDistance = iniFile.ReadInt32("Measurements", "MaxCalloutDistance", MaxCalloutDistance);
            PedBlipScale = iniFile.ReadDouble("Measurements", "PedBlipScale", PedBlipScale);

            // Keybindings
            InteractKey = iniFile.ReadEnum("Keybindings", "InteractKey", InteractKey);
            EndCalloutKey = iniFile.ReadEnum("Keybindings", "EndCalloutKey", EndCalloutKey);

            // Chances
            ChanceOfPressingCharges = iniFile.ReadInt32("Chances", "ChanceOfPressingCharges", ChanceOfPressingCharges);
            ChanceOfCallingOwner = iniFile.ReadInt32("Chances", "ChanceOfCallingOwner", ChanceOfCallingOwner);
            
            Game.LogTrivial("[Emergency Callouts]: Loaded settings");
        }
    }
}