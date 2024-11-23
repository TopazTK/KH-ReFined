using ReFined.Libraries;
using ReFined.KH2.Information;

using BSharpConvention = Binarysharp.MSharp.Assembly.CallingConvention.CallingConventions;

namespace ReFined.KH2.InGame
{
    public static class Dialogs
    {
        static bool DIALOG_CAMP_ACTIVE;
        static bool DIALOG_CAMP_ANSWERED;

        public static nint FUNC_SETMENUMODE;
        public static nint FUNC_SETCAMPWARNING;
        public static nint FUNC_SHOWCAMPWARNING;
        public static nint FUNC_FADECAMPWARNING;

        public static bool ShowDialogCAMP(short StringID, DIALOG_BUTTONS Buttons)
        {
            var _isPaused = Hypervisor.Read<byte>(Variables.ADDR_PauseFlag) == 0x01 ? true : false;
            var _menuType = Hypervisor.Read<byte>(Variables.ADDR_MenuType);

            var _returnType = false;

            if (_isPaused && _menuType == 0x08)
            {
                Variables.SharpHook[FUNC_SETCAMPWARNING].ExecuteJMP(BSharpConvention.MicrosoftX64, StringID, 0x0000);
                Variables.SharpHook[FUNC_SHOWCAMPWARNING].Execute((int)Buttons);
                Variables.SharpHook[FUNC_SETMENUMODE].Execute(BSharpConvention.MicrosoftX64, 0x04, 0x00);
                Variables.SharpHook[FUNC_SETMENUMODE + 0x40].Execute();
                DIALOG_CAMP_ACTIVE = true;
            }

            while (DIALOG_CAMP_ACTIVE)
            {
                var _selectRead = Hypervisor.Read<byte>(Variables.ADDR_DialogSelect);

                var _confirmPressed = Variables.IS_PRESSED(Variables.CONFIRM_BUTTON);
                var _rejectPressed = Variables.IS_PRESSED(Variables.REJECT_BUTTON);

                if (_confirmPressed)
                {
                    _returnType = _selectRead == 0x00 ? true : false;
                    DIALOG_CAMP_ACTIVE = false;
                }

                else if (_rejectPressed)
                {
                    _returnType = false;
                    DIALOG_CAMP_ACTIVE = false;
                }
            }

            if (!DIALOG_CAMP_ACTIVE)
            {
                Variables.SharpHook[FUNC_FADECAMPWARNING].Execute();
                Thread.Sleep(300);
            }

            return _returnType;
        }

        public enum DIALOG_BUTTONS : int
        {
            NO_BUTTON = -1,
            OK_BUTTON = 0,
            YES_NO_BUTTON = 1
        };
    }
}
