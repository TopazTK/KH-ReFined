using ReFined.Common;
using ReFined.Libraries;
using ReFined.KH2.InGame;
using ReFined.KH2.Information;
using BSharpConvention = Binarysharp.MSharp.Assembly.CallingConvention.CallingConventions;
using System.Linq;

namespace ReFined.KH2.Functions
{
    public static class Demand
    {
        public static IntPtr OffsetShortcutUpdate;

        public static ulong PROMPT_FUNCTION;

        public static int SKIP_STAGE;
        public static bool SKIP_ROXAS;

        static byte CURR_SHORTCUT = 0x00;
        static bool SHORTCUT_TOGGLE = false;

        static byte UPDATE_PHASE = 0x00;
        static bool UPDATE_TRIGGERED = false;

        static byte[] MAIN_TEXT = null;

        static bool[] DEBOUNCE = new bool[0x20];

        static byte[] UPDATE_TEXT = null;
        static byte[] UPDATE_DONE_TEXT = null;
          
        static int UPDATE_BAR_INDEX = 0x00;
        static int UPDATE_PRG_INDEX = 0x00;

        public static void TriggerReset()
        {
            var _currentTime = DateTime.Now;

            var _buttonRead = Hypervisor.Read<ushort>(Variables.ADDR_Input);
            var _confirmRead = Hypervisor.Read<ushort>(Variables.ADDR_Confirm);

            var _loadRead = Hypervisor.Read<byte>(Variables.ADDR_LoadFlag);

            var _canReset = !Variables.IS_TITLE && _loadRead == 0x01;

            if (_buttonRead == Variables.RESET_COMBO && _canReset && !DEBOUNCE[0])
            {
                Terminal.Log("Soft Reset requested.", 0);
                DEBOUNCE[0] = true;

                if (Variables.RESET_PROMPT)
                {
                    Terminal.Log("Soft Reset Prompt enabled. Showing prompt.", 0);

                    Message.ShowSmallObtained(0x0100);
                    var _cancelRequest = false;

                    Task.Factory.StartNew(() =>
                    {
                        Terminal.Log("Waiting 2 seconds before execution.", 0);

                        while ((DateTime.Now - _currentTime) < TimeSpan.FromMilliseconds(2000))
                        {
                            var _buttonSeek = (_confirmRead == 0x01 ? 0x2000 : 0x4000);
                            var _buttonSecond = Hypervisor.Read<ushort>(Variables.ADDR_Input);

                            if ((_buttonSecond & _buttonSeek) == _buttonSeek)
                            {
                                Terminal.Log("Soft Reset interrupted.", 0);
                                Message.ShowSmallObtained(0x0101);
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

            var _roomRead = Hypervisor.Read<byte>(Variables.ADDR_LoadFlag);
            var _abilityRead = Hypervisor.Read<ushort>(Variables.ADDR_SaveData + 0x2544, 0x60);

            if (!Variables.IS_TITLE && _roomRead == 0x01 && !DEBOUNCE[2] && !_abilityRead.Contains<ushort>(0x80F8) && !_abilityRead.Contains<ushort>(0x00F8))
            {
                var _fetchIndex = Array.FindIndex(_abilityRead, x => x == 0x0000);

                Terminal.Log("Encounter Plus has been added to the inventory!", 0);

                Hypervisor.Write<ushort>(Variables.ADDR_SaveData + 0x2544 + (ulong)(_fetchIndex * 0x02), 0x00F8);
                DEBOUNCE[2] = true;
            }

            else if (Variables.IS_TITLE && DEBOUNCE[2])
                DEBOUNCE[2] = false;

            if (_roomRead == 0x00 && _abilityRead.Contains<ushort>(0x80F8) && !DEBOUNCE[3] && Critical.AREA_READ == null)
            {
                Terminal.Log("Enemy data has been cleared!", 0);
                Hypervisor.Write(_roomPoint, new byte[0x100], true);
                DEBOUNCE[3] = true;
            }

            else if (_roomRead == 0x01 && DEBOUNCE[3])
                DEBOUNCE[3] = false;
        }

        public static void TriggerPrologueSkip()
        {
            var _diffRead = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0x2498);
            var _selectButton = Hypervisor.Read<byte>(0xB1D5E4);

            if (Variables.IS_TITLE && SKIP_STAGE != 0)
            {
                Terminal.Log("Title Screen detected! Resetting Roxas Skip!", 0);
                SKIP_STAGE = 0;
                SKIP_ROXAS = false;
            }

            if (!Variables.IS_TITLE)
            {
                var _worldCheck = Hypervisor.Read<byte>(Variables.ADDR_Area);
                var _roomCheck = Hypervisor.Read<byte>(Variables.ADDR_Area + 0x01);
                var _eventCheck = Hypervisor.Read<byte>(Variables.ADDR_Area + 0x04);

                var _cutsceneCheck = Hypervisor.Read<byte>(Variables.ADDR_CutsceneFlag);

                if (_selectButton == 0x00 && SKIP_STAGE == 3)
                {
                    Terminal.Log("Loaded game abandoned. Re-enabling Roxas Skip!", 0);
                    SKIP_STAGE = 0;
                }

                if (_worldCheck == 0x02 && _roomCheck == 0x01 && _eventCheck == 0x38 && SKIP_STAGE == 0)
                {
                    if (SKIP_ROXAS)
                    {
                        Terminal.Log("Room and Settings are correct! Initiating Roxas Skip's First Phase...", 0);

                        Critical.LOCK_AUTOSAVE = true;

                        Hypervisor.Write(Variables.ADDR_Area, 0x322002);
                        Hypervisor.Write(Variables.ADDR_Area + 0x04, 0x01);
                        Hypervisor.Write(Variables.ADDR_Area + 0x08, 0x01);

                        while (Hypervisor.Read<byte>(0xABB3C7) != 0x80) ;

                        Hypervisor.DeleteInstruction((ulong)(Critical.OffsetSetFadeOff + 0x81A), 0x08);
                        Hypervisor.Write<byte>(0xABB3C7, 0x80);

                        while (Hypervisor.Read<byte>(Variables.ADDR_LoadFlag) == 0x00) ;

                        Variables.SharpHook[Critical.OffsetMapJump].Execute(BSharpConvention.MicrosoftX64, (long)(Hypervisor.PureAddress + Variables.ADDR_Area), 2, 0, 0, 0);

                        while (Hypervisor.Read<byte>(Variables.ADDR_LoadFlag) == 0x00) ;

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

                else if (_worldCheck == 0x02 && _roomCheck == 0x20 && _eventCheck == 0x01 && _cutsceneCheck == 0x01 && SKIP_STAGE == 1)
                {
                    Terminal.Log("Room parameters correct! Skip was initiated! Initiating Roxas Skip's Second Phase...", 0);

                    Hypervisor.Write<uint>(Variables.ADDR_Area, 0x00320E02);
                    Hypervisor.Write<uint>(Variables.ADDR_Area + 0x04, 0x02);
                    Hypervisor.Write<uint>(Variables.ADDR_Area + 0x08, 0x12);

                    while (Hypervisor.Read<byte>(Variables.ADDR_LoadFlag) == 0x00) ;

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

                else if (_selectButton == 0x01 && SKIP_STAGE == 0)
                {
                    Terminal.Log("Loaded game detected! Disabling Roxas Skip...", 0);
                    SKIP_STAGE = 3;
                }
            }
        }

        public static void TriggerShortcut()
        {
            var _menuPointer = Hypervisor.GetPointer64(0x2A11110);
            var _menuType = Hypervisor.Read<byte>(_menuPointer, true);

            var _isPaused = Hypervisor.Read<byte>(Variables.ADDR_PauseFlag);
            var _subMenuType = Hypervisor.Read<byte>(Variables.ADDR_SubMenuType);

            var _inputRead = Hypervisor.Read<short>(Variables.ADDR_Input);
            var _currShort = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0xE000);
            var _currForm = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0x3524);

            var _shortReal = Variables.ADDR_SaveData + 0x36F8;
            var _shortFake = Variables.ADDR_SaveData + 0xE100;

            var _fetchPoint = Operations.FetchPointerMSG(Variables.PINT_SystemMSG, 0x051F);

            var _isInMainShortcut = _isPaused == 0x01 && _subMenuType == 0x19;
            var _isEditingShortcut = _isPaused == 0x01 && (_subMenuType == 0x1A || _subMenuType == 0x1D || _subMenuType == 0x1E || _subMenuType == 0x1F);
            var _isInShortcut = _isPaused == 0x01 && (_subMenuType == 0x19 || _subMenuType == 0x1A || _subMenuType == 0x1D || _subMenuType == 0x1E || _subMenuType == 0x1F);

            if (MAIN_TEXT == null)
                MAIN_TEXT = Operations.FetchStringMSG(Variables.PINT_SystemMSG, 0x051F);

            if (!Variables.IS_TITLE)
            {
                if (_isEditingShortcut && !SHORTCUT_TOGGLE)
                    SHORTCUT_TOGGLE = true;

                else if ((!_isEditingShortcut && SHORTCUT_TOGGLE) || Hypervisor.Read<short>(_shortFake) == 0x0000)
                {
                    Terminal.Log("Submitting Shortcut Menu " + Char.ConvertFromUtf32(0x41 + CURR_SHORTCUT) + "!", 0x00);
                    var _shortTake = Hypervisor.Read<byte>(_shortReal, 0x08);
                    Hypervisor.Write(_shortFake + (0x08U * CURR_SHORTCUT), _shortTake);
                    SHORTCUT_TOGGLE = false;
                }

                if (!_isInShortcut)
                    Hypervisor.Write(_fetchPoint, "Sora".ToKHSCII(), true);

                else
                {
                    MAIN_TEXT[MAIN_TEXT.Length - 0x02] = (byte)(0x2E + CURR_SHORTCUT);
                    Hypervisor.Write(_fetchPoint, MAIN_TEXT, true);
                }

                if (_menuType != 0x05 || ((_inputRead & 0x40) == 0x00 && (_inputRead & 0x10) == 0x00))
                    DEBOUNCE[1] = false;

                if (!_isInMainShortcut || ((_inputRead & 0x0400) == 0x00 && (_inputRead & 0x0800) == 0x00))
                    DEBOUNCE[4] = false;

                if (_menuType == 0x05 && !DEBOUNCE[1] && _currForm != 0x03)
                {
                    if ((_inputRead & 0x40) == 0x40 && !DEBOUNCE[1])
                    {
                        Sound.PlaySFX(0x14);
                        CURR_SHORTCUT++;
                        DEBOUNCE[1] = true;
                    }

                    if ((_inputRead & 0x10) == 0x10 && !DEBOUNCE[1])
                    {
                        Sound.PlaySFX(0x14);
                        CURR_SHORTCUT--;
                        DEBOUNCE[1] = true;
                    }
                }

                if (_isInMainShortcut && !DEBOUNCE[4])
                {
                    if ((_inputRead & 0x0800) == 0x0800 && !DEBOUNCE[4])
                    {
                        Sound.PlaySFX(0x02);
                        CURR_SHORTCUT++;
                        DEBOUNCE[4] = true;
                    }

                    if ((_inputRead & 0x0400) == 0x0400 && !DEBOUNCE[4])
                    {
                        Sound.PlaySFX(0x02);
                        CURR_SHORTCUT--;
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
        }
    
        public static void TriggerUpdate()
        {
            if (!UPDATE_TRIGGERED)
            {
                var _isLoaded = Hypervisor.Read<byte>(Variables.ADDR_LoadFlag) == 0x01 ? true : false;
                var _isPaused = Hypervisor.Read<byte>(Variables.ADDR_PauseFlag) == 0x01 ? true : false;
                var _menuType = Hypervisor.Read<byte>(Variables.ADDR_MenuType);

                var UPDATE_TEXTAbsolute = Operations.FetchPointerMSG(Variables.PINT_SystemMSG, 0x0129);

                if (!Variables.IS_TITLE && _isLoaded)
                {
                    Critical.LOCK_AUTOSAVE = true;

                    if (!_isPaused && _menuType != 0x08)
                    {
                        Thread.Sleep(750);
                        Variables.SharpHook[Critical.OffsetCampMenu].Execute(BSharpConvention.MicrosoftX64, 0, 0);
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
                            UPDATE_DONE_TEXT = Operations.FetchStringMSG(Variables.PINT_SystemMSG, 0x012A);
                            UPDATE_TEXT = Operations.FetchStringMSG(Variables.PINT_SystemMSG, 0x0129);

                            UPDATE_BAR_INDEX = UPDATE_TEXT.ToList().FetchIndexOf(x => x == 0x87);
                            UPDATE_PRG_INDEX = UPDATE_TEXT.ToList().FetchIndexOf(x => x == 0x98);
                           
                            var _tempText = new List<byte>();
                           
                            _tempText.AddRange(UPDATE_TEXT.Take(UPDATE_BAR_INDEX));
                            _tempText.AddRange(Enumerable.Repeat((byte)0x6B, 20).ToArray());
                            _tempText.AddRange(UPDATE_TEXT.Skip(UPDATE_BAR_INDEX + 20));
                           
                            UPDATE_TEXT = _tempText.ToArray();
                            Hypervisor.Write(UPDATE_TEXTAbsolute, UPDATE_TEXT, true);
                        }

                        if (UPDATE_PHASE == 0x00)
                        {
                            Variables.SharpHook[Message.OffsetSetCampWarning].ExecuteJMP(BSharpConvention.MicrosoftX64, 0x0128, 0x0000);
                            Variables.SharpHook[Message.OffsetShowCampWarning].Execute(0x01);
                            Variables.SharpHook[Message.OffsetMenu].Execute(BSharpConvention.MicrosoftX64, 0x04, 0x00);
                            Variables.SharpHook[Message.OffsetMenu + 0x40].Execute();

                            UPDATE_PHASE = 0x01;
                        }

                        if (UPDATE_PHASE == 0x01)
                        {
                            if (_isConfirming && _selectRead == 0x00)
                                UPDATE_PHASE = 2;

                            else if ((_isConfirming && _selectRead == 0x01) || _isDenying)
                            {
                                UPDATE_TRIGGERED = true;
                                UPDATE_PHASE = 0;
                            }
                        }

                        if (UPDATE_PHASE == 0x02)
                        {
                            Variables.SharpHook[0x304B30].Execute();
                            Thread.Sleep(300);

                            Variables.SharpHook[Message.OffsetSetCampWarning].ExecuteJMP(BSharpConvention.MicrosoftX64, 0x0129, 0x0000);
                            
                            for (int i = 0; i < 100; i++)
                            {
                                var _downProgress = Math.Floor(i / 5D);
                                Hypervisor.Write(UPDATE_TEXTAbsolute + (ulong)UPDATE_BAR_INDEX + (ulong)_downProgress, (byte)0x6A, true);
                                Hypervisor.Write(UPDATE_TEXTAbsolute + (uint)UPDATE_PRG_INDEX, i.ToString("000").ToKHSCII(), true);
                                Thread.Sleep(250);
                            }

                            Hypervisor.Write(UPDATE_TEXTAbsolute, UPDATE_DONE_TEXT, true);

                            Variables.SharpHook[Message.OffsetShowCampWarning].Execute();
                            Variables.SharpHook[Message.OffsetMenu].Execute(BSharpConvention.MicrosoftX64, 0x04, 0x00);
                            Variables.SharpHook[Message.OffsetMenu + 0x40].Execute();

                            UPDATE_PHASE = 0x03;
                        }

                        if (UPDATE_PHASE == 0x03)
                        {
                            if (_isConfirming || _isDenying)
                            {
                                UPDATE_TRIGGERED = true;
                                Critical.LOCK_AUTOSAVE = false;
                            }
                        }
                    }
                }
            }
        }
    }
}