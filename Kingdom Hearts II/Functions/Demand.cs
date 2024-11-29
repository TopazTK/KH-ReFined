using Octokit;
using System.Net;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;

using ReFined.Common;
using ReFined.Libraries;
using ReFined.KH2.InGame;
using ReFined.KH2.Information;

using BSharpConvention = Binarysharp.MSharp.Assembly.CallingConvention.CallingConventions;

namespace ReFined.KH2.Functions
{
    public static class Demand
    {
        public static IntPtr OffsetShortcutUpdate;

        public static ulong PROMPT_FUNCTION;

        public static int SKIP_STAGE;
        public static bool SKIP_ROXAS;

        public static ulong SORA_MSG_POINT;
        public static bool SORA_TEXT_SWITCH = false;

        static byte CURR_SHORTCUT = 0x00;
        static bool SHORTCUT_TOGGLE = false;

        static string LATEST_URL = "";
        static byte UPDATE_PHASE = 0x00;
        static bool UPDATE_AVAILABLE = false;
        static bool DOWNLOAD_STARTED = false;
        static bool UPDATE_TRIGGERED = false;

        static byte[] MAIN_TEXT = null;
        static byte[] SORA_TEXT = null;

        static bool[] DEBOUNCE = new bool[0x20];

        static byte[] UPDATE_TEXT = null;
        static ulong UPDATE_TEXT_ABSOLUTE;
        static int UPDATE_BAR_INDEX = 0x00;
        static byte[] UPDATE_DONE_TEXT = null;

        public static void TriggerAutoattack()
        {
            if (Variables.AUTOATTACK)
            {
                var _currAction = Hypervisor.Read<int>(Variables.ADDR_ActionExe);
                var _subCommand = Hypervisor.Read<ulong>(Variables.PINT_ChildMenu);
                var _partyLimit = Hypervisor.Read<ulong>(Variables.PINT_PartyLimit);
                var _currCommand = Hypervisor.Read<byte>(Hypervisor.GetPointer64(Variables.PINT_ChildMenu - 0x08, [0x74]), true);
                var _firstCommand = Hypervisor.Read<short>(Hypervisor.GetPointer64(Variables.PINT_ChildMenu - 0x08, [0x16]), true);
                var _mainCommandType = Hypervisor.Read<byte>(Hypervisor.GetPointer64(Variables.PINT_ChildMenu - 0x08, [0x00]), true);

                var _isActionGood = _subCommand == 0x00 && _partyLimit == 0x00 && _currCommand == 0x00;
                var _isCommandGood = (_firstCommand == 0x0001 || _firstCommand == 0x016D || _firstCommand == 0x0088) && (_mainCommandType == 0x00 || _mainCommandType == 0x06);
                var _isStatusGood = !Variables.IS_TITLE && !Variables.IS_CUTSCENE && Variables.BATTLE_MODE != Variables.BATTLE_TYPE.PEACEFUL;

                var _autoCheck = Variables.IS_PRESSED(Variables.CONFIRM_BUTTON) && _isActionGood && _isCommandGood && _isStatusGood;

                if (_autoCheck && _currAction == 0x00)
                    Hypervisor.Write(Variables.ADDR_ActionExe, 0x01);

                else if (!_autoCheck && _currAction == 0x01)
                    Hypervisor.Write(Variables.ADDR_ActionExe, 0x00);
            }
        }
       
        public static void TriggerReset()
        {
            var _currentTime = DateTime.Now;

            var _canReset = !Variables.IS_TITLE && Variables.IS_LOADED;

            if (Variables.IS_PRESSED(Variables.RESET_COMBO) && _canReset && !DEBOUNCE[0])
            {
                Terminal.Log("Soft Reset requested.", 0);
                DEBOUNCE[0] = true;

                if (Variables.RESET_PROMPT)
                {
                    Terminal.Log("Soft Reset Prompt enabled. Showing prompt.", 0);

                    Popups.PopupPrize(0x0100);
                    var _cancelRequest = false;

                    Task.Factory.StartNew(() =>
                    {
                        Terminal.Log("Waiting 2 seconds before execution.", 0);

                        while ((DateTime.Now - _currentTime) < TimeSpan.FromMilliseconds(2000))
                        {
                            if (Variables.IS_PRESSED(Variables.CONFIRM_BUTTON))
                            {
                                Terminal.Log("Soft Reset interrupted.", 0);
                                Popups.PopupPrize(0x0101);
                                _cancelRequest = true;
                                DEBOUNCE[0] = false;
                                break;
                            };
                        }

                        if (!_cancelRequest)
                        {
                            Hypervisor.Write<byte>(Variables.ADDR_Reset, 0x01);
                            Terminal.Log("Soft Reset executed.", 0);
                            DEBOUNCE[0] = false;
                        }
                    });
                }

                else
                {
                    Hypervisor.Write<byte>(Variables.ADDR_Reset, 0x01);
                    Terminal.Log("Soft Reset executed.", 0);
                    DEBOUNCE[0] = false;
                }
            }
        }

        public static void TriggerEncounter()
        {
            var _roomPoint = Hypervisor.Read<ulong>(Variables.PINT_EnemyInfo) + 0x08;
            var _abilityRead = Hypervisor.Read<ushort>(Variables.ADDR_SaveData + 0x2544, 0x60);

            if (!Variables.IS_TITLE && Variables.IS_LOADED && !DEBOUNCE[2] && !_abilityRead.Contains<ushort>(0x80F8) && !_abilityRead.Contains<ushort>(0x00F8))
            {
                var _fetchIndex = Array.FindIndex(_abilityRead, x => x == 0x0000);

                Terminal.Log("Encounter Plus has been added to the inventory!", 0);

                Hypervisor.Write<ushort>(Variables.ADDR_SaveData + 0x2544 + (ulong)(_fetchIndex * 0x02), 0x00F8);
                DEBOUNCE[2] = true;
            }

            else if (Variables.IS_TITLE && DEBOUNCE[2])
                DEBOUNCE[2] = false;

            if (!Variables.IS_LOADED && _abilityRead.Contains<ushort>(0x80F8) && !DEBOUNCE[3] && Critical.AREA_READ == null)
            {
                Terminal.Log("Enemy data has been cleared!", 0);
                Hypervisor.Write(_roomPoint, new byte[0x100], true);
                DEBOUNCE[3] = true;
            }

            else if (Variables.IS_LOADED && DEBOUNCE[3])
                DEBOUNCE[3] = false;
        }

        public static void TriggerPrologueSkip()
        {
            var _diffRead = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0x2498);
            var _selectButton = Hypervisor.Read<byte>(Variables.ADDR_TitleSelect);

            if (Variables.IS_TITLE)
            {
                if (_selectButton == 0x01 && SKIP_STAGE == 0)
                {
                    Terminal.Log("Loaded game detected! Disabling Roxas Skip...", 0);
                    SKIP_STAGE = 3;
                }

                else if (_selectButton == 0x00 && SKIP_STAGE > 0x00)
                {
                    Terminal.Log("Loaded game abandoned. Re-enabling Roxas Skip!", 0);
                    SKIP_STAGE = 0;
                }
            }

            if (!Variables.IS_TITLE)
            {
                var _worldCheck = Hypervisor.Read<byte>(Variables.ADDR_Area);
                var _roomCheck = Hypervisor.Read<byte>(Variables.ADDR_Area + 0x01);
                var _eventCheck = Hypervisor.Read<byte>(Variables.ADDR_Area + 0x04);

                var _cutsceneMode = Hypervisor.Read<byte>(Variables.ADDR_CutsceneMode);

                if (_worldCheck == 0x02 && _roomCheck == 0x01 && _eventCheck == 0x38 && SKIP_STAGE == 0)
                {
                    if (SKIP_ROXAS)
                    {
                        Terminal.Log("Room and Settings are correct! Initiating Roxas Skip's First Phase...", 0);

                        Critical.LOCK_AUTOSAVE = true;

                        Hypervisor.Write(Variables.ADDR_Area, 0x322002);
                        Hypervisor.Write(Variables.ADDR_Area + 0x04, 0x01);
                        Hypervisor.Write(Variables.ADDR_Area + 0x08, 0x01);

                        while (Hypervisor.Read<byte>(Variables.ADDR_FadeValue) != 0x80) ;

                        Hypervisor.DeleteInstruction((ulong)(Critical.OffsetSetFadeOff + 0x81A), 0x08);
                        Hypervisor.Write<byte>(Variables.ADDR_FadeValue, 0x80);

                        while (!Variables.IS_LOADED) ;

                        Variables.SharpHook[Critical.OffsetMapJump].Execute(BSharpConvention.MicrosoftX64, (long)(Hypervisor.PureAddress + Variables.ADDR_Area), 2, 0, 0, 0);

                        while (!Variables.IS_LOADED) ;

                        Hypervisor.Write<byte>((ulong)(Critical.OffsetSetFadeOff + 0x81A), [0xF3, 0x0F, 0x11, 0x8F, 0x0C, 0x01, 0x00, 0x00]);

                        Hypervisor.Write(Variables.ADDR_SaveData + 0x1CD0, 0x1FF00001);
                        Hypervisor.Write(Variables.ADDR_SaveData + 0x1CD4, 0x00000000);

                        SKIP_STAGE = 1;
                    }

                    else
                    {
                        Terminal.Log("Room is correct but settings are not! Disabling Roxas Skip...", 0);
                        SKIP_STAGE = 3;
                    }
                }

                else if (_worldCheck == 0x02 && _roomCheck == 0x20 && _eventCheck == 0x01 && _cutsceneMode != 0x00 && SKIP_STAGE == 1)
                {
                    Terminal.Log("Room parameters correct! Skip was initiated! Initiating Roxas Skip's Second Phase...", 0);

                    Hypervisor.Write<uint>(Variables.ADDR_Area, 0x00320E02);
                    Hypervisor.Write<uint>(Variables.ADDR_Area + 0x04, 0x02);
                    Hypervisor.Write<uint>(Variables.ADDR_Area + 0x08, 0x12);

                    while (!Variables.IS_LOADED) ;

                    Variables.SharpHook[Critical.OffsetMapJump].Execute(BSharpConvention.MicrosoftX64, (long)(Hypervisor.PureAddress + Variables.ADDR_Area), 2, 0, 0, 0);

                    Hypervisor.Write(
                        Variables.ADDR_SaveData + 0x31C,
                        new byte[]
                        {
                           0x04,
                           0x00,
                           0x00,
                           0x00,
                           0x00,
                           0x00,
                           0x04,
                           0x00,
                           0x00,
                           0x00,
                           0x05,
                           0x00,
                           0x04,
                           0x00,
                           0x00,
                           0x00,
                           0x12,
                           0x00,
                           0x00,
                           0x00,
                           0x00,
                           0x00,
                           0x00,
                           0x00,
                           0x04,
                           0x00,
                           0x00,
                           0x00,
                           0x12,
                           0x00,
                           0x04,
                           0x00,
                           0x15,
                           0x00,
                           0x12,
                           0x00,
                           0x04,
                           0x00,
                           0x00,
                           0x00,
                           0x12,
                           0x00,
                           0x04,
                           0x00,
                           0x00,
                           0x00,
                           0x12,
                           0x00,
                           0x00,
                           0x00,
                           0x00,
                           0x00,
                           0x12,
                           0x00,
                           0x04,
                           0x00,
                           0x00,
                           0x00,
                           0x12,
                           0x00,
                           0x00,
                           0x00,
                           0x00,
                           0x00,
                           0x12,
                           0x00,
                           0x00,
                           0x00,
                           0x00,
                           0x00,
                           0x12,
                           0x00,
                           0x02,
                           0x00,
                           0x00,
                           0x00,
                           0x12
                        });

                    Hypervisor.Write(Variables.ADDR_SaveData + 0x03E8, 0x04);
                    Hypervisor.Write(Variables.ADDR_SaveData + 0x03EE, 0x04);
                    Hypervisor.Write(Variables.ADDR_SaveData + 0x1CE2, 0x67);

                    Hypervisor.Write(Variables.ADDR_SaveData + 0x20E4, 0xCB75CB74);
                    Hypervisor.Write(Variables.ADDR_SaveData + 0x20F4, 0x9F609F4D);
                    Hypervisor.Write(Variables.ADDR_SaveData + 0x2120, 0xC54BC54A);

                    Hypervisor.Write(Variables.ADDR_SaveData + 0x23EE, 0x01);

                    Hypervisor.Write(Variables.ADDR_SaveData + 0x2444, 0x098A);
                    Hypervisor.Write(Variables.ADDR_SaveData + 0x2454, 0x0989);

                    Hypervisor.Write(Variables.ADDR_SaveData + 0x353D, 0x00120201);
                    Hypervisor.Write(Variables.ADDR_SaveData + 0x3FF5, 0x00000107);
                    Hypervisor.Write(Variables.ADDR_SaveData + 0x41A8, 0x0A);
                    Hypervisor.Write(Variables.ADDR_SaveData + 0x41AD, 0x0202);
                    Hypervisor.Write(Variables.ADDR_SaveData + 0x41B1, 0x08);

                    Hypervisor.Write(
                        Variables.ADDR_SaveData + 0x4278,
                        new byte[]
                        {
                            0xF8,
                            0x00,
                            0x89,
                            0x01,
                            0x88,
                            0x01,
                            0xA5,
                            0x01,
                            0x94,
                            0x01,
                            0x97,
                            0x01,
                            0x95,
                            0x01,
                            0x52,
                            0x00,
                            0x8A,
                            0x00,
                            0x9E,
                            0x00,
                            0x00,
                            0x00,
                            0x00,
                            0x00,
                            0x00,
                            0x00,
                        });

                    Hypervisor.Write(Variables.ADDR_SaveData + 0x4318, 0x00A800A7);
                    Hypervisor.Write(Variables.ADDR_SaveData + 0x4378, 0x019B01AD);
                    Hypervisor.Write(Variables.ADDR_SaveData + 0x43D8, 0x008A00A3);

                    Hypervisor.Write(
                        Variables.ADDR_SaveData + 0x4438,
                        new byte[]
                        {
                            0xCC,
                            0x00,
                            0xAC,
                            0x01,
                            0xB0,
                            0x00,
                            0xA1,
                            0x01,
                            0x9C,
                            0x01,
                            0x9D,
                            0x01,
                            0xA0,
                            0x01
                        });

                    Hypervisor.Write(Variables.ADDR_SaveData + 0x4498, 0x0195019B);

                    Hypervisor.Write(Variables.ADDR_SaveData + 0x44F8,
                        new short[]
                        {
                            0x00CB,
                            0x01AA,
                            0x01AB,
                            0x01A1,
                            0x019B,
                            0x0196,
                            0x019D,
                            0x01A0,
                            0x01A2
                        });

                    Hypervisor.Write(Variables.ADDR_SaveData + 0x4558,
                        new short[]
                        {
                            0x00D2,
                            0x01BE,
                            0x01BF,
                            0x01C0,
                            0x01A1,
                            0x019B,
                            0x0195,
                            0x0197,
                            0x019E,
                            0x01A4
                        });

                    Hypervisor.Write(Variables.ADDR_SaveData + 0x45B8,
                        new short[]
                        {
                            0x00CD,
                            0x00B1,
                            0x01AE,
                            0x01A1,
                            0x019B,
                            0x019F,
                            0x019E,
                            0x01A3
                        });

                    Hypervisor.Write(Variables.ADDR_SaveData + 0x4618,
                       new short[]
                       {
                            0x00CE,
                            0x00AF,
                            0x01B0,
                            0x01B1,
                            0x01A1,
                            0x01A5,
                            0x01A4,
                            0x0197,
                            0x0198,
                            0x0199,
                            0x019A
                       });

                    Hypervisor.Write(Variables.ADDR_SaveData + 0x4678,
                       new short[]
                       {
                            0x00D1,
                            0x01B7,
                            0x01B8,
                            0x00BE,
                            0x01A1,
                            0x019C,
                            0x019E,
                            0x01A3,
                            0x01A4
                       });

                    Hypervisor.Write(Variables.ADDR_SaveData + 0x46D8,
                       new short[]
                       {
                            0x019B,
                            0x0196,
                            0x01A2
                       });

                    Hypervisor.Write(Variables.ADDR_SaveData + 0x4738,
                       new short[]
                       {
                            0x00D0,
                            0x01B6,
                            0x01B4,
                            0x00BB,
                            0x01A1,
                            0x019B,
                            0x019E,
                            0x019F,
                            0x01A0,
                            0x01A6,
                            0x01A3
                       });

                    Hypervisor.Write(Variables.ADDR_SaveData + 0x4798,
                       new short[]
                       {
                            0x005E,
                            0x00D8,
                            0x00D9,
                            0x00DA,
                            0x00DB,
                            0x00F6,
                            0x00F7,
                            0x0111,
                            0x00DF,
                            0x00A2,
                            0x00A3
                       });

                    Hypervisor.Write(Variables.ADDR_SaveData + 0x47F8,
                       new short[]
                       {
                            0x0062,
                            0x00DC,
                            0x00DD,
                            0x00E0,
                            0x00E1,
                            0x0111,
                            0x01A6
                       });

                    Hypervisor.Write(Variables.ADDR_SaveData + 0x4858,
                       new short[]
                       {
                            0x0234,
                            0x0239,
                            0x023A,
                            0x023B,
                            0x023C,
                            0x023D,
                            0x023E,
                            0x023F,
                            0x024B,
                            0x024C,
                            0x024D,
                            0x0052,
                            0x0106,
                            0x0108,
                            0x010D,
                            0x019C,
                            0x0195,
                            0x0197,
                            0x019D
                       });

                    Hypervisor.Write(Variables.ADDR_SaveData + 0x048B8,
                       new short[]
                       {
                            0x0066,
                            0x0101,
                            0x0102,
                            0x0105,
                            0x00DF,
                            0x0103,
                            0x01A5,
                            0x00A3,
                            0x0195
                       });

                    Hypervisor.Write(Variables.ADDR_SaveData + 0x4918,
                       new short[]
                       {
                            0x006A,
                            0x0207,
                            0x00DD,
                            0x00DF,
                            0x020F,
                            0x0210,
                            0x0211,
                            0x0212,
                            0x019D
                       });

                    Hypervisor.Write(Variables.ADDR_SaveData + 0x4978,
                       new short[]
                       {
                            0x00A2,
                            0x00A3,
                            0x0208,
                            0x0209,
                            0x020A,
                            0x020B
                       });

                    Hypervisor.Write<byte>(Variables.ADDR_SaveData + 0x239E, 0x9F);
                    Hypervisor.Write(
                        Variables.ADDR_SaveData + 0x1CD0,
                        new byte[]
                        {
                            0x01,
                            0x00,
                            0xF0,
                            0xFF,
                            0xFF,
                            0xFF,
                            0xFF,
                            0xFF,
                            0xFF,
                            0xFF,
                            0xFF,
                            0xDB,
                            0xFF,
                            0xFF,
                            0xFF,
                            0xFF,
                            0xFF,
                            0xFF,
                            0x07,
                            0x00,
                            0x00,
                            0x00,
                            0x00,
                            0x00,
                            0x00,
                            0xD0,
                            0x05,
                            0x08,
                            0x01,
                            0x00,
                            0x00,
                            0x81
                        });

                    if (_diffRead == 0x03)
                    {
                        Hypervisor.Write<byte>(Variables.ADDR_SaveData + 0x24F4, 0x18);
                        Hypervisor.Write<byte>(Variables.ADDR_SaveData + 0x24F4 + 0x01, 0x18);
                        Hypervisor.Write(
                            Variables.ADDR_SaveData + 0x2544,
                            new byte[]
                            {
                                0xF8,
                                0x00,
                                0x89,
                                0x01,
                                0x88,
                                0x01,
                                0xA5,
                                0x01,
                                0x94,
                                0x01,
                                0x97,
                                0x01,
                                0x97,
                                0x01,
                                0x95,
                                0x01,
                                0x52,
                                0x00,
                                0x8A,
                                0x00,
                                0x9E,
                                0x00
                            }
                        );
                    }

                    else
                    {
                        Hypervisor.Write<byte>(Variables.ADDR_SaveData + 0x24F4, 0x1E);
                        Hypervisor.Write<byte>(Variables.ADDR_SaveData + 0x24F4 + 0x01, 0x1E);
                        Hypervisor.Write(
                            Variables.ADDR_SaveData + 0x2544,
                            new byte[] { 0x52, 0x00, 0x8A, 0x00, 0x9E, 0x00 }
                        );
                    }

                    Hypervisor.Write<byte>(Variables.ADDR_SaveData + 0x3700, 0x04);

                    Hypervisor.Write<byte>(Variables.ADDR_SaveData + 0x3708, 0x06);
                    Hypervisor.Write<byte>(Variables.ADDR_SaveData + 0x370A, 0x40);
                    Hypervisor.Write<byte>(Variables.ADDR_SaveData + 0x370D, 0x02);

                    Hypervisor.Write<byte>(Variables.ADDR_SaveData + 0x36C7, 0x80);
                    Hypervisor.Write<byte>(Variables.ADDR_SaveData + 0x36C9, 0x80);

                    Critical.LOCK_AUTOSAVE = false;

                    Terminal.Log("Roxas Skip has been completed!", 0);
                    SKIP_STAGE = 2;
                }
            }
        }

        public static void TriggerShortcut()
        {
            var _menuPointer = Hypervisor.GetPointer64(Variables.PINT_ChildMenu - 0x08);
            var _menuType = Hypervisor.Read<byte>(_menuPointer, true);

            var _isPaused = Hypervisor.Read<byte>(Variables.ADDR_PauseFlag);
            var _subMenuType = Hypervisor.Read<byte>(Variables.ADDR_SubMenuType);

            var _currShort = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0xE000);
            var _currForm = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0x3524);

            var _shortReal = Variables.ADDR_SaveData + 0x36F8;
            var _shortFake = Variables.ADDR_SaveData + 0xE100;

            if (SORA_MSG_POINT == 0x00)
                SORA_MSG_POINT = Operations.GetStringPointer(Variables.PINT_SystemMSG, 0x051F);

            var _isInMainShortcut = _isPaused == 0x00 && _subMenuType == 0x19;
            var _isEditingShortcut = _isPaused == 0x00 && (_subMenuType == 0x1A || _subMenuType == 0x1D || _subMenuType == 0x1E || _subMenuType == 0x1F);
            var _isInShortcut = _isPaused == 0x00 && (_subMenuType == 0x19 || _subMenuType == 0x1A || _subMenuType == 0x1D || _subMenuType == 0x1E || _subMenuType == 0x1F);

            if (MAIN_TEXT == null)
            {
                SORA_TEXT = Operations.GetStringLiteral(Variables.PINT_SystemMSG, 0x012E);
                MAIN_TEXT = Operations.GetStringLiteral(Variables.PINT_SystemMSG, 0x051F);
            }

            if (!Variables.IS_TITLE && !Variables.IS_LITE)
            {
                if (_isEditingShortcut && !SHORTCUT_TOGGLE)
                    SHORTCUT_TOGGLE = true;

                else if ((!_isEditingShortcut && SHORTCUT_TOGGLE) || (Hypervisor.Read<short>(_shortFake) == 0x0000 && Hypervisor.Read<short>(_shortReal) != 0x0000))
                {
                    Terminal.Log("Submitting Shortcut Menu " + Char.ConvertFromUtf32(0x41 + CURR_SHORTCUT) + "!", 0x00);
                    var _shortTake = Hypervisor.Read<byte>(_shortReal, 0x08);
                    Hypervisor.Write(_shortFake + (0x08U * CURR_SHORTCUT), _shortTake);
                    SHORTCUT_TOGGLE = false;
                }

                if (!_isInShortcut && !SORA_TEXT_SWITCH)
                {
                    Hypervisor.Write(SORA_MSG_POINT, SORA_TEXT, true);
                    SORA_TEXT_SWITCH = true;
                }

                else if (_isInShortcut)
                {
                    MAIN_TEXT[MAIN_TEXT.Length - 0x02] = (byte)(0x2E + CURR_SHORTCUT);
                    Hypervisor.Write(SORA_MSG_POINT, MAIN_TEXT, true);
                    SORA_TEXT_SWITCH = false;
                }

                if (_menuType != 0x05 || !Variables.IS_PRESSED(Variables.BUTTON.DOWN) && !Variables.IS_PRESSED(Variables.BUTTON.UP))
                    DEBOUNCE[1] = false;

                if (!_isInMainShortcut || !Variables.IS_PRESSED(Variables.BUTTON.L1) && !Variables.IS_PRESSED(Variables.BUTTON.R1))
                    DEBOUNCE[4] = false;

                if (_menuType == 0x05 && !DEBOUNCE[1] && _currForm != 0x03 && _currForm != 0x06)
                {
                    if (Variables.IS_PRESSED(Variables.BUTTON.DOWN) && !DEBOUNCE[1])
                    {
                        Sound.PlaySFX(0x14);
                        CURR_SHORTCUT++;
                        DEBOUNCE[1] = true;
                    }

                    if (Variables.IS_PRESSED(Variables.BUTTON.UP) && !DEBOUNCE[1])
                    {
                        Sound.PlaySFX(0x14);
                        CURR_SHORTCUT--;
                        DEBOUNCE[1] = true;
                    }
                }

                if (_isInMainShortcut && !DEBOUNCE[4] && _currForm != 0x03 && _currForm != 0x06)
                {
                    if (Variables.IS_PRESSED(Variables.BUTTON.R1) && !DEBOUNCE[4])
                    {
                        Sound.PlaySFX(0x02);
                        CURR_SHORTCUT++;
                        DEBOUNCE[4] = true;
                    }

                    if (Variables.IS_PRESSED(Variables.BUTTON.L1) && !DEBOUNCE[4])
                    {
                        Sound.PlaySFX(0x02);
                        CURR_SHORTCUT--;
                        DEBOUNCE[4] = true;
                    }
                }

                if (_menuType == 0x05 && (_currForm == 0x03 || _currForm == 0x06))
                {
                    if ((Variables.IS_PRESSED(Variables.BUTTON.DOWN) || Variables.IS_PRESSED(Variables.BUTTON.UP)) && !DEBOUNCE[1])
                    {
                        Sound.PlaySFX(0x05);
                        DEBOUNCE[4] = true;
                    }
                }

                switch (CURR_SHORTCUT)
                {
                    case 0x03:
                        CURR_SHORTCUT = 0x00;
                        break;

                    case 0xFF:
                        CURR_SHORTCUT = 0x02;
                        break;
                }

                if (CURR_SHORTCUT != _currShort && (DEBOUNCE[1] || DEBOUNCE[4]))
                {
                    Terminal.Log("Swapping to Shortcut Menu " + Char.ConvertFromUtf32(0x41 + CURR_SHORTCUT) + "!", 0x00);

                    var _shortTake = Hypervisor.Read<byte>(_shortFake + (0x08U * CURR_SHORTCUT), 0x08);

                    Hypervisor.Write(_shortReal, _shortTake);
                    Hypervisor.Write(Variables.ADDR_SaveData + 0xE000, CURR_SHORTCUT);

                    if (_isInShortcut)
                        Variables.SharpHook[OffsetShortcutUpdate].Execute();
                }
            }

            else if (!Variables.IS_TITLE && Variables.IS_LITE && !SORA_TEXT_SWITCH)
            {
                Hypervisor.Write(SORA_MSG_POINT, SORA_TEXT, true);
                SORA_TEXT_SWITCH = true;
            }
        }

        public static void TriggerUpdate()
        {
            if (LATEST_URL == "")
            {
                try
                {
                    var _gitClient = new GitHubClient(new ProductHeaderValue("ReFined-UpdateAgent"));
                    var _latestInfo = _gitClient.Repository.Release.GetAll("KH-ReFined", "KH-ReFined").Result[0];

                    var _latestNumber = Convert.ToDouble(_latestInfo.TagName.Substring(1), CultureInfo.InvariantCulture);
                    var _nameVersion = "[v{0}].exe";

                    var _exeAssembly = Assembly.GetExecutingAssembly();

                    if (_latestNumber > Variables.VERSION)
                        UPDATE_AVAILABLE = true;

                    LATEST_URL = _latestInfo.Assets.FirstOrDefault(x => x.Name.Contains(Variables.PLATFORM)).BrowserDownloadUrl;
                }

                catch (Exception EX)
                {
                    LATEST_URL = "NONE";
                }
            }

            if (!UPDATE_TRIGGERED && UPDATE_AVAILABLE)
            {
                var _downPath = Path.GetTempPath() + "reFinedUpdate.zip";
                var _exePath = Assembly.GetExecutingAssembly().Location;

                var _isPaused = Hypervisor.Read<byte>(Variables.ADDR_PauseFlag) == 0x00 ? true : false;
                var _menuType = Hypervisor.Read<byte>(Variables.ADDR_MenuType);

                UPDATE_TEXT_ABSOLUTE = Operations.GetStringPointer(Variables.PINT_SystemMSG, 0x0129);

                if (!Variables.IS_TITLE && Variables.IS_LOADED)
                {
                    Critical.LOCK_AUTOSAVE = true;

                    if (!_isPaused && _menuType != 0x08)
                    {
                        Thread.Sleep(750);
                        Popups.PopupMenu(0, 0);
                        Thread.Sleep(750);
                    }

                    else if (_isPaused && _menuType == 0x08)
                    {
                        var _inputRead = Hypervisor.Read<ushort>(Variables.ADDR_Input);

                        var _confirmButton = Hypervisor.Read<byte>(Variables.ADDR_Confirm) == 0x01 ? 0x2000 : 0x4000;
                        var _denyButton = Hypervisor.Read<byte>(Variables.ADDR_Confirm) == 0x01 ? 0x4000 : 0x2000;

                        var _selectRead = Hypervisor.Read<byte>(0x902521);

                        var _isConfirming = (_inputRead & _confirmButton) == _confirmButton;
                        var _isDenying = (_inputRead & _denyButton) == _denyButton;

                        if (UPDATE_DONE_TEXT == null)
                        {
                            UPDATE_DONE_TEXT = Operations.GetStringLiteral(Variables.PINT_SystemMSG, 0x012A);
                            UPDATE_TEXT = Operations.GetStringLiteral(Variables.PINT_SystemMSG, 0x0129);

                            UPDATE_BAR_INDEX = UPDATE_TEXT.ToList().FetchIndexOf(x => x == 0x87);

                            var _tempText = new List<byte>();

                            _tempText.AddRange(UPDATE_TEXT.Take(UPDATE_BAR_INDEX));
                            _tempText.AddRange(Enumerable.Repeat((byte)0x6B, 20).ToArray());
                            _tempText.AddRange(UPDATE_TEXT.Skip(UPDATE_BAR_INDEX + 20));

                            UPDATE_TEXT = _tempText.ToArray();
                            Hypervisor.Write(UPDATE_TEXT_ABSOLUTE, UPDATE_TEXT, true);
                        }

                        if (UPDATE_PHASE == 0x00)
                        {
                            var _result = Dialogs.ShowDialogCAMP(0x0128, Dialogs.DIALOG_BUTTONS.YES_NO_BUTTON);

                            if (_result)
                                UPDATE_PHASE = 0x01;

                            if (!_result)
                            {
                                UPDATE_TRIGGERED = true;
                                UPDATE_PHASE = 0;
                            }
                        }

                        if (UPDATE_PHASE == 0x01 && !DOWNLOAD_STARTED)
                        {
                            using (var _client = new WebClient())
                            {
                                _client.DownloadProgressChanged += DownloadEvent;
                                _client.DownloadFileAsync(new Uri(LATEST_URL), _downPath);

                                Variables.SharpHook[Dialogs.FUNC_SETCAMPWARNING].ExecuteJMP(BSharpConvention.MicrosoftX64, 0x0129, 0x0000);

                                DOWNLOAD_STARTED = true;
                            }
                        }

                        if (UPDATE_PHASE == 0x02)
                        {
                            using (var _zipArch = ZipFile.OpenRead(_downPath))
                            {
                                var _entryList = _zipArch.Entries;

                                var _exeBase = AppDomain.CurrentDomain.BaseDirectory;
                                var _exeName = "KINGDOM HEARTS II FINAL MIX.exe";

                                var _dateStr = DateTime.Now.ToString("dd_MM_yyyy");

                                var _realName = Path.Combine(_exeBase, _exeName);
                                var _backName = Path.Combine(_exeBase + "/BACKUP_EXE", _exeName).Replace(".exe", "_" + _dateStr + ".exe");

                                if (!Directory.Exists(_exeBase + "BACKUP_EXE/"))
                                    Directory.CreateDirectory(_exeBase + "BACKUP_EXE/");

                                File.Move(_realName, _backName);

                                foreach (var _file in _entryList)
                                    if (!File.Exists(Path.Combine(_exeBase, _file.Name)))
                                        _file.ExtractToFile(_file.Name, true);

                                UPDATE_PHASE = 0x03;
                            }
                        }

                        if (UPDATE_PHASE == 0x03 && (_isConfirming || _isDenying))
                        {
                            File.Delete(_downPath);
                            Process.Start(_exePath);
                            UPDATE_TRIGGERED = true;
                            Critical.LOCK_AUTOSAVE = false;
                            Process.GetCurrentProcess().Kill();
                        }
                    }
                }
            }
        }

        public static void TriggerMare()
        {
            if (Variables.PLATFORM == "EPIC")
            {
                var _isMare = Hypervisor.Read<byte>(Variables.ADDR_Mare);

                var _isMenu = Hypervisor.Read<byte>(Variables.ADDR_MenuFlag);
                var _typeMenu = Hypervisor.Read<byte>(Variables.ADDR_MenuType);

                var _readInstruction = Hypervisor.Read<byte>(0x103A80 + 0x1C, 0x06);

                var _titleOkay = Variables.IS_TITLE && _isMenu == 0x00;
                var _menuOkay = !Variables.IS_TITLE && _isMenu == 0x01 && _typeMenu == 0x08;

                if (Variables.IS_PRESSED(Variables.MARE_SHORTCUT) && (_titleOkay || _menuOkay))
                {
                    Hypervisor.DeleteInstruction(0x103A80 + 0x1C, 0x06);
                    Thread.Sleep(10);
                    Hypervisor.Write(0x103A80 + 0x1C, _readInstruction);
                }
            }
        }

        static void DownloadEvent(object sender, DownloadProgressChangedEventArgs e)
        {
            var _downProgress = Math.Floor(e.ProgressPercentage / 5D) >= 20 ? 19 : Math.Floor(e.ProgressPercentage / 5D);

            Hypervisor.Write(UPDATE_TEXT_ABSOLUTE + (ulong)UPDATE_BAR_INDEX + (ulong)_downProgress, (byte)0x6A, true);
            Thread.Sleep(250);

            if (e.ProgressPercentage == 100)
            {
                Hypervisor.Write(UPDATE_TEXT_ABSOLUTE, UPDATE_DONE_TEXT, true);

                Variables.SharpHook[Dialogs.FUNC_SHOWCAMPWARNING].Execute();
                Variables.SharpHook[Dialogs.FUNC_SETMENUMODE].Execute(BSharpConvention.MicrosoftX64, 0x04, 0x00);
                Variables.SharpHook[Dialogs.FUNC_SETMENUMODE + 0x40].Execute();

                UPDATE_PHASE = 0x02;
            }
        }
    }
}