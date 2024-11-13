using System.Text;

using ReFined.Common;
using ReFined.Libraries;
using ReFined.KH2.Information;

namespace ReFined.KH2.Functions
{
    public static class Switchers
    {
        static string WL_SUFF;
        static string US_SUFF;
        static string FM_SUFF;

        static bool PAST_MUSIC;
        static bool PAST_ENEMY;
        static byte[] OBJENTRY_READ;

        public static void SwitchAudio()
        {
            var _byteCheck = Hypervisor.Read<byte>(Variables.ADDR_EVTFormatter, 0x0F);
            var _stringCheck = Encoding.Default.GetString(_byteCheck);

            var _stringANM = "anm/{0}/";
            var _stringPAX = "obj/%s.a.{0}";
            var _stringEVT = "voice/{0}/event";
            var _stringBTL = "voice/{0}/battle";

            if (Variables.AUDIO_MODE == 0x01 && _stringCheck != "voice/jp/event/")
            {
                Terminal.Log("Switching to Japanese Audio...", 0);

                WL_SUFF = "jp";
                US_SUFF = "jp";
                FM_SUFF = "jp";
            }

            if (Variables.AUDIO_MODE == 0x02 && _stringCheck != "voice/xx/event/")
            {
                Terminal.Log("Switching to Extra Audio...", 0);

                WL_SUFF = "xx";
                US_SUFF = "xx";
                FM_SUFF = "xx";
            }

            if (Variables.AUDIO_MODE == 0x00 && _stringCheck != "voice/us/event/")
            {
                Terminal.Log("Switching to English Audio...", 0);

                WL_SUFF = "%s";
                US_SUFF = "us";
                FM_SUFF = "fm";
            }

            if (_stringCheck != String.Format(_stringPAX, WL_SUFF))
            {
                Hypervisor.Write(Variables.ADDR_PAXFormatter, Encoding.Default.GetBytes(String.Format(_stringPAX, WL_SUFF)));
                Hypervisor.Write(Variables.ADDR_PAXFormatter + 0x10, Encoding.Default.GetBytes(String.Format(_stringPAX, US_SUFF)));

                Hypervisor.Write(Variables.ADDR_ANBFormatter, Encoding.Default.GetBytes(String.Format(_stringANM, US_SUFF)));
                Hypervisor.Write(Variables.ADDR_ANBFormatter + 0x08, Encoding.Default.GetBytes(String.Format(_stringANM, FM_SUFF)));

                Hypervisor.Write(Variables.ADDR_BTLFormatter, Encoding.Default.GetBytes(String.Format(_stringBTL, US_SUFF)));
                Hypervisor.Write(Variables.ADDR_EVTFormatter, Encoding.Default.GetBytes(String.Format(_stringEVT, US_SUFF)));
            }
        }

        public static void SwitchMusic()
        {
            if (Variables.MUSIC_VANILLA != PAST_MUSIC)
            {
                Terminal.Log(String.Format("Switching Music to {0}...", Variables.MUSIC_VANILLA ? "Vanilla" : "Remastered"), 0);
                Hypervisor.Write<byte>(Variables.ADDR_MusicPath, Variables.MUSIC_VANILLA ? [0x70, 0x73, 0x32, 0x6D, 0x64] : [0x6D, 0x75, 0x73, 0x69, 0x63]);

                PAST_MUSIC = Variables.MUSIC_VANILLA;
            }
        }

        public static void SwitchEnemies()
        {
            byte _bossPrefix = Variables.ENEMY_VANILLA ? (byte)0x56 : (byte)0x42;
            byte _enemyPrefix = Variables.ENEMY_VANILLA ? (byte)0x56 : (byte)0x4D;

            if (OBJENTRY_READ == null)
            {
                var _headerCheck = Hypervisor.Read<byte>(Variables.ADDR_ObjentryBASE);
                var _itemCount = Hypervisor.Read<int>(Variables.ADDR_ObjentryBASE + 0x04);

                if (_headerCheck == 0x03)
                    OBJENTRY_READ = Hypervisor.Read<byte>(Variables.ADDR_ObjentryBASE + 0x08, 0x60 * _itemCount);
            }

            if (OBJENTRY_READ != null)
            {
                if (Variables.IS_TITLE)
                    OBJENTRY_READ = null;

                else if (Variables.ENEMY_VANILLA != PAST_ENEMY)
                {
                    Terminal.Log(String.Format("Switching Enemies to the {0} Palette...", Variables.ENEMY_VANILLA ? "Classic" : "Special"), 0);

                    foreach (var _name in Variables.BOSSObjentry)
                    {
                        var _stringArr1 = Encoding.Default.GetBytes(_name);
                        var _stringArr2 = Encoding.Default.GetBytes(_name.Replace("B_", "V_"));

                        var _searchClassic = OBJENTRY_READ.FindValue(_stringArr2);
                        var _searchRemastered = OBJENTRY_READ.FindValue(_stringArr1);

                        if (_searchClassic == 0xFFFFFFFFFFFFFFFF && _searchRemastered == 0xFFFFFFFFFFFFFFFF)
                            break;

                        else
                            Hypervisor.Write(Variables.ADDR_ObjentryBASE + 0x08 + (_searchClassic == 0xFFFFFFFFFFFFFFFF ? _searchRemastered : _searchClassic), _bossPrefix);
                    }

                    foreach (var _name in Variables.ENEMYObjentry)
                    {
                        var _stringArr1 = Encoding.Default.GetBytes(_name);
                        var _stringArr2 = Encoding.Default.GetBytes(_name.Replace("M_", "V_"));

                        var _searchClassic = OBJENTRY_READ.FindValue(_stringArr2);
                        var _searchRemastered = OBJENTRY_READ.FindValue(_stringArr1);

                        if (_searchClassic == 0xFFFFFFFFFFFFFFFF && _searchRemastered == 0xFFFFFFFFFFFFFFFF)
                            break;

                        else
                            Hypervisor.Write(Variables.ADDR_ObjentryBASE + 0x08 + (_searchClassic == 0xFFFFFFFFFFFFFFFF ? _searchRemastered : _searchClassic), _enemyPrefix);
                    }

                    PAST_ENEMY = Variables.ENEMY_VANILLA;
                }
            }
        }
    }
}
