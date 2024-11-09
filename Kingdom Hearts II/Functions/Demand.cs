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

        /// <summary>
        /// When the proper input is given, returns to the title screen.
        /// When the option for it is toggled, prompts the user for a cancellation.
        /// </summary>
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

        /// <summary>
        /// When it's chosen as a new game begins, this logic will skip the Roxas Prologue.
        /// </summary>
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

                        Hypervisor.Write(Variables.ADDR_Area, 0x322002);
                        Hypervisor.Write(Variables.ADDR_Area + 0x04, 0x01);
                        Hypervisor.Write(Variables.ADDR_Area + 0x08, 0x01);

                        while (Hypervisor.Read<byte>(Variables.ADDR_LoadFlag) == 0x00) ;

                        Variables.SharpHook[Critical.OffsetMapJump].Execute(BSharpConvention.MicrosoftX64, (long)(Hypervisor.PureAddress + Variables.ADDR_Area), 2, 0, 0, 0);

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

                else if (_worldCheck == 0x02 && _roomCheck == 0x20 && _eventCheck == 0x9A && SKIP_STAGE == 1)
                {
                    Terminal.Log("Room parameters correct! Skip was initiated! Initiating Roxas Skip's Second Phase...", 0);

                    Hypervisor.Write<uint>(Variables.ADDR_Area, 0x001702);
                    Hypervisor.Write<uint>(Variables.ADDR_Area + 0x04, (0x02 << 10) + 0x02);
                    Hypervisor.Write<uint>(Variables.ADDR_Area + 0x08, 0x02);

                    while (Hypervisor.Read<byte>(Variables.ADDR_LoadFlag) == 0x00) ;

                    Variables.SharpHook[Critical.OffsetMapJump].Execute(BSharpConvention.MicrosoftX64, (long)(Hypervisor.PureAddress + Variables.ADDR_Area), 2, 0, 0, 0);

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
