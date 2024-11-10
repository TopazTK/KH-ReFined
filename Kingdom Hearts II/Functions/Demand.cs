using ReFined.Common;
using ReFined.Libraries;
using ReFined.KH2.InGame;
using ReFined.KH2.Information;
using BSharpConvention = Binarysharp.MSharp.Assembly.CallingConvention.CallingConventions;

namespace ReFined.KH2.Functions
{
    public static class Demand
    {
        public static ulong PROMPT_FUNCTION;

        public static int SKIP_STAGE;
        public static bool SKIP_ROXAS;

        static bool[] DEBOUNCE = new bool[0x20];

        public static void TriggerReset()
        {
            var _currentTime = DateTime.Now;

            var _buttonRead = Hypervisor.Read<ushort>(Variables.ADDR_Input);
            var _confirmRead = Hypervisor.Read<ushort>(Variables.ADDR_Confirm);

            var _loadRead = Hypervisor.Read<byte>(Variables.ADDR_LoadFlag);

            var _canReset = !Variables.IS_TITLE && _loadRead == 0x01;

            // If the button combo was exactly as requested, and a menu isn't present:
            if (_buttonRead == Variables.RESET_COMBO && _canReset && !DEBOUNCE[0])
            {
                Terminal.Log("Soft Reset requested.", 0);
                DEBOUNCE[0] = true;

                // If the prompt has been requested:
                if (Variables.RESET_PROMPT)
                {
                    Terminal.Log("Soft Reset Prompt enabled. Showing prompt.", 0);

                    // Show the prompt.
                    Message.ShowSmallObtained(0x0100);
                    var _cancelRequest = false;

                    // Start the prompt task.
                    Task.Factory.StartNew(() =>
                    {
                        Terminal.Log("Waiting 2.2 seconds before execution.", 0);

                        // For the next 2 seconds:
                        while ((DateTime.Now - _currentTime) < TimeSpan.FromMilliseconds(2200))
                        {
                            // Monitor the buttons, and if pressed:
                            var _buttonSeek = (_confirmRead == 0x01 ? 0x2000 : 0x4000);
                            var _buttonSecond = Hypervisor.Read<ushort>(Variables.ADDR_Input);

                            // Cancel the reset.
                            if ((_buttonSecond & _buttonSeek) == _buttonSeek)
                            {
                                Terminal.Log("Soft Reset interrupted.", 0);
                                Message.ShowSmallObtained(0x0101);
                                _cancelRequest = true;
                                DEBOUNCE[0] = false;
                                break;
                            };
                        }

                        // If not cancelled: Initiate the reset.
                        if (!_cancelRequest)
                        {
                            Hypervisor.Write<byte>(Variables.ADDR_Reset, 0x01);
                            Terminal.Log("Soft Reset executed.", 0);
                            DEBOUNCE[0] = false;
                        }
                    });
                }

                // If the prompt isn't requested: Reset instantly.
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
            // The pointer for the current room's enemy information.
            var _roomPoint = Hypervisor.Read<ulong>(Variables.PINT_EnemyInfo) + 0x08;

            // Reading the current values into memory.
            var _roomRead = Hypervisor.Read<byte>(Variables.ADDR_LoadFlag);
            var _abilityRead = Hypervisor.Read<ushort>(Variables.ADDR_SaveData + 0x2544, 0x60);

            // Check if the game is loaded, and if the player has Encounter Plus in anyway.
            if (!Variables.IS_TITLE && _roomRead == 0x01 && !DEBOUNCE[2] && !_abilityRead.Contains<ushort>(0x80F8) && !_abilityRead.Contains<ushort>(0x00F8))
            {
                // If not, give them the ability.
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

                        // Wait until the fade has been completed.
                        while (Hypervisor.Read<byte>(0xABB3C7) != 0x80) ;

                        // Destroy the fade handler so it does not cause issues.
                        Hypervisor.DeleteInstruction((ulong)(Critical.OffsetSetFadeOff + 0x81A), 0x08);
                        Hypervisor.Write<byte>(0xABB3C7, 0x80);

                        while (Hypervisor.Read<byte>(Variables.ADDR_LoadFlag) == 0x00) ;

                        Variables.SharpHook[Critical.OffsetMapJump].Execute(BSharpConvention.MicrosoftX64, (long)(Hypervisor.PureAddress + Variables.ADDR_Area), 2, 0, 0, 0);

                        while (Hypervisor.Read<byte>(Variables.ADDR_LoadFlag) == 0x00) ;

                        // Restore the fade initiater after load.
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
                    Thread.Sleep(1000);

                    Terminal.Log("Room parameters correct! Skip was initiated! Initiating Roxas Skip's Second Phase...", 0);

                    Hypervisor.Write<uint>(Variables.ADDR_Area, 0x00320E02);
                    Hypervisor.Write<uint>(Variables.ADDR_Area + 0x04, 0x02);
                    Hypervisor.Write<uint>(Variables.ADDR_Area + 0x08, 0x12);

                    while (Hypervisor.Read<byte>(Variables.ADDR_LoadFlag) == 0x00) ;

                    Variables.SharpHook[Critical.OffsetMapJump].Execute(BSharpConvention.MicrosoftX64, (long)(Hypervisor.PureAddress + Variables.ADDR_Area), 2, 0, 0, 0);

                    // Look. Imma be honest with ya. All THIS to skip just *ONE* cutscene may not be worth it.
                    // Buth damn does it make the transition E X T R E M E L Y smooth.

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
    }
}
