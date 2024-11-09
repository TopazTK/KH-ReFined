using ReFined.Common;
using ReFined.Libraries;
using ReFined.KH2.InGame;
using ReFined.KH2.Information;

namespace ReFined.KH2.Functions
{
    public static class Demand
    {
        public static ulong PROMPT_FUNCTION;
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
                    Message.ShowSmallObtained(0x01BA);
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
                                Message.ShowSmallObtained(0x01BB);
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
    }
}
