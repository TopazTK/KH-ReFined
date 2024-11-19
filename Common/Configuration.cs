using ReFined.KH2.Information;
using ReFined.Libraries;

namespace ReFined.Common
{
    public static class Configuration
    {
        public static void Initialize()
        {
            if (!File.Exists("reFined.cfg"))
            {
                var _outDefault = new string[]
                {
                    "[General]",
                    "liteMode = false",
                    "discordRPC = true",
                    "resetCombo = 0x0300",
                    "",
                    "[Kingdom Hearts II]",
                    "driveShortcuts = true",
                    "resetPrompt = true",
                    "",
                    "# Options: retry, continue",
                    "deathPrompt = retry",
                    "",
                    "# Options: sonic, arcanum, raid, ragnarok",
                    "# Order: [CONFIRM], TRI, SQU, [JUMP]",
                    "# Duplicates are allowed. All 4 slots must be filled.",
                    "limitShortcuts = [sonic, arcanum, raid, ragnarok]",
                };

                File.WriteAllLines("reFined.cfg", _outDefault);
            }

            else
            {
                var _confIni = new INI("reFined.cfg");
                if (_confIni.KeyExists("debugMode", "General"))
                    Variables.DEV_MODE = Convert.ToBoolean(_confIni.Read("debugMode", "General"));
            }
        }
    }
}
