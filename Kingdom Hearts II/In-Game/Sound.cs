using ReFined.KH2.Information;

namespace ReFined.KH2.InGame
{
    public static class Sound
    {
        public static nint FUNC_PLAYSFX;
        public static nint FUNC_KILLBGM;

        public static void PlaySFX(int SoundID) => Variables.SharpHook[FUNC_PLAYSFX].Execute(SoundID);
        public static void KillBGM() => Variables.SharpHook[FUNC_KILLBGM].Execute();
    }
}
