using DiscordRPC;
using Binarysharp.MSharp;

using ReFined.KH2.Menus;
using ReFined.Libraries;

namespace ReFined.KH2.Information
{
    public static class Variables
    {
        //
        // CONFIG VARIABLES
        //
        // Variables that will be read from a config file to tell Re:Fined what to do.
        //

        public static bool IS_LITE = false;
        public static bool DEV_MODE;
        public static bool ATTACK_TOGGLE;
        public static bool DISCORD_TOGGLE = true;

        public static bool REGISTER_MAGIC = false;
        public static bool RESET_PROMPT = true;
        public static ushort RESET_COMBO = 0x0300;

        public static int SAVE_MODE = 0x00;

        public static bool RATIO_ADJUST;
        public static bool FORM_SHORTCUT = true;

        public static int AUDIO_MODE = 0x00;
        public static bool ENEMY_VANILLA = true;
        public static bool MUSIC_VANILLA = false;

        public static bool RETRY_DEFAULT = true;
        public static bool CONTROLLER_MODE = true;

        public static string LIMIT_SHORTS;

        public static Config CONFIG_MENU;
        public static Intro INTRO_MENU;
        public static Continue CONTINUE_MENU;

        public static bool IS_TITLE =>
            Hypervisor.Read<uint>(ADDR_Area) == 0x00FFFFFF
         || Hypervisor.Read<uint>(ADDR_Area) == 0x00000101
         || Hypervisor.Read<uint>(ADDR_Reset) == 0x00000001;

        //
        // RESOURCE LIBRARY
        //
        // Reserved for static resources, or initialization of APIs
        //

        public static MemorySharp SharpHook;
        public static DiscordRpcClient DiscordClient = new DiscordRpcClient("833511404274974740");

        public static string[] BOSSObjentry =
{
            "B_BB100",
            "B_BB100_GM",
            "B_BB100_TSURU",
            "B_CA000",
            "B_CA050",
            "B_CA050_GM",
            "B_LK120",
            "B_LK120_GM",
            "B_MU120",
            "B_MU120_GM",
        };

        public static string[] ENEMYObjentry =
        {
            "M_EX010",
            "M_EX010_NM",
            "M_EX050",
            "M_EX060",
            "M_EX200",
            "M_EX200_NM",
            "M_EX500",
            "M_EX500_GM",
            "M_EX500_HB",
            "M_EX500_HB_GM",
            "M_EX500_NM",
            "M_EX510",
            "M_EX520",
            "M_EX520_AL",
            "M_EX530",
            "M_EX540",
            "M_EX550",
            "M_EX560",
            "M_EX570",
            "M_EX590",
            "M_EX620",
            "M_EX620_AL",
            "M_EX630",
            "M_EX640",
            "M_EX650",
            "M_EX670",
            "M_EX690",
            "M_EX710",
            "M_EX720",
            "M_EX730",
            "M_EX750",
            "M_EX750_NM",
            "M_EX780",
            "M_EX790",
            "M_EX790_HALLOWEEN",
            "M_EX790_HALLOWEEN_NM"
        };

        //
        // ALTERED VARIABLES
        //
        // Variables that can be altered reside here.
        //

        public static bool Initialized = false;

        public static Task DCTask;
        public static Task ASTask;
        public static Task CRTask;
        public static CancellationToken Token;
        public static CancellationTokenSource Source;

        //
        // ADDRESSES
        //
        // All of the necessary address values.
        //

        public static ulong ADDR_Reset = 0xABAC5A;
        public static ulong ADDR_Input = 0xBF3270;
        public static ulong ADDR_Confirm = 0x715382;

        public static ulong ADDR_Area = 0x717008;
        public static ulong ADDR_Title = 0x7169B4;
        public static ulong ADDR_LoadFlag = 0x9BA8D0;
        public static ulong ADDR_PauseFlag = 0x9006B0;
        public static ulong ADDR_FinishFlag = 0xABC66C;
        public static ulong ADDR_SubMenuType = 0x7435D4;

        public static ulong ADDR_BattleFlag = 0x2A11404;
        public static ulong ADDR_CutsceneFlag = 0x728440;

        public static ulong ADDR_Config = 0x9ADA54;
        public static ulong ADDR_NewGame = 0xB1D790;
        public static ulong ADDR_SaveData = 0x9A98B0;

        public static ulong ADDR_Framerate = 0x71536E;

        public static ulong ADDR_Framelimiter = 0x154620;
        public static ulong ADDR_ControllerMode = 0x2B44A88;

        public static ulong ADDR_ConfigMenu = 0x820000;
        public static ulong ADDR_NewGameMenu = 0x820200;

        public static ulong ADDR_ActionExe = 0x2A5C996;
        public static ulong ADDR_ReactionID = 0x2A11162;

        public static ulong ADDR_MenuType = 0x900724;
        public static ulong ADDR_MenuSelect = 0x902FA0;

        public static ulong ADDR_MagicCommands = 0x2A11188;
        public static ulong ADDR_MagicIndex = 0x2A1073C;
        public static ulong ADDR_MagicLV1 = 0x9ACE44;
        public static ulong ADDR_MagicLV2 = 0x9ACE7F;

        public static ulong ADDR_MusicPath = 0x5B4C74;

        public static ulong ADDR_PAXFormatter = 0x5C8590;
        public static ulong ADDR_ANBFormatter = 0x5B8FB0;
        public static ulong ADDR_EVTFormatter = 0x5B9020;
        public static ulong ADDR_BTLFormatter = 0x5C5E48;

        public static ulong ADDR_ObjentryBASE = 0x2A254D0;

        //
        // POINTERS
        //
        // Addresses for the pointers we need.
        //

        public static ulong PINT_SystemMSG = 0x2A11678;

        public static ulong PINT_ConfigMenu = 0xBF0150;
        public static ulong PINT_SubMenuOptionSelect = 0xBEECD8;

        public static ulong PINT_SaveInformation = 0x79CB10;

        public static ulong PINT_GameOver = 0xBEF4A8;
        public static ulong PINT_GameOverOptions = 0x2A11360;

        public static ulong PINT_ChildMenu = 0x2A11118;

        //
        // RPC ASSET LIBRARY
        //
        // Everything DiscordRPC uses (except for the RPC itself) resides here.
        //

        public static string[] DICTIONARY_BTL = { "safe", "mob", "boss" };
        public static string[] DICTIONARY_WRL = { "", "", "tt", "", "hb", "bb", "he", "al", "mu", "po", "lk", "lm", "dc", "wi", "nm", "wm", "ca", "tr", "eh" };
        public static string[] DICTIONARY_CPS = { "cup_pp", "cup_cerb", "cup_titan", "cup_god", "cup_hades" };
        public static string[] DICTIONARY_FRM = { "None", "Valor", "Wisdom", "Limit", "Master", "Final", "Anti" };
        public static string[] DICTIONARY_MDE = { "Beginner Mode", "Standard Mode", "Proud Mode", "Critical Mode" };

        //
        // FUNCTION SIGNATURES
        //
        // Because I don't want to do this again.
        //

        public static string FUNC_ShowInformation = "40 53 48 83 EC 20 48 8B D9 48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 48 8B D3";
        public static string FUNC_ShowObatined = "40 53 48 83 EC 20 48 8B 15 ?? ?? ?? ?? 48 8B D9 4C 63 82 ?? ?? ?? ??";
        public static string FUNC_PlaySFX = "48 83 EC ?? 44 8B C2 C7 44 24 20 ?? ?? ?? ??";
        public static string FUNC_SetMenuType = "89 0D ?? ?? ?? ?? C7 05 ?? ?? ?? ?? FF FF FF FF 89 15 ?? ?? ?? ??";
        public static string FUNC_SetSLWarning = "40 57 48 83 EC 50 8B F9";
        public static string FUNC_ShowSLWarning = "48 89 5C 24 08 57 48 83 EC 40 8B F9 48 8B 0D ?? ?? ?? ??";
        public static string FUNC_SetCampWarning = "48 89 5C 24 08 57 48 83 EC 50 8B F9 8B DA";
        public static string FUNC_ShowCampWarning = "40 55 48 83 EC 50 44 8B 0D ?? ?? ?? ??";
        public static string FUNC_ExecuteCampMenu = "40 56 41 56 41 57 48 83 EC 20 45 32 FF 44 8B F2 44 38 3D ?? ?? ?? ??";
        public static string FUNC_StopBGM = "40 53 48 83 EC 20 48 83 3D ?? ?? ?? ?? 00 0F 84 ?? ?? ?? ?? 48 8B 1D ?? ?? ?? ??";
        public static string FUNC_MapJump = "48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 57 48 83 EC 20 80 3D ?? ?? ?? ?? 00 41 0F B6 E9";
        public static string FUNC_SetFadeOff = "48 83 EC 28 85 C9 79 0F 0F BA F1 1F 89 0D ?? ?? ?? ?? 48 83 C4 28 C3 89 0D ?? ?? ?? ?? 81 E1 FF FF FF 3F 0F 84 ?? ?? ?? ?? 83 E9 01";

        //
        // HOTFIX SIGNATURES
        //
        // Refer to the block above.
        //

        public static string HFIX_Framelimiter = "F3 0F 10 15 ?? ?? ?? ?? F3 0F 10 0D ?? ?? ?? ?? F3 0F 10 05 ?? ?? ?? ?? F3 0F 59 CA";
        public static string HFIX_ContPrompts = "C7 05 ?? ?? ?? ?? 01 00 00 00 E8 ?? ?? ?? ?? 8B 0D ?? ?? ?? ??";
        public static string HFIX_WarpContinue = "E8 59 00 00 00 48 8B 0D ?? ?? ?? ??";
        public static string HFIX_InventoryReset = "48 8D 15 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? 41 B8 30 08 00 00 48 8D 15 ?? ?? ?? ??";
        public static string HFIX_CommandNavigation = "48 89 5C 24 18 55 41 56 41 57 48 83 EC 20 4C 8B 41 08 48 8B D9";

        public static string HFIX_ConfigFirst = "40 53 48 83 EC 20 0F B6 D9 48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 4C 8B 1D ?? ?? ?? ??";
        public static string HFIX_ConfigSecond = "48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 57 41 54 41 55 41 56 41 57 48 81 EC 80 00 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 70 E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ??";
        public static string HFIX_ConfigThird = "48 89 5C 24 08 48 89 74 24 10 57 48 83 EC 30 48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 8B F8 E8 ?? ?? ?? ?? 8B F0 83 F8 02 0F 85 ?? ?? ?? ?? E8 ?? ?? ?? ?? 8B D8 E8 ?? ?? ?? ?? 84 C0 75 59 48 8B 0D ?? ?? ?? ??";
        public static string HFIX_ConfigFourth = "48 89 5C 24 08 57 48 83 EC 20 8B FA 8B D9 E8 ?? ?? ?? ?? 8D 0C 3B 44 8D 04 9D 00 00 00 00";
        public static string HFIX_ConfigFifth = "48 89 5C 24 08 48 89 74 24 10 57 48 83 EC 30 E8 ?? ?? ?? ?? 45 33 C0 33 C9 41 8D 50 FF E8 ?? ?? ?? ??";
        public static string HFIX_ConfigSixth = "40 53 55 56 57 41 54 41 55 41 56 41 57 48 83 EC 58 E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 4C 8B F8 E8 ?? ?? ?? ?? 41 BD ?? ?? ?? ??";

        public static List<ulong> HFIX_ConfigOffsets = new List<ulong>();

        //
        // VALUE DUMP
        //
        // The values themselves, which will be written to shit, are stored here.
        // 

        public static List<ushort[]> ARRY_ContinueOptions = new List<ushort[]>
        {
            new ushort[] { 0x0002, 0x0002, 0x8AB0, 0x0001, 0x8AAF, 0x0000, 0x0000, 0x0000, 0x0000 }, // No Retry
            new ushort[] { 0x0004, 0x0002, 0x8AB1, 0x0002, 0x01DE, 0x0002, 0x8AB0, 0x0001, 0x8AAF }, // Retry Default
            new ushort[] { 0x0004, 0x0002, 0x8AB0, 0x0002, 0x8AB1, 0x0002, 0x01DE, 0x0001, 0x8AAF }, // Continue Default
        };

        public static byte[] HASH_SwapAudio = { 0x26, 0x72, 0x0C, 0xDE, 0xD5, 0x68, 0x39, 0x0F, 0x18, 0x5A, 0x98, 0x8E, 0xD0, 0x8C, 0x90, 0xC5 };
        public static byte[] HASH_SwapExtra = { 0x79, 0x57, 0x31, 0x9B, 0xB3, 0xDC, 0x23, 0x1D, 0x8D, 0xF5, 0x54, 0x23, 0x08, 0xB8, 0x03, 0xA1 };
        public static byte[] HASH_SwapEnemy = { 0x82, 0x99, 0xD3, 0x20, 0xC6, 0x70, 0xC4, 0x9F, 0x7C, 0x02, 0x94, 0x06, 0xAC, 0x19, 0x53, 0xBD };
        public static byte[] HASH_SwapMusic = { 0x84, 0x7F, 0x72, 0x02, 0x21, 0xE0, 0xBC, 0x89, 0x70, 0xEC, 0x27, 0xE2, 0x25, 0x2D, 0x2E, 0x26 };

        public static byte[] VALUE_MPSEQD = { 0x7A, 0x78, 0x18, 0x79 };

        //
        // ENUM AREA
        //
        // The various enums which will be used.
        //

        public enum CONFIG_BITWISE : ushort
        {
            OFF = 0x0000,
            VIBRATION = 0x0001,
            AUTOSAVE_SILENT = 0x0002,
            AUTOSAVE_INDICATOR = 0x0004,
            NAVI_MAP = 0x0008,
            FIELD_CAM = 0x0010,
            RIGHT_STICK = 0x0020,
            COMMAND_KH2 = 0x0040,
            CAMERA_H = 0x0080,
            CAMERA_V = 0x0100,
            SUMMON_PARTIAL = 0x0200,
            SUMMON_FULL = 0x0400,
            AUDIO_JAPANESE = 0x0800,
            MUSIC_VANILLA = 0x1000,
            PROMPT_CONTROLLER = 0x2000,
            COMMAND_VLAD = 0x4000,
            HEARTLESS_VANILLA = 0x8000
        }
    }
}
