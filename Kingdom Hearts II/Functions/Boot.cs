using ReFined.Common;
using ReFined.Libraries;
using ReFined.KH2.Menus;
using ReFined.KH2.Information;
using ReFined.KH2.InGame;

using Binarysharp.MSharp;

namespace ReFined.KH2.Functions
{
    public static class Boot
    {
        public static void Initialization()
        {
            var _versionString = "Re:Fined";

            try
            {
                Configuration.Initialize();
                var _configIni = new INI("reFined.cfg");

                Variables.DISCORD_TOGGLE = Convert.ToBoolean(_configIni.Read("discordRPC", "General"));
                Variables.IS_LITE = Convert.ToBoolean(_configIni.Read("liteMode", "General"));
                Variables.RESET_PROMPT = Convert.ToBoolean(_configIni.Read("resetPrompt", "Kingdom Hearts II"));
                Variables.RESET_COMBO = Convert.ToUInt16(_configIni.Read("resetCombo", "General"), 16);
                Variables.RETRY_DEFAULT = _configIni.Read("deathPrompt", "Kingdom Hearts II") == "retry" ? true : false;

                _versionString = Variables.IS_LITE ? "Re:Freshed" : "Re:Fined";

                Terminal.Log("Welcome to " + _versionString + " Patreon BETA v0.50!", 0);
                Terminal.Log("Please be patient while " + _versionString + " initializes.", 0);

                Terminal.Log("Initializing SharpHook...", 0);
                Variables.SharpHook = new MemorySharp(Hypervisor.Process);
                Variables.DiscordClient.Initialize();

                Terminal.Log("Locating Function Signatures...", 0);

                Message.OffsetMenu = Hypervisor.FindSignature(Variables.FUNC_SetMenuType);
                Message.OffsetInfo = Hypervisor.FindSignature(Variables.FUNC_ShowInformation);
                Message.OffsetObtained = Hypervisor.FindSignature(Variables.FUNC_ShowObatined);

                Message.OffsetSetSLWarning = Hypervisor.FindSignature(Variables.FUNC_SetSLWarning);
                Message.OffsetShowSLWarning = Hypervisor.FindSignature(Variables.FUNC_ShowSLWarning);
                Message.OffsetSetCampWarning = Hypervisor.FindSignature(Variables.FUNC_SetCampWarning);
                Message.OffsetShowCampWarning = Hypervisor.FindSignature(Variables.FUNC_ShowCampWarning);

                Critical.OffsetCampMenu = Hypervisor.FindSignature(Variables.FUNC_ExecuteCampMenu);
                Critical.OffsetShutMusic = Hypervisor.FindSignature(Variables.FUNC_StopBGM);
                Critical.OffsetMapJump = Hypervisor.FindSignature(Variables.FUNC_MapJump);
                Critical.OffsetSetFadeOff = Hypervisor.FindSignature(Variables.FUNC_SetFadeOff);

                Operations.OffsetFindFile = Hypervisor.FindSignature(Variables.FUNC_FindFile);
                Operations.OffsetGetFileSize = Hypervisor.FindSignature(Variables.FUNC_GetFileSize);

                Sound.OffsetSound = Hypervisor.FindSignature(Variables.FUNC_PlaySFX);

                Terminal.Log("Locating Hotfix Signatures...", 0);

                Continuous.LIMITER_OFFSET = (ulong)Hypervisor.FindSignature(Variables.HFIX_Framelimiter);
                Continuous.PROMPT_OFFSET = (ulong)Hypervisor.FindSignature(Variables.HFIX_ContPrompts);
                
                Critical.INVT_OFFSET = (ulong)Hypervisor.FindSignature(Variables.HFIX_InventoryReset);
                Critical.WARP_OFFSET = (ulong)Hypervisor.FindSignature(Variables.HFIX_WarpContinue);
                Critical.CMD_OFFSET = (ulong)Hypervisor.FindSignature(Variables.HFIX_CommandNavigation);

                Critical.ICON_OFFSET = (ulong)Hypervisor.FindSignature(Variables.HFIX_ShortcutIconAssign);
                Critical.LIST_OFFSET = (ulong)Hypervisor.FindSignature(Variables.HFIX_ShortcutListFilter);
                Critical.EQUIP_OFFSET = (ulong)Hypervisor.FindSignature(Variables.HFIX_ShortcutEquipFilter);
                Critical.CATEGORY_OFFSET = (ulong)Hypervisor.FindSignature(Variables.HFIX_ShortcutCategoryFilter);

                Critical.WARP_FUNCTION = Hypervisor.Read<byte>(Critical.WARP_OFFSET, 0x05);
                Critical.INVT_FUNCTION = Hypervisor.Read<byte>(Critical.INVT_OFFSET, 0x07);
                
                Terminal.Log("Locating Hotfix Signatures for the Menus...", 0);

                Variables.HFIX_ConfigOffsets.Add((ulong)Hypervisor.FindSignature(Variables.HFIX_ConfigFirst));
                Variables.HFIX_ConfigOffsets.Add((ulong)Hypervisor.FindSignature(Variables.HFIX_ConfigSecond));
                Variables.HFIX_ConfigOffsets.Add((ulong)Hypervisor.FindSignature(Variables.HFIX_ConfigThird));
                Variables.HFIX_ConfigOffsets.Add((ulong)Hypervisor.FindSignature(Variables.HFIX_ConfigFourth));
                Variables.HFIX_ConfigOffsets.Add((ulong)Hypervisor.FindSignature(Variables.HFIX_ConfigFifth));
                Variables.HFIX_ConfigOffsets.Add((ulong)Hypervisor.FindSignature(Variables.HFIX_ConfigSixth));

                Variables.HFIX_IntroOffsets.Add((ulong)Hypervisor.FindSignature(Variables.HFIX_IntroFirst));
                Variables.HFIX_IntroOffsets.Add((ulong)Hypervisor.FindSignature(Variables.HFIX_IntroSecond));
                Variables.HFIX_IntroOffsets.Add((ulong)Hypervisor.FindSignature(Variables.HFIX_IntroThird));
                Variables.HFIX_IntroOffsets.Add((ulong)Hypervisor.FindSignature(Variables.HFIX_IntroFourth));
                Variables.HFIX_IntroOffsets.Add((ulong)Hypervisor.FindSignature(Variables.HFIX_IntroFifth));
                Variables.HFIX_IntroOffsets.Add((ulong)Hypervisor.FindSignature(Variables.HFIX_IntroSixth));
                Variables.HFIX_IntroOffsets.Add((ulong)Hypervisor.FindSignature(Variables.HFIX_IntroSeventh));

                Variables.INTRO_MENU = new Intro();
                Variables.CONFIG_MENU = new Config();
                Variables.CONTINUE_MENU = new Continue();

                if (!Variables.IS_LITE)
                {
                    Terminal.Log("Initializing Extra Options in the menus...", 0);

                    Variables.FORM_SHORTCUT = Convert.ToBoolean(_configIni.Read("driveShortcuts", "Kingdom Hearts II"));

                    if (Operations.GetFileSize("obj/V_BB100.mdlx") != 0x00)
                    {
                        Terminal.Log("An enemy palette pack was located! Adding the options for it...", 0);

                        var _entConfig = new Config.Entry(2, 0x011D, [0x011E, 0x0120], [0x011F, 0x0121]);
                        var _entIntro = new Intro.Entry(2, 0x0136, 0x0000, [0x011E, 0x0120], [0x011F, 0x0121]);

                        Variables.CONFIG_MENU.Children.Insert(9, _entConfig);
                        Variables.INTRO_MENU.Children.Insert(2, _entIntro);

                    }

                    if (Operations.GetFileSize("bgm/ps2md050.win32.scd") != 0x00)
                    {
                        Terminal.Log("A music pack was located! Adding the options for it...", 0);

                        var _entConfig = new Config.Entry(2, 0x0118, [0x0119, 0x011B], [0x011A, 0x011C]);
                        var _entIntro = new Intro.Entry(2, 0x0135, 0x0000, [0x0119, 0x011B], [0x011A, 0x011C]);

                        Variables.CONFIG_MENU.Children.Insert(9, _entConfig);
                        Variables.INTRO_MENU.Children.Insert(2, _entIntro);

                    }

                    var _entRoxas = new Intro.Entry(2, 0x0130, 0x0000, [0x0138, 0x0139], [0x0131, 0x0132]);
                    Variables.INTRO_MENU.Children.Add(_entRoxas);
                }

                Variables.Source = new CancellationTokenSource();
                Variables.Token = Variables.Source.Token;

                Variables.Initialized = true;

                Terminal.Log(_versionString + " initialized with no errors!", 0);
            }

            catch (Exception ERROR)
            {
                Terminal.Log(ERROR);
                Terminal.Log(_versionString + " terminated with an exception!", 1);
                Environment.Exit(-1);
            }
        }
    }
}