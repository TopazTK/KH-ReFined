using ReFined.KH2.Information;

using BSharpConvention = Binarysharp.MSharp.Assembly.CallingConvention.CallingConventions;

namespace ReFined.KH2.InGame
{
    public static class Popups
    {
        public static nint FUNC_STARTCAMP;
        public static nint FUNC_SHOWPRIZE;
        public static nint FUNC_SHOWINFORMATION;

        public static void PopupMenu(int Type, int SubType) => Variables.SharpHook[FUNC_STARTCAMP].Execute(BSharpConvention.MicrosoftX64, Type, SubType);

        public static void PopupInformation(short StringID)
        {
            if (!Variables.IS_TITLE && Variables.IS_LOADED && !Variables.IS_CUTSCENE)
            {
                long _pointString = (long)Operations.GetStringPointer(StringID);
                Variables.SharpHook[FUNC_SHOWINFORMATION].Execute(_pointString);
            }
        }

        public static void PopupPrize(short StringID)
        {
            if (!Variables.IS_TITLE && Variables.IS_LOADED && !Variables.IS_CUTSCENE)
            {
                long _pointString = (long)Operations.GetStringPointer(StringID);
                Variables.SharpHook[FUNC_SHOWPRIZE].Execute(_pointString);
            }
        }
    }
}
