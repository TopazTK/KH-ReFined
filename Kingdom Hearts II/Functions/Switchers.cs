using System.Text;

using ReFined.Common;
using ReFined.Libraries;
using ReFined.KH2.Information;
using ReFined.KH2.InGame;
using System.Security.Cryptography;

namespace ReFined.KH2.Functions
{
    public static class Switchers
    {
        static string WL_SUFF;
        static string US_SUFF;
        static string FM_SUFF;

        static bool TOGGLE_WI;
        static bool PAST_MUSIC;
        static bool PAST_ENEMY;
        static byte[] OBJENTRY_READ;

        static byte PAST_VLAD = 0x00;
        static byte PAST_TYPE = 0x00;

        public static void SwitchWillie()
        {
            var _worldCheck = Hypervisor.Read<byte>(Variables.ADDR_Area);

            if (OBJENTRY_READ == null)
            {
                var _headerCheck = Hypervisor.Read<byte>(Variables.ADDR_ObjentryBase);
                var _itemCount = Hypervisor.Read<int>(Variables.ADDR_ObjentryBase + 0x04);

                if (_headerCheck == 0x03)
                    OBJENTRY_READ = Hypervisor.Read<byte>(Variables.ADDR_ObjentryBase + 0x08, 0x60 * _itemCount);
            }

            if (!Variables.TECHNICOLOR)
            {
                if (OBJENTRY_READ != null)
                {
                    if (Variables.IS_TITLE)
                        OBJENTRY_READ = null;

                    if (_worldCheck == 0x0D && !TOGGLE_WI)
                    {
                        Terminal.Log("Adjusting Elements for Timeless River.", 0);

                        foreach (var _name in Variables.SUMMObjentry)
                        {
                            var _fetchSummon = Encoding.Default.GetBytes(_name);
                            var _searchSummon = OBJENTRY_READ.FindValue(_fetchSummon);

                            if (_searchSummon == 0xFFFFFFFFFFFFFFFF)
                                break;

                            else
                                Hypervisor.WriteString(Variables.ADDR_ObjentryBase + 0x08 + _searchSummon, _name.Replace("P_", "X_").Replace("N_", "X_"));
                        }

                        TOGGLE_WI = true;
                    }

                    else if (_worldCheck != 0x0D && TOGGLE_WI)
                    {
                        Terminal.Log("Adjusting Elements for Colored Worlds.", 0);

                        foreach (var _name in Variables.SUMMObjentry)
                        {
                            var _fetchSummon = Encoding.Default.GetBytes(_name.Replace("P_", "X_").Replace("N_", "X_"));
                            var _searchSummon = OBJENTRY_READ.FindValue(_fetchSummon);

                            if (_searchSummon == 0xFFFFFFFFFFFFFFFF)
                                break;

                            else
                                Hypervisor.WriteString(Variables.ADDR_ObjentryBase + 0x08 + _searchSummon, _name);
                        }

                        TOGGLE_WI = false;
                    }
                }
            }
        }

        public static void SwitchCommand()
        {
            var _checkString = Hypervisor.ReadString(Variables.ADDR_CommandMenu);
            var _vladBit = Hypervisor.Read<byte>(Variables.ADDR_Config + 0x03);
            var _typeCheck = Hypervisor.Read<byte>(Variables.ADDR_PromptType);

            if (_vladBit == 0x01 && (_typeCheck != PAST_TYPE || !_checkString.Contains("qd0")))
            {
                Terminal.Log("Toggling the Quadratum Command Menu.", 0x00);

                if (_typeCheck == 0x01)
                {
                    Hypervisor.WriteString(Variables.ADDR_CommandMenu, "field2d/%s/qd0cmdxbox.2dd");
                    Hypervisor.WriteString(Variables.ADDR_CommandMenu + 0x20, "qd0cmdxbox.2dd");
                }

                else
                {
                    Hypervisor.WriteString(Variables.ADDR_CommandMenu, "field2d/%s/qd0command.2dd");
                    Hypervisor.WriteString(Variables.ADDR_CommandMenu + 0x20, "qd0command.2dd");
                }

                PAST_TYPE = _typeCheck;
                Hypervisor.Write(Variables.ADDR_CommandFlag, 0x02);
            }

            else if (_vladBit == 0x00 && !_checkString.Contains("zz0command"))
            {
                Terminal.Log("Toggling the Classic Command Menu.", 0x00);

                Hypervisor.WriteString(Variables.ADDR_CommandMenu, "field2d/%s/zz0command.2dd");
                Hypervisor.WriteString(Variables.ADDR_CommandMenu + 0x20, "zz0command.2dd");

                Hypervisor.Write(Variables.ADDR_CommandFlag, 0x02);
            }
        }

        public static void SwitchAudio()
        {
            var _audioRead = Hypervisor.Read<byte>(Variables.ADDR_Config + 0x02);
            var _paxCheck = Hypervisor.ReadString(Variables.DATA_PAXPath + 0x10);

            var _stringANM = "anm/{0}/";
            var _stringPAX = "obj/%s.a.{0}";
            var _stringEVT = "voice/{0}/event/";
            var _stringBTL = "voice/{0}/battle/";
            var _stringGMI = "voice/{0}/gumibattle/gumi.win32.scd";

            var _audioSuffix = "us";
            var _audioFormat = String.Format(_stringPAX, _audioSuffix);

            US_SUFF = "us";
            FM_SUFF = "fm";

            try
            {
                if (Variables.AUDIO_MODE == 0x01)
                {
                    _audioSuffix = Variables.LOADED_LANGS[0x00].ToLower();
                    _audioFormat = String.Format(_stringPAX, _audioSuffix);

                    if (Critical.AUDIO_SUB_ONLY)
                    {
                        _audioSuffix = Variables.LOADED_LANGS[_audioRead].ToLower();
                        _audioFormat = String.Format(_stringPAX, _audioSuffix);
                    }
                }

                else if (Variables.AUDIO_MODE == 0x02)
                {
                    _audioSuffix = Variables.LOADED_LANGS[_audioRead + 0x01].ToLower();
                    _audioFormat = String.Format(_stringPAX, _audioSuffix);
                }
            }

            catch (ArgumentOutOfRangeException)
            {
                Terminal.Log("Caught an exception within Multi Audio... Switching to English Audio.", 1);
                Variables.AUDIO_MODE = 0x00;

                _audioSuffix = "us";
                _audioFormat = String.Format(_stringPAX, _audioSuffix);

                US_SUFF = "us";
                FM_SUFF = "fm";

            }

            if (_paxCheck != _audioFormat)
            {
                Terminal.Log("Switching to " + _audioSuffix.ToUpper() + " Audio...", 0);
                
                if (Variables.AUDIO_MODE != 0x00)
                {
                    US_SUFF = _audioSuffix;
                    FM_SUFF = _audioSuffix;
                }

                Hypervisor.WriteString(Variables.DATA_PAXPath, String.Format(_stringPAX, FM_SUFF));
                Hypervisor.WriteString(Variables.DATA_PAXPath + 0x10, String.Format(_stringPAX, US_SUFF));

                Hypervisor.WriteString(Variables.DATA_ANBPath, String.Format(_stringANM, US_SUFF != "jp" ? "us" : US_SUFF));
                Hypervisor.WriteString(Variables.DATA_ANBPath + 0x08, String.Format(_stringANM, US_SUFF != "jp" ? "fm" : FM_SUFF));

                Hypervisor.WriteString(Variables.DATA_BTLPath, String.Format(_stringBTL, US_SUFF));
                Hypervisor.WriteString(Variables.DATA_EVTPath, String.Format(_stringEVT, US_SUFF));

                Hypervisor.WriteString(Variables.DATA_GMIPath, String.Format(_stringGMI, US_SUFF));
                Hypervisor.WriteString(Variables.DATA_GMIPath + 0x28, String.Format(_stringGMI, US_SUFF));
            }
        }

        public static void SwitchMusic()
        {
            if (Variables.MUSIC_VANILLA != PAST_MUSIC)
            {
                Terminal.Log(String.Format("Switching Music to {0}...", Variables.MUSIC_VANILLA ? "Vanilla" : "Remastered"), 0);
                Hypervisor.Write<byte>(Variables.DATA_BGMPath, Variables.MUSIC_VANILLA ? [0x70, 0x73, 0x32, 0x6D, 0x64] : [0x6D, 0x75, 0x73, 0x69, 0x63]);

                PAST_MUSIC = Variables.MUSIC_VANILLA;
            }
        }

        public static void SwitchEnemies()
        {
            if (!Variables.IS_TITLE && Variables.ENEMY_VANILLA != PAST_ENEMY)
            {
                Terminal.Log(String.Format("Switching Enemies to the {0} Palette...", Variables.ENEMY_VANILLA ? "Classic" : "Special"), 0);

                foreach (var _id in Variables.BOSSObjentry)
                {
                    var _fetchObjentry = Operations.FindInObjentry(_id);
                    var _readName = Hypervisor.ReadString(_fetchObjentry + 0x08, true);

                    var _nameString = _readName.StartsWith("V_") ? _readName.Replace("V_", "B_") : _readName.Replace("B_", "V_");

                    Hypervisor.WriteString(_fetchObjentry + 0x08, _nameString, true);
                }

                foreach (var _id in Variables.ENEMYObjentry)
                {
                    var _fetchObjentry = Operations.FindInObjentry(_id);
                    var _readName = Hypervisor.ReadString(_fetchObjentry + 0x08, true);

                    var _nameString = _readName.StartsWith("V_") ? _readName.Replace("V_", "M_") : _readName.Replace("M_", "V_");

                    Hypervisor.WriteString(_fetchObjentry + 0x08, _nameString, true);
                }

                PAST_ENEMY = Variables.ENEMY_VANILLA;
            }
        }
    }
}
