#define STEAM_RELEASE

using DiscordRPC;

using ReFined.Common;
using ReFined.Libraries;
using ReFined.KH2.Information;
using ReFined.KH2.InGame;

namespace ReFined.KH2.Functions
{
    public static class Continuous
    {
        public static ulong SAVE_OFFSET;
        public static ulong PROMPT_OFFSET;
        public static ulong LIMITER_OFFSET;

        public static short[] LIMIT_SHORT = null;
        static byte[] LIMITER_FUNCTION = null;

        static bool SAVE_RESET;

        static List<string> RPC_TEXTS = null;
        static List<string> MODE_TEXTS = null;
        static List<string> FORM_TEXTS = null;

        public static void ToggleSavePoint()
        {
            var _reactionRead = Hypervisor.Read<ushort>(Variables.ADDR_ReactionID);

            if (SAVE_RESET)
            {
                Hypervisor.Write<byte>(SAVE_OFFSET + 0x25B, 0x75);
                SAVE_RESET = false;
            }

            if (!Variables.IS_LOADED && !SAVE_RESET)
                SAVE_RESET = true;

            if (_reactionRead == 0x0037)
            {
                var _soraHPRead = Hypervisor.Read<byte>(Variables.ADDR_PlayerHP, 0x08);
                var _soraMPRead = Hypervisor.Read<byte>(Variables.ADDR_PlayerHP + 0x180, 0x08);

                var _donaldHPRead = Hypervisor.Read<byte>(Variables.ADDR_PlayerHP - 0x278, 0x08);
                var _donaldMPRead = Hypervisor.Read<byte>(Variables.ADDR_PlayerHP - 0x278 + 0x180, 0x08);

                var _goofyHPRead = Hypervisor.Read<byte>(Variables.ADDR_PlayerHP - 0x4F0, 0x08);
                var _goofyMPRead = Hypervisor.Read<byte>(Variables.ADDR_PlayerHP - 0x4F0 + 0x180, 0x08);

                var _soraFlag = _soraHPRead[0x00] == _soraHPRead[0x04] && _soraMPRead[0x00] == _soraMPRead[0x04];
                var _donaldFlag = _donaldHPRead[0x00] == _donaldHPRead[0x04] && _donaldMPRead[0x00] == _donaldMPRead[0x04];
                var _goofyFlag = _goofyHPRead[0x00] == _goofyHPRead[0x04] && _goofyMPRead[0x00] == _goofyMPRead[0x04];

                if (_soraFlag && _donaldFlag && _goofyFlag)
                    Hypervisor.Write<byte>(SAVE_OFFSET + 0x25B, 0x74);

                else
                    Hypervisor.Write<byte>(SAVE_OFFSET + 0x25B, 0x75);
            }
        }

        public static void ToggleWarpGOA()
        {
            var _flagRead = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0x1EF6);
            var _goaVisit = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0x231B);
            var _worldRead = Hypervisor.Read<byte>(Variables.ADDR_Area);

            if (_worldRead == 0x0F && (_goaVisit & 0x04) == 0x04 && (_flagRead & 0x40) == 0x00)
                Hypervisor.Write(Variables.ADDR_SaveData + 0x1EF6, (byte)(_flagRead + 0x40));

            else if (_worldRead != 0x0F && (_flagRead & 0x40) == 0x40)
                Hypervisor.Write(Variables.ADDR_SaveData + 0x1EF6, (byte)(_flagRead - 0x40));
        }

        public static void ToggleLimiter()
        {
            if (LIMITER_FUNCTION == null)
                LIMITER_FUNCTION = Hypervisor.Read<byte>(LIMITER_OFFSET, 0x06);

            var _fetchFramerate = Hypervisor.Read<byte>(Variables.ADDR_Framerate);
            var _fetchFunction = Hypervisor.Read<byte>(LIMITER_OFFSET);
            var _limiterInvert = (byte)(_fetchFramerate == 0x00 ? 0x01 : 0x00);

            if (_fetchFramerate == 0x00 && _fetchFunction == 0x90)
            {
                Terminal.Log("Toggling the framelimiter for 30FPS.", 0);
                Hypervisor.Write(LIMITER_OFFSET, LIMITER_FUNCTION);
            }

            else if (_fetchFramerate != 0x00 && _fetchFunction != 0x90)
            {
                Terminal.Log("Toggling the framelimiter for 60FPS.", 0);
                Hypervisor.DeleteInstruction(LIMITER_OFFSET, 0x06);
            }

            Hypervisor.Write(Variables.ADDR_Framelimiter, _limiterInvert);
        }

        public static void TogglePrompts()
        {
            var _promptCheck = Hypervisor.Read<byte>(PROMPT_OFFSET + 0x06) == 0x00 ? true : false;
            var _promptString = Variables.CONTROLLER_MODE ? "Manual" : "Automatic";

            if (_promptCheck != Variables.CONTROLLER_MODE)
            {
                Terminal.Log("Switching to " + _promptString + " Prompt Mode.", 0);
                Hypervisor.Write(PROMPT_OFFSET + 0x06, (byte)(Variables.CONTROLLER_MODE ? 0x00 : 0x01));
            }

            if (Variables.CONTROLLER_MODE)
            Hypervisor.Write(Variables.ADDR_ControllerMode, 0x00);
        }

        public static void ToggleLimitShortcuts()
        {
            var _modeRead = Hypervisor.Read<ushort>(Variables.ADDR_ControllerMode);
            var _shortRead = Hypervisor.Read<ushort>(Variables.ADDR_LimitShortcut);

            if (Variables.CONFIRM_BUTTON == Variables.BUTTON.CIRCLE && _shortRead != LIMIT_SHORT[3])
            {
                Terminal.Log("Overriding Limits for the Japanese scheme.", 0);

                Hypervisor.Write(Variables.ADDR_LimitShortcut, LIMIT_SHORT[3]);
                Hypervisor.Write(Variables.ADDR_LimitShortcut + 0x06, LIMIT_SHORT[0]);
            }

            else if (Variables.CONFIRM_BUTTON == Variables.BUTTON.CROSS && _shortRead != LIMIT_SHORT[0] && _modeRead == 0)
            {
                Terminal.Log("Overriding Limits for the English Scheme", 0);

                Hypervisor.Write(Variables.ADDR_LimitShortcut, LIMIT_SHORT[0]);
                Hypervisor.Write(Variables.ADDR_LimitShortcut + 0x06, LIMIT_SHORT[3]);
            }

            Hypervisor.Write(Variables.ADDR_LimitShortcut + 0x02, LIMIT_SHORT[1]);
            Hypervisor.Write(Variables.ADDR_LimitShortcut + 0x04, LIMIT_SHORT[2]);
        }

        public static void ToggleDiscord()
        {
            var _worldID = Hypervisor.Read<byte>(Variables.ADDR_Area);

            var _roomRead = Hypervisor.Read<byte>(Variables.ADDR_Area + 0x01);
            var _roundRead = Hypervisor.Read<byte>(Variables.ADDR_Area + 0x02);
            var _eventRead = Hypervisor.Read<ushort>(Variables.ADDR_Area + 0x04);

            var _healthValue = Hypervisor.Read<byte>(Variables.ADDR_PlayerHP);
            var _magicValue = Hypervisor.Read<byte>(Variables.ADDR_PlayerHP + 0x180);

            var _formValue = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0x3524);
            var _diffValue = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0x2498);
            var _levelValue = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0x24FF);

            var _timeValue = Math.Floor(Hypervisor.Read<int>(Variables.ADDR_SaveData + 0x2444) / 60F);

            var _timeHours = Math.Floor(_timeValue / 3600F);
            var _timeMinutes = Math.Floor((_timeValue % 3600F) / 60F);

            var _checkUnderdrome = _worldID == 0x06 && _roomRead == 0x09 && (_eventRead >= 0xBD && _eventRead <= 0xC4);

            if (RPC_TEXTS == null)
            {
                RPC_TEXTS = new List<string>();
                MODE_TEXTS = new List<string>();
                FORM_TEXTS = new List<string>();

                for (var i = 0; i < 5; i++)
                    RPC_TEXTS.Add(Operations.GetStringLiteral(Variables.PINT_SystemMSG, (ushort)(0x5740 + i)).FromKHSCII().Replace("/", "|"));

                for (var i = 0; i < 4; i++)
                {
                    var _stringID = (ushort)(0x3738 + i);

                    if (i == 0x03)
                        _stringID = 0x4E30;

                    MODE_TEXTS.Add(Operations.GetStringLiteral(Variables.PINT_SystemMSG, _stringID).FromKHSCII());
                }

                for (var i = 0; i < 6; i++)
                {
                    var _stringID = (ushort)(0x432A + (i >= 0x02 ? (i - 1) : i));

                    if (i == 0x02)
                        _stringID = 0x4E80;

                    FORM_TEXTS.Add(Operations.GetStringLiteral(Variables.PINT_SystemMSG, _stringID).FromKHSCII());
                }
            }

            var _detail = RPC_TEXTS[0]
                         .Replace("[0]", _healthValue.ToString())
                         .Replace("[1]", _magicValue > 0 ? _magicValue.ToString() : RPC_TEXTS[4]);

            var _state = RPC_TEXTS[1]
                        .Replace("[0]", _levelValue.ToString())
                        .Replace("[1]", _formValue == 0x00 ? "N/A" : FORM_TEXTS[_formValue - 0x01]);

            var _timeFormat = string.Format("{0}:{1}", _timeHours.ToString("00"), _timeMinutes.ToString("00"));
            var _time = RPC_TEXTS[3].Replace("[0]", _timeFormat);

            var _worldImage = Variables.DICTIONARY_WRL.ElementAtOrDefault(_worldID);

            if (_checkUnderdrome)
            {
                _state = RPC_TEXTS[2]
                        .Replace("[0]", _levelValue.ToString())
                        .Replace("[2]", _roundRead.ToString())
                        .Replace("[1]", FORM_TEXTS[_formValue]);

                _worldImage = Variables.DICTIONARY_CPS.ElementAtOrDefault(_eventRead < 0xC1 ? _eventRead - 0xBD : _eventRead - 0xC1);
            }

            if (!Variables.IS_TITLE)
            {
                Variables.DiscordClient.SetPresence(
                    new RichPresence
                    {
                        Details = _detail,
                        State = _state,
                        Assets = new Assets
                        {
                            LargeImageText = _time,
                            LargeImageKey = _worldImage,
                            SmallImageKey = Variables.DICTIONARY_BTL[(byte)Variables.BATTLE_MODE],
                            SmallImageText = MODE_TEXTS[_diffValue],
                        },
                    }
                );
            }

            else
            {
                Variables.DiscordClient.SetPresence(
                    new RichPresence
                    {
                        Details = null,
                        State = null,
                        Assets = new Assets
                        {
                            LargeImageKey = "title",
                            SmallImageKey = null,
                            SmallImageText = null
                        },
                    }
                );
            }
        }
        
    }
}
