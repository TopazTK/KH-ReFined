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
        /// Shows a Warning Window, with the given text.
        /// Must be in the Save/Load Menu.
        /// </summary>
        /// <param name="StringID">The ID of thetext to be shown.</param>
        public static void ShowSLWarning(short StringID)
        {
            if (!Variables.IS_TITLE)
            {
                Variables.SharpHook[OffsetSetSLWarning].Execute(StringID);
                Variables.SharpHook[OffsetShowSLWarning].Execute();
                Variables.SharpHook[OffsetMenu].Execute(BSharpConvention.MicrosoftX64, 0x05, 0x00);
            }
        }

        /// <summary>
        /// Shows a Warning Window, with the given text ID.
        /// Must be in a Menu.
        /// </summary>
        /// <param name="StringID">The ID of the text to be shown.</param>
        /// <param name="Type">The menu to fall back after confirmation.</param>
        public static void ShowCampWarning(short StringID, int WarningType, int MenuType)
        {
            if (!Variables.IS_TITLE)
            {
                var _currentMenu = Hypervisor.Read<int>(0x687B1E);
                Hypervisor.Write(0x689542, _currentMenu);

                Variables.SharpHook[OffsetSetCampWarning].ExecuteJMP(BSharpConvention.MicrosoftX64, StringID, 0x0000);
                Variables.SharpHook[OffsetShowCampWarning].Execute(WarningType);
                Variables.SharpHook[OffsetMenu].Execute(BSharpConvention.MicrosoftX64, MenuType, 0x00);

                Hypervisor.Write(0x1DCDDA + 0x340, _currentMenu);
            }
        }
     }
}
