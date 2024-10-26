using ReFined.KH2.Information;

namespace ReFined.KH2.InGame
{
    public static class Sound
    {
        public static IntPtr OffsetSound;

        /// <summary>
        /// Plays a sound effect according to the ID given.
        /// </summary>
        /// <param name="SoundID">The ID of the sound to be played.</param>
        public static void PlaySFX(int SoundID)
        {
            Variables.SharpHook[OffsetSound].Execute(SoundID);
        }
    }
}
