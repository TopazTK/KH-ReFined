using ReFined.Libraries;
using ReFined.KH2.Information;

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
                    "resetCombo = [L2, R2]",
                    "mareShortcut = [SQUARE]",
                    "",
                    "[Accessibility]",
                    "autoAttack = false",
                    "",
                    "[Kingdom Hearts II]",
                    "driveShortcuts = true",
                    "resetPrompt = true",
                    "deathPrompt = retry",
                    "limitShortcuts = [sonic, arcanum, raid, ragnarok]",
                };

                File.WriteAllLines("reFined.cfg", _outDefault);
            }

            else
            {
                var _readFile = File.ReadAllText("reFined.cfg");

                if (!_readFile.Contains("mareShortcut"))
                {
                    File.Delete("reFined.cfg");
                    Initialize();
                    return;
                }

                var _confIni = new INI("reFined.cfg");
                if (_confIni.KeyExists("debugMode", "General"))
                    Variables.DEV_MODE = Convert.ToBoolean(_confIni.Read("debugMode", "General"));
            }
        }
    }
}
