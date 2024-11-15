using ReFined.Common;
using ReFined.KH2.Functions;
using ReFined.KH2.Information;
using ReFined.Libraries;

using BSharpConvention = Binarysharp.MSharp.Assembly.CallingConvention.CallingConventions;

namespace ReFined.KH2.InGame
{
    public static class Message
    {
        public static IntPtr OffsetInfo;
        public static IntPtr OffsetMenu;
        public static IntPtr OffsetObtained;
        public static IntPtr OffsetSetSLWarning;
        public static IntPtr OffsetShowSLWarning;
        public static IntPtr OffsetSetCampWarning;
        public static IntPtr OffsetShowCampWarning;
        public static IntPtr OffsetFadeCampWarning;

        static bool DIALOG_CAMP_ACTIVE = false;

        /// <summary>
        /// Shows the Information Bar in-game, with the given text.
        /// </summary>
        /// <param name="StringID">The ID of the text to be shown.</param>
        public static void ShowInformation(ushort StringID)
        {
            if (!Variables.IS_TITLE)
            {
                var _pointString = Operations.FetchPointerMSG(Variables.PINT_SystemMSG, StringID);
                Variables.SharpHook[OffsetInfo].Execute((long)_pointString);
            }
        }

        /// <summary>
        /// Shows the Information Bar in-game, with the given raw text.
        /// </summary>
        /// <param name="String">The text to be shown.</param>
        public static void ShowInformationRAW(string Input)
        {
            if (!Variables.IS_TITLE)
            {
                var _convString = Input.ToKHSCII();
                Hypervisor.Write(Hypervisor.PureAddress + 0x800000, _convString, true);

                Variables.SharpHook[OffsetInfo].Execute((long)(Hypervisor.PureAddress + 0x800000));
            }
        }

        /// <summary>
        /// Shows the Small Obtained Window in-game, with the given text.
        /// </summary>
        /// <param name="StringID">The ID of the text to be shown.</param>
        public static void ShowSmallObtained(ushort StringID)
        {
            if (!Variables.IS_TITLE)
            {
                var _pointString = Operations.FetchPointerMSG(Variables.PINT_SystemMSG, StringID);
                Variables.SharpHook[OffsetObtained].Execute((long)_pointString);
            }
        }

        /// <summary>
        /// Shows the Small Obtained Window in-game, with the given raw text.
        /// </summary>
        /// <param name="String">The text to be shown.</param>
        public static void ShowSmallObtainedRAW(string Input)
        {
            if (!Variables.IS_TITLE)
            {
                var _convString = Input.ToKHSCII();
                Hypervisor.Write(Hypervisor.PureAddress + 0x800000, _convString, true);
                Variables.SharpHook[OffsetObtained].Execute((long)(Hypervisor.PureAddress + 0x800000));
            }
        }

        /// <summary>
        /// Pops-Up a Dialog in the Camp Menu, with the given StringID and button layout.
        /// </summary>
        /// <param name="StringID">The text to show, as the StringID.</param>
        /// <param name="Buttons">The button layout. See DIALOG_TYPE.</param>
        /// <returns>TRUE if confirmed, FALSE if rejected.</returns>
        public static bool ShowDialogCamp(short StringID, Variables.DIALOG_TYPE Buttons)
        {
            var _isPaused = Hypervisor.Read<byte>(Variables.ADDR_PauseFlag) == 0x01 ? true : false;
            var _menuType = Hypervisor.Read<byte>(Variables.ADDR_MenuType);

            var _returnType = false;

            if (_isPaused && _menuType == 0x08)
            {
                Variables.SharpHook[OffsetSetCampWarning].ExecuteJMP(BSharpConvention.MicrosoftX64, StringID, 0x0000);
                Variables.SharpHook[OffsetShowCampWarning].Execute((int)Buttons);
                Variables.SharpHook[OffsetMenu].Execute(BSharpConvention.MicrosoftX64, 0x04, 0x00);
                Variables.SharpHook[OffsetMenu + 0x40].Execute();
                DIALOG_CAMP_ACTIVE = true;
            }

            while (DIALOG_CAMP_ACTIVE)
            {
                var _inputRead = Hypervisor.Read<ushort>(Variables.ADDR_Input);
                var _selectRead = Hypervisor.Read<byte>(Variables.ADDR_DialogSelect);

                var _denyButton = Hypervisor.Read<byte>(Variables.ADDR_Confirm) == 0x01 ? 0x4000 : 0x2000;
                var _confirmButton = Hypervisor.Read<byte>(Variables.ADDR_Confirm) == 0x01 ? 0x2000 : 0x4000;

                var _isDenying = (_inputRead & _denyButton) == _denyButton;
                var _isConfirming = (_inputRead & _confirmButton) == _confirmButton;

                if (_isConfirming && _selectRead == 0x00)
                {
                    Variables.SharpHook[OffsetFadeCampWarning].Execute();
                    Thread.Sleep(300);

                    _returnType = true;
                    DIALOG_CAMP_ACTIVE = false;
                }

                else if ((_isConfirming && _selectRead == 0x01) || _isDenying)
                {
                    Variables.SharpHook[OffsetFadeCampWarning].Execute();
                    Thread.Sleep(300);

                    _returnType = false;
                    DIALOG_CAMP_ACTIVE = false;
                }
            }
        
            return _returnType;
        }
     }
}
