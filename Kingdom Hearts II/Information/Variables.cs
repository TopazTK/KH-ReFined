#define STEAM

using DiscordRPC;
using Binarysharp.MSharp;

using ReFined.KH2.Menus;
using ReFined.Libraries;

namespace ReFined.KH2.Information
{
    public static class Variables
    {
        // === CONFIG VARIABLES === //

        public static bool IS_LITE = false;
        public static bool INITIALIZED = false;

        #if STEAM
        public static string PLATFORM = "STEAM";
        #elif EPIC
        public static string PLATFORM = "EPIC";
        #endif

        public static double VERSION = 2.00;

        public static bool DEV_MODE = false;
        public static bool AUTOATTACK = false;
        public static bool RESET_PROMPT = true;
        public static bool FORM_SHORTCUT = true;
        public static bool RETRY_DEFAULT = true;
        public static bool DISCORD_TOGGLE = true;

        public static int SAVE_MODE = 0x00;
        public static int AUDIO_MODE = 0x00;
        public static int SUB_LANGUAGE = 0x00;
        public static bool CONTROLLER_MODE = true;

        public static BUTTON MARE_SHORTCUT = BUTTON.NONE;
        public static BUTTON RESET_COMBO = BUTTON.NONE;

        public static bool TECHNICOLOR = false;
        public static bool ENEMY_VANILLA = true;
        public static bool MUSIC_VANILLA = false;

        public static string LIMIT_SHORTS;

        public static Intro INTRO_MENU;
        public static Config CONFIG_MENU;
        public static Continue CONTINUE_MENU;

        public static Intro.Entry AUDIO_SUB_INTRO;
        public static Config.Entry AUDIO_SUB_CONFIG;

        public static List<string> LOADED_LANGS = new List<string>();

        // === FLAG VARIABLES === //

        public static bool IS_TITLE =>
            Hypervisor.Read<uint>(ADDR_Area) == 0x00FFFFFF
         || Hypervisor.Read<uint>(ADDR_Area) == 0x00000101
         || Hypervisor.Read<uint>(ADDR_Title) == 0x00000001
         || Hypervisor.Read<uint>(ADDR_Reset) == 0x00000001;

        public static bool IS_LOADED =>
            Hypervisor.Read<byte>(ADDR_LoadFlag) == 0x01;

        public static bool IS_EVENT =>
            Hypervisor.Read<ulong>(PINT_EventInfo) != 0x00 &&
           (Hypervisor.Read<uint>(Hypervisor.GetPointer64(PINT_EventInfo, [0x04]), true) == 0xCAFEEFAC ||
            Hypervisor.Read<uint>(Hypervisor.GetPointer64(PINT_EventInfo, [0x04]), true) == 0xEFACCAFE);

        public static bool IS_CUTSCENE =>
            Hypervisor.Read<ulong>(PINT_EventInfo) != 0x00 &&
            Hypervisor.Read<uint>(Hypervisor.GetPointer64(PINT_EventInfo, [0x04]), true) != 0xCAFEEFAC &&
            Hypervisor.Read<uint>(Hypervisor.GetPointer64(PINT_EventInfo, [0x04]), true) != 0xEFACCAFE;

        public static bool IS_MOVIE =>
            Hypervisor.Read<byte>(ADDR_MovieFlag) == 0x01;

        #if STEAM

        public static BUTTON CONFIRM_BUTTON =>
            Hypervisor.Read<byte>(ADDR_Confirm) == 0x01 ? BUTTON.CIRCLE : BUTTON.CROSS;

        public static BUTTON REJECT_BUTTON =>
            Hypervisor.Read<byte>(ADDR_Confirm) == 0x01 ? BUTTON.CROSS : BUTTON.CIRCLE;

        #elif EPIC

        public static BUTTON CONFIRM_BUTTON =>
            Hypervisor.Read<byte>(ADDR_Confirm) == 0x00 ? BUTTON.CIRCLE : BUTTON.CROSS;

        public static BUTTON REJECT_BUTTON =>
            Hypervisor.Read<byte>(ADDR_Confirm) == 0x00 ? BUTTON.CROSS : BUTTON.CIRCLE;

        #endif

        public static bool IS_PRESSED(BUTTON Input) =>
            (Hypervisor.Read<BUTTON>(ADDR_Input) & Input) == Input;

        public static BATTLE_TYPE BATTLE_MODE =>
            Hypervisor.Read<BATTLE_TYPE>(ADDR_BattleFlag);

        // === STATIC VARIABLES === //

        public static MemorySharp SharpHook;
        public static DiscordRpcClient DiscordClient = new DiscordRpcClient("833511404274974740");

        public static string[] SUMMObjentry =
        {
            "P_EX330",
            "P_EX350",
            "N_HB040_BTL",
            "P_AL010"
        };

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

        public static List<BUTTON> KONAMI_CODE = new List<BUTTON>()
        {
            BUTTON.UP,
            BUTTON.UP,
            BUTTON.DOWN,
            BUTTON.DOWN,
            BUTTON.LEFT,
            BUTTON.RIGHT,
            BUTTON.LEFT,
            BUTTON.RIGHT,
            BUTTON.CROSS,
            BUTTON.CIRCLE
        };

        // === TASK VARIABLES === //

        public static Task DCTask;
        public static Task ASTask;
        public static Task CRTask;
        public static CancellationToken Token;
        public static CancellationTokenSource Source;

        // === ADDRESSES === //

        // Steam Menu Array = 5BADB0

#if STEAM

        public static ulong ADDR_Area = 0x0717008;
        public static ulong ADDR_Mare = 0x0000000;
        public static ulong ADDR_Reset = 0x0ABAC5A;
        public static ulong ADDR_Input = 0x0BF3270;
        public static ulong ADDR_Title = 0x07169B4;
        public static ulong ADDR_Config = 0x09ADA54;
        public static ulong ADDR_Confirm = 0x0715382;
        public static ulong ADDR_LoadFlag = 0x09BA8D0;
        public static ulong ADDR_MenuFlag = 0x09006B0;
        public static ulong ADDR_PlayerHP = 0x2A23598;
        public static ulong ADDR_MenuType = 0x0900724;
        public static ulong ADDR_SaveData = 0x09A98B0;
        public static ulong ADDR_MagicLV1 = 0x09ACE44;
        public static ulong ADDR_MagicLV2 = 0x09ACE7F;
        public static ulong ADDR_ContData = 0x07A0000;
        public static ulong ADDR_FadeValue = 0x0ABB3C7;
        public static ulong ADDR_Framerate = 0x071536E;
        public static ulong ADDR_PauseFlag = 0x0717418;
        public static ulong ADDR_ActionExe = 0x2A5C996;
        public static ulong ADDR_MovieFlag = 0x2B561E8;
        public static ulong ADDR_IntroMenu = 0x0820200;
        public static ulong ADDR_VendorMem = 0x2A25378;
        public static ulong ADDR_PromptType = 0x0715380;
        public static ulong ADDR_MenuSelect = 0x0902FA0;
        public static ulong ADDR_ReactionID = 0x2A11162;
        public static ulong ADDR_ConfigMenu = 0x0820000;
        public static ulong ADDR_BattleFlag = 0x2A11404;
        public static ulong ADDR_FinishFlag = 0x0ABC66C;
        public static ulong ADDR_MagicIndex = 0x2A1073C;
        public static ulong ADDR_CampBitwise = 0x0BEEC20;
        public static ulong ADDR_Viewspace2D = 0x08A09B8;
        public static ulong ADDR_Viewspace3D = 0x08A0990;
        public static ulong ADDR_SubMenuType = 0x07435D4;
        public static ulong ADDR_TitleSelect = 0x0B1D5E4;
        public static ulong ADDR_CommandMenu = 0x05B16A8;
        public static ulong ADDR_CommandFlag = 0x071740C;
        public static ulong ADDR_DialogSelect = 0x0902521;
        public static ulong ADDR_CutsceneMode = 0x0B65210;
        public static ulong ADDR_Framelimiter = 0x0ABAC08;
        public static ulong ADDR_ObjentryBase = 0x2A254D0;
        public static ulong ADDR_LimitShortcut = 0x05C9678;
        public static ulong ADDR_MagicCommands = 0x2A11188;
        public static ulong ADDR_IntroSelection = 0x0820500;
        public static ulong ADDR_ControllerMode = 0x2B44A88;
        public static ulong ADDR_RenderResolution = 0x08A0980;

        public static ulong DATA_BGMPath = 0x05B4C74;
        public static ulong DATA_PAXPath = 0x05C8590;
        public static ulong DATA_ANBPath = 0x05B8FB0;
        public static ulong DATA_EVTPath = 0x05B9020;
        public static ulong DATA_BTLPath = 0x05C5E48;
        public static ulong DATA_GMIPath = 0x05B5818;

        public static ulong PINT_Camp2LD = 0x09076D0;
        public static ulong PINT_GameOver = 0x0BEF4A8;
        public static ulong PINT_SystemMSG = 0x2A11678;
        public static ulong PINT_ChildMenu = 0x2A11118;
        public static ulong PINT_EnemyInfo = 0x2A0CD70;
        public static ulong PINT_EventInfo = 0x2A11478;
        public static ulong PINT_PartyLimit = 0x2A24CC0;
        public static ulong PINT_ConfigMenu = 0x0BF0150;
        public static ulong PINT_SaveInformation = 0x079CB10;
        public static ulong PINT_GameOverOptions = 0x2A11360;
        public static ulong PINT_SubMenuOptionSelect = 0x0BEECD8;

#elif EPIC

        public static ulong ADDR_Area = 0x0716DF8;
        public static ulong ADDR_Mare = 0x079C7FC;
        public static ulong ADDR_Reset = 0x0ABA6DA;
        public static ulong ADDR_Input = 0x29FAE40;
        public static ulong ADDR_Title = 0x07167A4;
        public static ulong ADDR_Config = 0x09AD4D4;
        public static ulong ADDR_Confirm = 0x0714E02;
        public static ulong ADDR_LoadFlag = 0x09BA350;
        public static ulong ADDR_MenuFlag = 0x0900150;
        public static ulong ADDR_PlayerHP = 0x2A23018;
        public static ulong ADDR_MenuType = 0x09001C4;
        public static ulong ADDR_SaveData = 0x09A9330;
        public static ulong ADDR_MagicLV1 = 0x09AC8C4;
        public static ulong ADDR_MagicLV2 = 0x09AC8FF;
        public static ulong ADDR_ContData = 0x07A0000;
        public static ulong ADDR_FadeValue = 0x0ABAE47;
        public static ulong ADDR_Framerate = 0x08CBD0A;
        public static ulong ADDR_PauseFlag = 0x0717208;
        public static ulong ADDR_ActionExe = 0x2A5C416;
        public static ulong ADDR_MovieFlag = 0x2B56028;
        public static ulong ADDR_IntroMenu = 0x0820200;
        public static ulong ADDR_PromptType = 0x0715380;
        public static ulong ADDR_MenuSelect = 0x0902A40;
        public static ulong ADDR_ReactionID = 0x2A10BE2;
        public static ulong ADDR_ConfigMenu = 0x0820000;
        public static ulong ADDR_BattleFlag = 0x2A10E84;
        public static ulong ADDR_FinishFlag = 0x0ABC0EC;
        public static ulong ADDR_MagicIndex = 0x2A101BC;
        public static ulong ADDR_CampBitwise = 0x0BEE6A0;
        public static ulong ADDR_Viewspace2D = 0x08A0BE8;
        public static ulong ADDR_Viewspace3D = 0x08A0BC0;
        public static ulong ADDR_SubMenuType = 0x0743354;
        public static ulong ADDR_TitleSelect = 0x0B1D064;
        public static ulong ADDR_CommandMenu = 0x05B1868;
        public static ulong ADDR_CommandFlag = 0x07171FC;
        public static ulong ADDR_DialogSelect = 0x0902480;
        public static ulong ADDR_CutsceneMode = 0x0B64C90;
        public static ulong ADDR_CutsceneFlag = 0x07281C0;
        public static ulong ADDR_Framelimiter = 0x0ABA688;
        public static ulong ADDR_ObjentryBase = 0x2A24F50;
        public static ulong ADDR_LimitShortcut = 0x05C97E8;
        public static ulong ADDR_MagicCommands = 0x2A10C08;
        public static ulong ADDR_IntroSelection = 0x0820500;
        public static ulong ADDR_ControllerMode = 0x2B448C8;
        public static ulong ADDR_RenderResolution = 0x08A0BB0;

        public static ulong DATA_BGMPath = 0x05B4E34;
        public static ulong DATA_PAXPath = 0x05C8700;
        public static ulong DATA_ANBPath = 0x05B9140;
        public static ulong DATA_EVTPath = 0x05B91B0;
        public static ulong DATA_BTLPath = 0x05C5FB8;

        public static ulong PINT_Camp2LD = 0x0907170;
        public static ulong PINT_GameOver = 0x0BEEF28;
        public static ulong PINT_SystemMSG = 0x2A110F8;
        public static ulong PINT_ChildMenu = 0x2A10B98;
        public static ulong PINT_EnemyInfo = 0x2A0C7F0;
        public static ulong PINT_EventInfo = 0x2A10EF8;
        public static ulong PINT_ConfigMenu = 0x0BEFBD0;
        public static ulong PINT_PartyLimit = 0x2A24740;
        public static ulong PINT_SaveInformation = 0x2B0C240;
        public static ulong PINT_GameOverOptions = 0x2A10DE0;
        public static ulong PINT_SubMenuOptionSelect = 0x0BEE758;

#endif

        // === DICTIONARIES === //

        public static string[] DICTIONARY_BTL = { "safe", "mob", "boss" };
        public static string[] DICTIONARY_WRL = { "", "", "tt", "", "hb", "bb", "he", "al", "mu", "po", "lk", "lm", "dc", "wi", "nm", "wm", "ca", "tr", "eh" };
        public static string[] DICTIONARY_CPS = { "cup_pp", "cup_cerb", "cup_titan", "cup_god", "cup_hades" };

        public static Dictionary<string, short> DICTIONARY_LMT = new Dictionary<string, short>()
        {
            { "ragnarok", 0x02AB },
            { "arcanum", 0x02BD },
            { "raid", 0x02C0 },
            { "sonic", 0x02BA }
        };

        // === FUNCTION SIGNATURES === //

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
        public static string FUNC_FindFile = "48 89 5C 24 08 57 48 83 EC 20 8B DA 48 8B F9 45 33 C0 4D 85 C0 75 09 4C 8B 05 ?? ?? ?? ??";
        public static string FUNC_GetFileSize = "40 53 48 81 EC 30 01 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 20 01 00 00 48 8D 15 ?? ?? ?? ??";
        public static string FUNC_ShortcutUpdate = "48 83 EC 28 E8 97 F9 FF FF 48 83 C4 28 E9 DE 02 00 00";
        public static string FUNC_ConfigUpdate = "40 53 55 56 57 41 54 41 55 41 56 41 57 48 83 EC 58 E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 4C 8B F8 E8 ?? ?? ?? ??";
        public static string FUNC_SelectUpdate = "48 83 EC 28 48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 63 D0 48 8B 05 ?? ?? ?? ?? 48 0F BE 0C 02";
        public static string FUNC_FadeCampWarning = "48 83 EC 28 85 C9 BA 0B 00 00 00 48 8B 0D ?? ?? ?? ?? B8 08 00 00 00 0F 44 D0 E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ??";
        public static string FUNC_ResetCommandMenu = "40 56 48 83 EC 30 8B 35 ?? ?? ?? ?? E8 ?? ?? ?? ?? 84 C0 0F 85 ?? ?? ?? ?? E8 ?? ?? ?? ?? 84 C0";

        // === HOTFIX SIGNATURES === //

        public static string HFIX_Framelimiter = "F3 0F 10 15 ?? ?? ?? ?? F3 0F 10 0D ?? ?? ?? ?? F3 0F 10 05 ?? ?? ?? ?? F3 0F 59 CA";
        public static string HFIX_ContPrompts = "C7 05 ?? ?? ?? ?? 01 00 00 00 E8 ?? ?? ?? ?? 8B 0D ?? ?? ?? ??";
        public static string HFIX_WarpContinue = "E8 59 00 00 00 48 8B 0D ?? ?? ?? ??";
        public static string HFIX_InventoryReset = "48 8D 15 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? 41 B8 30 08 00 00 48 8D 15 ?? ?? ?? ??";
        public static string HFIX_CommandNavigation = "48 89 5C 24 18 55 41 56 41 57 48 83 EC 20 4C 8B 41 08 48 8B D9";
        public static string HFIX_ShortcutIconAssign = "48 89 5C 24 08 57 48 83 EC 20 48 8B FA 41 0F B6 D9 41 0F B6 D0 E8 ?? ?? ?? ??";
        public static string HFIX_ShortcutListFilter = "48 89 5C 24 18 57 48 83 EC 20 33 DB 48 89 6C 24 30 41 8B F8 48 8B E9";
        public static string HFIX_ShortcutEquipFilter = "48 83 EC 28 E8 ?? ?? ?? ?? 0F B6 48 02 84 C9 74 19";
        public static string HFIX_ShortcutCategoryFilter = "48 89 5C 24 10 48 89 6C 24 18 48 89 74 24 20 57 41 54 41 55 41 56 41 57 48 81 EC 90 01 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 80 01 00 00 33 F6 89 4C 24 28 85 C9 48 8D 05 ?? ?? ?? ??";
        public static string HFIX_FormInventory = "48 89 5C 24 18 57 48 83 EC 20 33 DB 48 89 6C 24 30 41 8B F8 48 8B E9";
        public static string HFIX_VoiceLineCheck = "40 55 56 57 41 54 41 55 41 56 41 57 48 8D 6C 24 E0 48 81 EC 20 01 00 00 48 C7 44 24 60 FE FF FF FF 48 89 9C 24 60 01 00 00 48 8B 05 ?? ?? ?? ??";
        public static string HFIX_SaveRecover = "40 55 53 48 8D 6C 24 B1 48 81 EC C8 00 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 45 3F 48 8B D9 E8 ?? ?? ?? ??";
        public static string HFIX_InfoSound = "48 89 5C 24 18 57 48 81 EC D0 00 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 C0 00 00 00 48 8B DA 48 8B F9";
        public static string HFIX_CampMenuBuild = "48 8B C4 48 81 EC 88 00 00 00 48 89 58 18 BA 02 00 00 00 48 89 68 F8 48 89 70 F0";
        public static string HFIX_CampMenuInit = "C3 CC CC CC CC CC CC CC CC 48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 48 89 7C 24 20 41 56 48 83 EC 40";

        public static string HFIX_ConfigFirst = "40 53 48 83 EC 20 0F B6 D9 48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 4C 8B 1D ?? ?? ?? ??";
        public static string HFIX_ConfigSecond = "48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 57 41 54 41 55 41 56 41 57 48 81 EC 80 00 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 70 E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ??";
        public static string HFIX_ConfigThird = "48 89 5C 24 08 48 89 74 24 10 57 48 83 EC 30 48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 8B F8 E8 ?? ?? ?? ?? 8B F0 83 F8 02 0F 85 ?? ?? ?? ?? E8 ?? ?? ?? ?? 8B D8 E8 ?? ?? ?? ?? 84 C0 75 59 48 8B 0D ?? ?? ?? ??";
        public static string HFIX_ConfigFourth = "48 89 5C 24 08 57 48 83 EC 20 8B FA 8B D9 E8 ?? ?? ?? ?? 8D 0C 3B 44 8D 04 9D 00 00 00 00";
        public static string HFIX_ConfigFifth = "48 89 5C 24 08 48 89 74 24 10 57 48 83 EC 30 E8 ?? ?? ?? ?? 45 33 C0 33 C9 41 8D 50 FF E8 ?? ?? ?? ??";
        public static string HFIX_ConfigSixth = "40 53 55 56 57 41 54 41 55 41 56 41 57 48 83 EC 58 E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 4C 8B F8 E8 ?? ?? ?? ?? 41 BD ?? ?? ?? ??";
        public static string HFIX_ConfigSeventh = "48 89 5C 24 08 48 89 74 24 10 57 48 83 EC 20 33 DB 48 8D 35 ?? ?? ?? ?? 33 FF 66 0F 1F 44 00 00 83 FB 06 0F 87 B5 00 00 00";

        public static string HFIX_IntroFirst = "48 89 5C 24 18 55 56 57 41 54 41 55 41 56 41 57 48 83 EC 50 48 8B 05 ?? ?? ?? ??";
        public static string HFIX_IntroSecond = "48 89 5C 24 20 55 56 57 41 54 41 55 41 56 41 57 48 83 EC 50 4C 8B 3D ?? ?? ?? ??";
        public static string HFIX_IntroThird = "40 53 48 83 EC 20 48 8B 0D ?? ?? ?? ?? 48 81 C1 40 04 00 00 E8 ?? ?? ?? ??";
        public static string HFIX_IntroFourth = "48 83 EC 38 48 8B 0D ?? ?? ?? ?? 48 89 5C 24 40 48 85 C9 74 27 E8 ?? ?? ?? ??";
        public static string HFIX_IntroFifth = "48 89 5C 24 10 57 48 83 EC 40 48 8B 05 ?? ?? ?? ?? 80 78 0C 00 74 18 E8 ?? ?? ?? ??";
        public static string HFIX_IntroSixth = "48 89 5C 24 20 56 57 41 56 48 83 EC 30 E8 ?? ?? ?? ?? 48 8B C8 E8 ?? ?? ?? ??";
        public static string HFIX_IntroSeventh = "48 83 EC 28 E8 ?? ?? ?? ?? 8B 15 ?? ?? ?? ?? 48 8B C8 E8 ?? ?? ?? ??";

        public static List<ulong> HFIX_ConfigOffsets = new List<ulong>();
        public static List<ulong> HFIX_IntroOffsets = new List<ulong>();

        // === ERROR STRINGS === // 

        public static string ERROR_403 = "{0} cannot access any file that's in your Documents folder!\n" +
                                         "Make sure your Documents folder is actually valid, and your Anti-Virus is not interfering with {0}." +
                                         "(Yes, Windows Security counts as an Anti-Virus. No, you probably did not disable it.)" +
                                         "{0} will now terminate.";

        public static string ERROR_404 = "{0} was not able to locate the base patch!\n" +
                                         "Please ensure that it is installed correctly!\n" +
                                         "{0} will now terminate.";

        public static string ERROR_420 = "The Keystone Assembler has crashed due to an error!\n" +
                                         "This may be caused by an antivirus software meddling with the library.\n" +
                                         "{0} cannot continue operation, and will thus terminate.";


        public static string ERROR_430 = "03SYSTEM.BIN is corrupt!\n" +
                                         "This usually happens because the patch was installed with OpenKH without an extracted game!\n" +
                                         "Please correct this error and try again!\n" +
                                         "{0} will now terminate.";

        public static string ERROR_550 = "One of the necessary libraries for {0}'s functionality is missing!\n" +
                                         "Please ensure that {0} was installed and/or extracted correctly, and all DLL files are present.\n" +
                                         "{0} will now terminate.";

        public static string ERROR_600 = "{0} has performed an illegal operation and must terminate!\n" +
                                         "Please send the log file found in the game directory to the Re:Fined Discord Server.";        
       


        // === ENUMERABLES === //

        public enum BATTLE_TYPE : byte
        {
            PEACEFUL = 0x00,
            FIELD = 0x01,
            BOSS = 0x02
        };

        public enum BUTTON : ushort
        {
            NONE = 0x0000,
            SELECT = 0x0001,
            START = 0x0008,
            TRIANGLE = 0x1000,
            CIRCLE = 0x2000,
            CROSS = 0x4000,
            SQUARE = 0x8000,
            L1 = 0x0400,
            R1 = 0x0800,
            L2 = 0x0100,
            R2 = 0x0200,
            L3 = 0x0002,
            R3 = 0x0004,
            UP = 0x0010,
            RIGHT = 0x0020,
            DOWN = 0x0040,
            LEFT = 0x0080
        }

        public enum CAMP_BITWISE : byte
        {
            ITEMS = 0x01,
            ABILITIES = 0x02,
            CUSTOMIZE = 0x04,
            PARTY = 0x08, 
            STATUS = 0x10, 
            JIMINY_JOURNAL = 0x20,
            ROXAS_JOURNAL = 0x40,
            CONFIG = 0x80
        }

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
            AUDIO_PRIMARY = 0x0800,
            AUDIO_SECONDARY = 0x1000,
            PROMPT_CONTROLLER = 0x2000,
            MUSIC_VANILLA = 0x4000,
            HEARTLESS_VANILLA = 0x8000
        }
    }
}
