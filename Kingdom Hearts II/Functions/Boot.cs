using ReFined.Common;
using ReFined.Libraries;
using ReFined.KH2.Menus;
using ReFined.KH2.Information;
using ReFined.KH2.InGame;

using Binarysharp.MSharp;
using System.Linq;

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
                Variables.LIMIT_SHORTS = _configIni.Read("limitShortcuts", "Kingdom Hearts II");

                _versionString = Variables.IS_LITE ? "Re:Freshed" : "Re:Fined";

                Terminal.Log("Welcome to " + _versionString + " Patreon BETA v0.75!", 0);
                Terminal.Log("Please be patient while " + _versionString + " initializes.", 0);

                Terminal.Log("Initializing SharpHook...", 0);
                Variables.SharpHook = new MemorySharp(Hypervisor.Process);
                Variables.DiscordClient.Initialize();

                Terminal.Log("Locating Function Signatures...", 0);

                Demand.OffsetShortcutUpdate = Hypervisor.FindSignature(Variables.FUNC_ShortcutUpdate);

                Message.OffsetMenu = Hypervisor.FindSignature(Variables.FUNC_SetMenuType);
                Message.OffsetInfo = Hypervisor.FindSignature(Variables.FUNC_ShowInformation);
                Message.OffsetObtained = Hypervisor.FindSignature(Variables.FUNC_ShowObatined);

                Message.OffsetSetSLWarning = Hypervisor.FindSignature(Variables.FUNC_SetSLWarning);
                Message.OffsetShowSLWarning = Hypervisor.FindSignature(Variables.FUNC_ShowSLWarning);
                Message.OffsetSetCampWarning = Hypervisor.FindSignature(Variables.FUNC_SetCampWarning);
                Message.OffsetShowCampWarning = Hypervisor.FindSignature(Variables.FUNC_ShowCampWarning);
                Message.OffsetFadeCampWarning = Hypervisor.FindSignature(Variables.FUNC_FadeCampWarning);

                Critical.OffsetCampMenu = Hypervisor.FindSignature(Variables.FUNC_ExecuteCampMenu);
                Critical.OffsetShutMusic = Hypervisor.FindSignature(Variables.FUNC_StopBGM);
                Critical.OffsetMapJump = Hypervisor.FindSignature(Variables.FUNC_MapJump);
                Critical.OffsetSetFadeOff = Hypervisor.FindSignature(Variables.FUNC_SetFadeOff);
                Critical.OffsetConfigUpdate= Hypervisor.FindSignature(Variables.FUNC_ConfigUpdate);
                Critical.OffsetSelectUpdate = Hypervisor.FindSignature(Variables.FUNC_SelectUpdate);

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
                Critical.FORM_OFFSET = (ulong)Hypervisor.FindSignature(Variables.HFIX_FormInventory);

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

                if (Operations.GetFileSize("itempic/item-271.imd") == 0x00) ;
                if (Operations.GetFileSize("03system.bin") == 0x00) ;

                Variables.INTRO_MENU = new Intro();
                Variables.CONFIG_MENU = new Config();
                Variables.CONTINUE_MENU = new Continue();

                if (!Variables.IS_LITE)
                {
                    Variables.LOADED_LANGS.Add(Operations.GetFileSize("voice/jp/battle/tt0_sora.win32.scd") != 0x00 ? "JP" : "JP");
                    Variables.LOADED_LANGS.Add(Operations.GetFileSize("voice/de/battle/tt0_sora.win32.scd") != 0x00 ? "" : "");
                    Variables.LOADED_LANGS.Add(Operations.GetFileSize("voice/es/battle/tt0_sora.win32.scd") != 0x00 ? "ES" : "ES");
                    Variables.LOADED_LANGS.Add(Operations.GetFileSize("voice/bg/battle/tt0_sora.win32.scd") != 0x00 ? "" : "");

                    ushort[][] _entryJP = !String.IsNullOrEmpty(Variables.LOADED_LANGS[0]) ? [[0x010E], [0x010F]] : [[], []];
                    ushort[][] _entryDE = !String.IsNullOrEmpty(Variables.LOADED_LANGS[1]) ? [[0x0112], [0x0113]] : [[], []];
                    ushort[][] _entryES = !String.IsNullOrEmpty(Variables.LOADED_LANGS[2]) ? [[0x0110], [0x0111]] : [[], []];
                    ushort[][] _entryFR = !String.IsNullOrEmpty(Variables.LOADED_LANGS[3]) ? [[0x0114], [0x0115]] : [[], []];

                    ushort _subDesc = 0x0117;
                    ushort _subLabel = 0x0116;

                    var _cfgAudioSub = new Config.Entry(0, 0x012B, [], []);
                    var _cfgAudioMain = new Config.Entry(0, 0x010B, [0x010C], [0x010D]);

                    var _intAudioSub = new Intro.Entry(0, 0x013A, 0x0000, [], []);
                    var _intAudioMain = new Intro.Entry(0, 0x0134, 0x0000, [0x010C], [0x010D]);

                    _cfgAudioSub.Buttons.AddRange(_entryDE[0]);
                    _cfgAudioSub.Buttons.AddRange(_entryES[0]);
                    _cfgAudioSub.Buttons.AddRange(_entryFR[0]);
                    
                    _cfgAudioSub.Descriptions.AddRange(_entryDE[1]);
                    _cfgAudioSub.Descriptions.AddRange(_entryES[1]);
                    _cfgAudioSub.Descriptions.AddRange(_entryFR[1]);

                    _intAudioSub.Buttons = _cfgAudioSub.Buttons.Select(x => (uint)x).ToList();
                    _intAudioSub.Descriptions = _cfgAudioSub.Descriptions.Select(x => (uint)x).ToList();

                    _cfgAudioMain.Buttons.AddRange(_entryJP[0]);
                    _cfgAudioMain.Descriptions.AddRange(_entryJP[1]);

                    if (_cfgAudioSub.Buttons.Count == 0x01)
                    {
                        _cfgAudioMain.Buttons.Add(_cfgAudioSub.Buttons[0]);
                        _cfgAudioMain.Descriptions.Add(_cfgAudioSub.Descriptions[0]);
                    }

                    else if (_cfgAudioSub.Buttons.Count == 0x02 && String.IsNullOrEmpty(Variables.LOADED_LANGS[0]))
                    {
                        _cfgAudioMain.Buttons.Add(_cfgAudioSub.Buttons[0]);
                        _cfgAudioMain.Descriptions.Add(_cfgAudioSub.Descriptions[0]);
                        _cfgAudioMain.Buttons.Add(_cfgAudioSub.Buttons[1]);
                        _cfgAudioMain.Descriptions.Add(_cfgAudioSub.Descriptions[1]);
                    }

                    else if (_cfgAudioSub.Buttons.Count > 0x01)
                    {
                        _cfgAudioMain.Buttons.Add(0x0116);
                        _cfgAudioMain.Descriptions.Add(0x0117);

                        if (String.IsNullOrEmpty(Variables.LOADED_LANGS[0]))
                            Critical.AUDIO_SUB_ONLY = true;
                    }

                    _intAudioMain.Buttons = _cfgAudioMain.Buttons.Select(x => (uint)x).ToList();
                    _intAudioMain.Descriptions = _cfgAudioMain.Descriptions.Select(x => (uint)x).ToList();

                    _cfgAudioSub.Count = (ushort)_cfgAudioSub.Buttons.Count;
                    _cfgAudioMain.Count = (ushort)_cfgAudioMain.Buttons.Count;                    
                    _intAudioSub.Count = (ushort)_intAudioSub.Buttons.Count;
                    _intAudioMain.Count = (ushort)_intAudioMain.Buttons.Count;

                    Variables.LOADED_LANGS.RemoveAll(x => x == "");

                    Terminal.Log("Initializing Extra Options in the menus...", 0);
                    Terminal.Log("Languages Found: " + String.Join(", ", Variables.LOADED_LANGS), 0);

                    Variables.FORM_SHORTCUT = Convert.ToBoolean(_configIni.Read("driveShortcuts", "Kingdom Hearts II"));

                    if (Operations.GetFileSize("obj/V_BB100.mdlx") != 0x00)
                    {
                        Terminal.Log("An Enemy Palette Pack was located! Adding the options for it...", 0);

                        var _entConfig = new Config.Entry(2, 0x011D, [0x011E, 0x0120], [0x011F, 0x0121]);
                        var _entIntro = new Intro.Entry(2, 0x0136, 0x0000, [0x011E, 0x0120], [0x011F, 0x0121]);

                        Variables.INTRO_MENU.Children.Insert(2, _entIntro);
                        Variables.CONFIG_MENU.Children.Insert(9, _entConfig);
                    }

                    if (Operations.GetFileSize("bgm/ps2md050.win32.scd") != 0x00)
                    {
                        Terminal.Log("A Music Pack was located! Adding the options for it...", 0);

                        var _entConfig = new Config.Entry(2, 0x0118, [0x0119, 0x011B], [0x011A, 0x011C]);
                        var _entIntro = new Intro.Entry(2, 0x0135, 0x0000, [0x0119, 0x011B], [0x011A, 0x011C]);

                        Variables.INTRO_MENU.Children.Insert(4, _entIntro);
                        Variables.CONFIG_MENU.Children.Insert(9, _entConfig);
                    }

                    if (_cfgAudioMain.Count > 0x01)
                    {
                        Terminal.Log("At least one Language Pack was located! Adding the options for it...", 0);

                        Variables.INTRO_MENU.Children.Insert(4, _intAudioMain);
                        Variables.CONFIG_MENU.Children.Insert(9, _cfgAudioMain);
                    }

                    if ((_cfgAudioSub.Count > 0x01 && Variables.LOADED_LANGS[0] == "JP") || 
                        (_cfgAudioSub.Count > 0x02 && Variables.LOADED_LANGS[0] != "JP"))
                    {
                        Terminal.Log("More than one European Language Pack located! Adjusting the options...", 0);

                        Variables.AUDIO_SUB_INTRO = _intAudioSub;
                        Variables.AUDIO_SUB_CONFIG = _cfgAudioSub;
                    }

                    var _entRoxas = new Intro.Entry(2, 0x0130, 0x0000, [0x0138, 0x0139], [0x0131, 0x0132]);
                    Variables.INTRO_MENU.Children.Add(_entRoxas);
                }

                if (Variables.LIMIT_SHORTS != "")
                {
                    Continuous.LIMIT_SHORT = new short[4];

                    var _splitArr = Variables.LIMIT_SHORTS.Replace("[", "").Replace("]", "").Replace(", ", ",").Split(',');

                    Continuous.LIMIT_SHORT[0] = Variables.DICTIONARY_LMT[_splitArr[0]];
                    Continuous.LIMIT_SHORT[1] = Variables.DICTIONARY_LMT[_splitArr[1]];
                    Continuous.LIMIT_SHORT[2] = Variables.DICTIONARY_LMT[_splitArr[2]];
                    Continuous.LIMIT_SHORT[3] = Variables.DICTIONARY_LMT[_splitArr[3]];
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
                Console.ReadKey(true);
                Environment.Exit(-1);
            }
        }
    }
}