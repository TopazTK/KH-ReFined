using DiscordRPC;
using ReFined.Common;
using ReFined.KH2.Information;
using ReFined.KH2.InGame;
using ReFined.Libraries;

namespace ReFined.KH2.Functions
{
    public static class Continuous
    {
        public static ulong PROMPT_OFFSET;
        public static ulong LIMITER_OFFSET;

        public static byte[]? LIMITER_FUNCTION = null;

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

            Hypervisor.Write(Variables.ADDR_ControllerMode, (byte)(Variables.CONTROLLER_MODE ? 0x00 : 0x01));
        }


        public static void ToggleDiscord()
        {
            // Read all the values.

            var _worldID = Hypervisor.Read<byte>(Variables.ADDR_Area);

            var _roomRead = Hypervisor.Read<byte>(Variables.ADDR_Area + 0x01);
            var _roundRead = Hypervisor.Read<byte>(Variables.ADDR_Area + 0x02);
            var _eventRead = Hypervisor.Read<ushort>(Variables.ADDR_Area + 0x04);

            var _formValue = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0x3524);
            var _healthValue = Hypervisor.Read<byte>(0x2A23598);
            var _magicValue = Hypervisor.Read<byte>(0x2A23718);

            var _battleFlag = Hypervisor.Read<byte>(Variables.ADDR_BattleFlag);
            var _diffValue = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0x2498);

            var _timeValue = Math.Floor(Hypervisor.Read<int>(Variables.ADDR_SaveData + 0x2444) / 60F);
            var _timeMinutes = Math.Floor((_timeValue % 3600F) / 60F);
            var _timeHours = Math.Floor(_timeValue / 3600F);

            // Construct the necessary strings.

            var _stringState = string.Format
            (
                "Level {0} | Form: {1}",
                Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0x24FF),
                Variables.DICTIONARY_FRM.ElementAtOrDefault(_formValue)
            );

            var _stringDetail = string.Format("HP: {0} | MP: {1}", _healthValue, _magicValue > 0 ? _magicValue : "Recharging!");
            var _timeText = string.Format("In-Game Time: {0}", string.Format("{0}:{1}", _timeHours.ToString("00"), _timeMinutes.ToString("00")));

            // If the game is NOT in the Title Screen, apply the detailed presence.
            if (!Variables.IS_TITLE)
            {
                if (_worldID == 0x06 && _roomRead == 0x09 && (_eventRead >= 0xBD && _eventRead <= 0xC4))
                {
                    Variables.DiscordClient.SetPresence(
                        new RichPresence
                        {
                            Details = _stringDetail,
                            State = _stringState + " | Round: " + _roundRead,
                            Assets = new Assets
                            {
                                LargeImageText = _timeText,

                                LargeImageKey = Variables.DICTIONARY_CPS.ElementAtOrDefault(_eventRead < 0xC1 ? _eventRead - 0xBD : _eventRead - 0xC1),
                                SmallImageText = Variables.DICTIONARY_MDE.ElementAtOrDefault(_diffValue),
                                SmallImageKey = Variables.DICTIONARY_BTL.ElementAtOrDefault(_battleFlag)
                            },
                        }
                    );
                }

                else
                {
                    Variables.DiscordClient.SetPresence(
                        new RichPresence
                        {
                            Details = _stringDetail,
                            State = _stringState,
                            Assets = new Assets
                            {
                                LargeImageText = _timeText,

                                LargeImageKey = Variables.DICTIONARY_WRL.ElementAtOrDefault(_worldID),
                                SmallImageText = Variables.DICTIONARY_MDE.ElementAtOrDefault(_diffValue),
                                SmallImageKey = Variables.DICTIONARY_BTL.ElementAtOrDefault(_battleFlag)
                            },
                        }
                    );
                }
            }

            // If the game is in the Title Screen, apply the simple presence.
            else
            {
                Variables.DiscordClient.SetPresence(
                    new RichPresence
                    {
                        Details = "On the Title Screen",
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
