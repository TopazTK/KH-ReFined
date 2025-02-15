﻿using ReFined.Common;
using ReFined.Libraries;
using ReFined.KH2.Menus;
using ReFined.KH2.InGame;
using ReFined.KH2.Information;

using BSharpConvention = Binarysharp.MSharp.Assembly.CallingConvention.CallingConventions;

namespace ReFined.KH2.Functions
{
    public static class Critical
    {
        public static IntPtr OffsetMapJump;
        public static IntPtr OffsetSetFadeOff;
        public static IntPtr OffsetConfigUpdate;
        public static IntPtr OffsetSelectUpdate;
        public static IntPtr OffsetItemUpdate;

        public static ulong WARP_OFFSET;
        public static ulong INVT_OFFSET;
        public static ulong CMD_OFFSET;
        public static ulong CAMP_OFFSET;
        public static ulong CAMP_INIT_OFFSET;

        public static ulong ICON_OFFSET;
        public static ulong LIST_OFFSET;
        public static ulong EQUIP_OFFSET;
        public static ulong FORM_OFFSET;
        public static ulong CATEGORY_OFFSET;

        public static byte[] CAMP_FUNCTION;
        public static byte[] CAMP_INIT_FUNCTION;

        public static byte[] WARP_FUNCTION;
        public static byte[] INVT_FUNCTION;
        public static bool AUDIO_SUB_ONLY;

        static bool SUB_INTRO_ACTIVE;
        static bool SUB_CONFIG_ACTIVE;

        static bool CONFIG_TOGGLE;
        static bool CONFIG_WRITTEN;
        static Variables.CONFIG_BITWISE CONFIG_BIT;

        static bool RETRY_HOLD;

        static bool[] DEBOUNCE = new bool[0x20];

        static ushort SAVE_ROOM;
        static ushort SAVE_WORLD;
        static byte SAVE_ITERATOR;

        static bool LOADED_SETTINGS;

        static DateTime START_DELAY;
        static bool DELAY_ULTRAWIDE = false;
        static int POSITIVE_OFFSET = 0x55;
        static int NEGATIVE_OFFSET = -0x55;

        static List<byte>? SETTINGS_READ;
        static byte[]? SETTINGS_WRITE;

        static bool ENTER_CONFIG;
        public static bool LOCK_AUTOSAVE;

        static bool STATE_COPIED;
        static byte HADES_COUNT;
        static ushort ROXAS_KEYBLADE;
        static byte RETRY_MODE;
        static byte PREPARE_MODE;
        static bool RETRY_BLOCK;

        static byte[] MAGIC_STORE;
        public static byte[] AREA_READ;

        static uint[] MAGIC_OFFSET = [0x1B1, 0x295, 0x2BD, 0x30C, 0x33C];
        static List<byte[]> MAGIC_INST;

        static uint MAGIC_LV1;
        static ushort MAGIC_LV2;
        static bool ROOM_LOADED;

        static int INDEX_ABSOLUTION = 0xFF;
        static int INDEX_RETRIBUTION = 0xFF;

        static ushort PARAM_ABSOLUTION = 0xFFFF;
        static ushort PARAM_RETRIBUTION = 0xFFFF;

        static ulong RETRIBUTION_RAM;
        static ulong ABSOLUTION_RAM;

        public static List<ushort> PARAMS_ALL = new List<ushort>();
        static List<ushort> PARAMS_LIST = new List<ushort>();

        static ulong[] LOAD_LIST = new ulong[7];
        static byte PAST_FORM;

        public static void HandleMagicSort()
        {
            var _menuPointer = Hypervisor.Read<ulong>(Variables.PINT_ChildMenu);

            var _magicOne = Hypervisor.Read<uint>(Variables.ADDR_MagicLV1);
            var _magicTwo = Hypervisor.Read<ushort>(Variables.ADDR_MagicLV2);

            var _readMagic = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0xE500, 0x0C);
            var _firstMagic = BitConverter.ToUInt16(_readMagic, 0x00);

            if (MAGIC_INST == null)
            {
                MAGIC_INST =
                [
                    Hypervisor.Read<byte>(CMD_OFFSET + MAGIC_OFFSET[0], 0x03),
                    Hypervisor.Read<byte>(CMD_OFFSET + MAGIC_OFFSET[1], 0x03),
                    Hypervisor.Read<byte>(CMD_OFFSET + MAGIC_OFFSET[2], 0x03),
                    Hypervisor.Read<byte>(CMD_OFFSET + MAGIC_OFFSET[3], 0x03),
                    Hypervisor.Read<byte>(CMD_OFFSET + MAGIC_OFFSET[4], 0x03),
                ];
            }

            if (Variables.IS_LOADED && MAGIC_STORE == null && _firstMagic != 0x00)
            {
                Terminal.Log("Detected a saved Magic sort! Reloading...", 0);
                MAGIC_STORE = _readMagic;
            }

            if (_magicOne != MAGIC_LV1 || _magicTwo != MAGIC_LV2)
            {
                Terminal.Log("Spell change detected! Resetting sort memory.", 1);

                MAGIC_STORE = null;

                MAGIC_LV1 = _magicOne;
                MAGIC_LV2 = _magicTwo;

                _readMagic = new byte[0x0C];
                Hypervisor.Write(Variables.ADDR_SaveData + 0xE500, _readMagic);

                _firstMagic = 0x00;
            }

            if (Variables.IS_TITLE)
                MAGIC_STORE = null; 

            if (Variables.IS_LOADED && ROOM_LOADED && _menuPointer != 0x00)
            {
                if (MAGIC_STORE != null)
                {
                    Terminal.Log("Roomchange detected! Restoring the Magic Menu.", 1);
                    Hypervisor.Write(Variables.ADDR_MagicCommands, MAGIC_STORE);
                }

                ROOM_LOADED = false;
            }

            else if (!Variables.IS_LOADED && !ROOM_LOADED)
                ROOM_LOADED = true;

            if (_menuPointer != 0x00 && Variables.IS_LOADED)
            {
                var _menuRead = Hypervisor.Read<byte>(_menuPointer, true);

                if (_menuRead == 0x01)
                {
                    var _magicIndex = Hypervisor.Read<byte>(Variables.ADDR_MagicIndex);
                    var _magicMax = Hypervisor.Read<byte>(_menuPointer + 0x10, true);

                    var _pressDigital = Variables.IS_PRESSED(Variables.BUTTON.R2 | Variables.BUTTON.UP) ? 0x01 : (Variables.IS_PRESSED(Variables.BUTTON.R2 | Variables.BUTTON.DOWN) ? 0x02 : 0x00);
                    var _pressAnalog = Variables.IS_PRESSED(Variables.BUTTON.R2) && Variables.IS_PRESSED(Variables.ANALOG.R_UP) ? 0x01 : (Variables.IS_PRESSED(Variables.BUTTON.R2) && Variables.IS_PRESSED(Variables.ANALOG.R_DOWN) ? 0x02 : 0x00);

                    var _inputCheck = (Hypervisor.Read<Variables.CONFIG_BITWISE>(Variables.ADDR_Config) & Variables.CONFIG_BITWISE.RIGHT_STICK) == Variables.CONFIG_BITWISE.RIGHT_STICK ? _pressAnalog : _pressDigital;

                    var _triggerCheck = Variables.IS_PRESSED(Variables.BUTTON.R2);

                    var _insCheck = Hypervisor.Read<byte>(CMD_OFFSET + 0x1B1);

                    if (_triggerCheck && _insCheck != 0x90)
                    {
                        Terminal.Log("R2 Detected within Magic Menu! Disabling input registry.", 1);

                        foreach (var _off in MAGIC_OFFSET)
                        Hypervisor.DeleteInstruction(CMD_OFFSET + _off, 0x03);
                    }

                    else if (!_triggerCheck && _insCheck == 0x90)
                    {
                        Terminal.Log("R2 has been let go! Enabling input registry.", 1);

                        for (int i = 0; i < MAGIC_OFFSET.Length; i++)
                            Hypervisor.Write(CMD_OFFSET + MAGIC_OFFSET[i], MAGIC_INST[i]);
                    }

                    if (!DEBOUNCE[1] && _inputCheck != 0x00)
                    {
                        DEBOUNCE[1] = true;

                        var _magicPointer = (0x02 * _magicIndex);
                        var _magicBounds = _magicPointer + (_inputCheck == 0x01 ? -0x02 : 0x02);

                        var _subjectMagic = Hypervisor.Read<ushort>(Variables.ADDR_MagicCommands + (ulong)_magicPointer);
                        var _targetMagic = _magicBounds >= 0 ? Hypervisor.Read<ushort>(Variables.ADDR_MagicCommands + (ulong)_magicBounds) : (ushort)0x0000;

                        if (_targetMagic != 0x0000)
                        {
                            Hypervisor.Write(Variables.ADDR_MagicCommands + (ulong)_magicPointer, _targetMagic);
                            Hypervisor.Write(Variables.ADDR_MagicCommands + (ulong)_magicBounds, _subjectMagic);

                            Hypervisor.Write(Variables.ADDR_MagicIndex, _magicIndex + (_inputCheck == 0x01 ? -0x01 : 0x01));
                            Hypervisor.Write(Variables.ADDR_MagicIndex + 0x04, _subjectMagic);

                            Terminal.Log(String.Format("Moving Magic ID \"{0}\" {1} within the menu!", "0x" + _subjectMagic.ToString("X4"), _inputCheck == 0x01 ? "up" : "down"), 0);

                            MAGIC_STORE = Hypervisor.Read<byte>(Variables.ADDR_MagicCommands, _magicMax * 0x02);
                            Hypervisor.Write(Variables.ADDR_SaveData + 0xE500, MAGIC_STORE);

                        }

                        else
                            Terminal.Log("Could not move the spell out of bounds!", 1);
                    }

                    else if (DEBOUNCE[1] && _inputCheck == 0x00)
                        DEBOUNCE[1] = false;
                }
            }

            else
            {
                for (int i = 0; i < MAGIC_OFFSET.Length; i++)
                    Hypervisor.Write(CMD_OFFSET + MAGIC_OFFSET[i], MAGIC_INST[i]);
            }
        }

        public static void HandleAutosave()
        {
            var _worldCheck = Hypervisor.Read<byte>(Variables.ADDR_Area);
            var _pauseCheck = Hypervisor.Read<byte>(Variables.ADDR_PauseFlag);
            var _roomCheck = Hypervisor.Read<byte>(Variables.ADDR_Area + 0x01);
            var _titleCheck = Hypervisor.Read<byte>(Variables.ADDR_TitleSelect);

            var _blacklistCheck =
                (_worldCheck == 0x08 && _roomCheck == 0x03) ||
                (_worldCheck == 0x0C && _roomCheck == 0x02) ||
                (_worldCheck == 0x12 && _roomCheck >= 0x13 && _roomCheck <= 0x1D) ||
                (_worldCheck == 0x02 && _roomCheck <= 0x01) ||
                (_worldCheck == 0x04 && _roomCheck == 0x10);

            if (!Variables.IS_TITLE && Variables.IS_LOADED && !_blacklistCheck)
            {
                Thread.Sleep(10);

                var _saveableBool = Variables.SAVE_MODE != 0x02 &&
                                   !Variables.IS_CUTSCENE && Variables.IS_LOADED &&
                                    Variables.BATTLE_MODE == Variables.BATTLE_TYPE.PEACEFUL &&
                                    _titleCheck < 0x02 &&
                                    _worldCheck >= 0x02 &&
                                    _pauseCheck == 0x01 &&
                                    LOCK_AUTOSAVE == false;

                if (_saveableBool)
                {
                    if (SAVE_WORLD != _worldCheck && _worldCheck != 0x07)
                    {
                        Terminal.Log("Attempting to Autosave.", 0);

                        Generators.GenerateSave();
                        SAVE_ITERATOR = 0;
                    }

                    else if (SAVE_ROOM != _roomCheck && _worldCheck >= 2)
                    {
                        SAVE_ITERATOR++;

                        if (SAVE_ITERATOR == 3)
                        {
                            Terminal.Log("Attempting to Autosave.", 0);

                            Generators.GenerateSave();
                            SAVE_ITERATOR = 0;
                        }
                    }

                    SAVE_WORLD = _worldCheck;
                    SAVE_ROOM = _roomCheck;
                }
            }
        }

        public static void HandleIntro()
        {
            var _subAudio = 0x00;
            var _selectButton = Hypervisor.Read<byte>(Variables.ADDR_TitleSelect);

            CONFIG_TOGGLE = _selectButton == 0x00 ? true : false;

            if (Variables.IS_TITLE)
            {
                if (CONFIG_WRITTEN)
                {
                    Terminal.Log("Back on Title Screen! Flushing Intro configuration...", 0x00);
                    CONFIG_BIT = 0x00;
                    CONFIG_WRITTEN = false;
                }

                var _audioMode = Variables.CONFIG_BITWISE.OFF;
                var _musicMode = Variables.CONFIG_BITWISE.OFF;
                var _enemyMode = Variables.CONFIG_BITWISE.OFF;

                var _readState = Hypervisor.Read<int>(Variables.ADDR_IntroSelection, 0x100);

                var _vibration = _readState[0x01] == 0x00 ? Variables.CONFIG_BITWISE.VIBRATION : Variables.CONFIG_BITWISE.OFF;

                var _autoSave = _readState[0x02] == 0x00 ? Variables.CONFIG_BITWISE.AUTOSAVE_INDICATOR :
                               (_readState[0x02] == 0x01 ? Variables.CONFIG_BITWISE.AUTOSAVE_SILENT : Variables.CONFIG_BITWISE.OFF);

                var _controlPrompt = _readState[0x03] == 0x00 ? Variables.CONFIG_BITWISE.PROMPT_CONTROLLER : Variables.CONFIG_BITWISE.OFF;

                if (!Variables.IS_LITE)
                {
                    var _addonOffset = 0;
                    var _fetchIndex = AUDIO_SUB_ONLY ? Variables.CONFIG_BITWISE.AUDIO_PRIMARY : Variables.CONFIG_BITWISE.AUDIO_SECONDARY;

                    var _audioActive = Variables.INTRO_MENU.Children.FirstOrDefault(x => x.Flair == 0x5734) == null ? false : true;
                    var _musicActive = Variables.INTRO_MENU.Children.FirstOrDefault(x => x.Flair == 0x5735) == null ? false : true;
                    var _enemyActive = Variables.INTRO_MENU.Children.FirstOrDefault(x => x.Flair == 0x5736) == null ? false : true;

                    if (_audioActive)
                    {
                        _audioMode = _readState[0x04] == 0x01 ? Variables.CONFIG_BITWISE.AUDIO_PRIMARY :
                                    (_readState[0x04] == 0x02 ? Variables.CONFIG_BITWISE.AUDIO_SECONDARY : Variables.CONFIG_BITWISE.OFF);

                        if (Variables.AUDIO_SUB_INTRO != null)
                        {
                            if (_audioMode == _fetchIndex && !SUB_INTRO_ACTIVE)
                            {
                                Variables.INTRO_MENU.Children.Insert(5, Variables.AUDIO_SUB_INTRO);
                                SUB_INTRO_ACTIVE = true;
                            }

                            else if (_audioMode != _fetchIndex && SUB_INTRO_ACTIVE)
                            {
                                Variables.INTRO_MENU.Children.Remove(Variables.AUDIO_SUB_INTRO);
                                SUB_INTRO_ACTIVE = false;
                            }
                        }

                        _addonOffset = _addonOffset + (SUB_INTRO_ACTIVE ? 0x02 : 0x01);
                    }

                    if (SUB_INTRO_ACTIVE)
                        _subAudio = _readState[0x05];

                    if (_musicActive)
                    {
                        _musicMode = _readState[0x04 + _addonOffset] == 0x00 ? Variables.CONFIG_BITWISE.MUSIC_VANILLA : Variables.CONFIG_BITWISE.OFF;
                        _addonOffset++;
                    }

                    if (_enemyActive)
                    {
                        _enemyMode = _readState[0x04 + _addonOffset] == 0x00 ? Variables.CONFIG_BITWISE.HEARTLESS_VANILLA: Variables.CONFIG_BITWISE.OFF;
                        _addonOffset++;
                    }

                    Demand.SKIP_ROXAS = _readState[0x04 + _addonOffset] == 0x01 ? true : false;
                }

                CONFIG_BIT = Variables.CONFIG_BITWISE.SUMMON_FULL | Variables.CONFIG_BITWISE.NAVI_MAP | _vibration | _autoSave | _controlPrompt | _audioMode | _musicMode | _enemyMode;
            }

            if (!Variables.IS_TITLE && !CONFIG_WRITTEN && CONFIG_TOGGLE)
            {
                var _areaRead = Hypervisor.Read<uint>(Variables.ADDR_Area);

                if (_areaRead == 0x0102 || (_areaRead == 0x2002))
                {
                    Terminal.Log("Intro Handler detected a new game! Writing the configuration.", 0);

                    Hypervisor.Write(Variables.ADDR_Config, CONFIG_BIT);

                    if (SUB_INTRO_ACTIVE)
                    Hypervisor.Write(Variables.ADDR_Config + 0x04, _subAudio);

                    Variables.AUDIO_MODE = CONFIG_BIT.HasFlag(Variables.CONFIG_BITWISE.AUDIO_PRIMARY) ? 0x01 :
                                          (CONFIG_BIT.HasFlag(Variables.CONFIG_BITWISE.AUDIO_SECONDARY) ? 0x02 : 0x00); 
                    
                    Variables.MUSIC_VANILLA = CONFIG_BIT.HasFlag(Variables.CONFIG_BITWISE.MUSIC_VANILLA);
                    Variables.ENEMY_VANILLA = CONFIG_BIT.HasFlag(Variables.CONFIG_BITWISE.HEARTLESS_VANILLA);
                    Variables.CONTROLLER_MODE = CONFIG_BIT.HasFlag(Variables.CONFIG_BITWISE.PROMPT_CONTROLLER);
                    Variables.SAVE_MODE = CONFIG_BIT.HasFlag(Variables.CONFIG_BITWISE.AUTOSAVE_INDICATOR) ? 0x00 : (CONFIG_BIT.HasFlag(Variables.CONFIG_BITWISE.AUTOSAVE_SILENT) ? 0x01 : 0x02);

                    CONFIG_WRITTEN = true;
                }
            }
        }

        public static void HandleConfig()
        {
            var _layoutPointer = Hypervisor.Read<ulong>(Variables.PINT_Camp2LD);

            var _configSecond = Hypervisor.Read<byte>(Variables.ADDR_Config + 0x02);
            var _configBitwise = Hypervisor.Read<Variables.CONFIG_BITWISE>(Variables.ADDR_Config);

            var _selectPoint = Hypervisor.Read<ulong>(Variables.PINT_SubMenuOptionSelect);

            var _pauseRead = Hypervisor.Read<byte>(Variables.ADDR_PauseFlag);
            var _menuRead = Hypervisor.Read<byte>(Variables.ADDR_SubMenuType);

            var _settingsPoint = Hypervisor.Read<ulong>(Variables.PINT_ConfigMenu);
            var _difficultyRead = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0x2498);

            var _audioActive = Variables.CONFIG_MENU.Children.FirstOrDefault(x => x.Title == 0x570B) == null ? false : true;
            var _musicActive = Variables.CONFIG_MENU.Children.FirstOrDefault(x => x.Title == 0x5718) == null ? false : true;
            var _enemyActive = Variables.CONFIG_MENU.Children.FirstOrDefault(x => x.Title == 0x571D) == null ? false : true;

            var _offsetAudio = Convert.ToInt32(_audioActive) + Convert.ToInt32(SUB_CONFIG_ACTIVE);
            var _offsetMusic = Convert.ToInt32(_audioActive) + Convert.ToInt32(_musicActive) + Convert.ToInt32(SUB_CONFIG_ACTIVE);
            var _offsetEnemy = Convert.ToInt32(_audioActive) + Convert.ToInt32(_musicActive) + Convert.ToInt32(_enemyActive) + Convert.ToInt32(SUB_CONFIG_ACTIVE);

            if (!Variables.IS_TITLE && !LOADED_SETTINGS)
            {
                Terminal.Log("Fetching the config from the save file.", 0);

                if (!Variables.IS_LITE)
                {
                    Variables.MUSIC_VANILLA = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.MUSIC_VANILLA);
                    Variables.ENEMY_VANILLA = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.HEARTLESS_VANILLA);
                    Variables.AUDIO_MODE = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.AUDIO_PRIMARY) ? 0x01 : (_configBitwise.HasFlag(Variables.CONFIG_BITWISE.AUDIO_SECONDARY) ? 0x02 : 0x00);
                }

                Variables.CONTROLLER_MODE = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.PROMPT_CONTROLLER);
                Variables.SAVE_MODE = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.AUTOSAVE_INDICATOR) ? 0x00 : (_configBitwise.HasFlag(Variables.CONFIG_BITWISE.AUTOSAVE_SILENT) ? 0x01 : 0x02);

                Terminal.Log("Fetch successful!", 0);

                LOADED_SETTINGS = true;
            }

            else if (Variables.IS_TITLE && LOADED_SETTINGS)
                LOADED_SETTINGS = false;

            if (_menuRead == 0x24 && _pauseRead == 0x00 && _selectPoint != 0x00)
            {
                if (_settingsPoint != 0x00 && !DEBOUNCE[6])
                {
                    Terminal.Log("Config Menu has been opened! Setting up necessary stuff.", 0);

                    var _vladBit = Hypervisor.Read<byte>(Variables.ADDR_Config + 0x03);

                    var _naviMap = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.NAVI_MAP) ? 0x00 : 0x01;
                    var _rightStick = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.RIGHT_STICK) ? 0x01 : 0x00;
                    var _cameraAuto = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.FIELD_CAM) ? 0x01 : 0x00;
                    var _cameraHRev = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.CAMERA_H) ? 0x01 : 0x00;
                    var _cameraVRev = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.CAMERA_V) ? 0x01 : 0x00;
                    var _commandKH2 = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.COMMAND_KH2) ? 0x01 : 0x00;
                    var _vibrationOn = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.VIBRATION) ? 0x00 : 0x01;
                    var _summonEffect = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.SUMMON_PARTIAL) ? 0x00 : (_configBitwise.HasFlag(Variables.CONFIG_BITWISE.SUMMON_FULL) ? 0x01 : 0x02);

                    var _autoSave = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.AUTOSAVE_INDICATOR) ? 0x00 : (_configBitwise.HasFlag(Variables.CONFIG_BITWISE.AUTOSAVE_SILENT) ? 0x01 : 0x02);
                    var _promptMode = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.PROMPT_CONTROLLER) ? 0x00 : 0x01;

                    if (_vladBit == 0x01)
                        _commandKH2 = 0x02;

                    SETTINGS_READ = new List<byte>
                    {
                        Convert.ToByte(_cameraAuto),
                        Convert.ToByte(_rightStick),
                        Convert.ToByte(_cameraVRev),
                        Convert.ToByte(_cameraHRev),
                        Convert.ToByte(_summonEffect),
                        Convert.ToByte(_naviMap),
                        Convert.ToByte(_autoSave),
                        Convert.ToByte(_promptMode),
                        Convert.ToByte(_vibrationOn),
                        Convert.ToByte(_commandKH2),
                        _difficultyRead
                    };

                    if (!Variables.IS_LITE)
                    {
                        var _audioToggle = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.AUDIO_PRIMARY) ? 0x01 :
                                          (_configBitwise.HasFlag(Variables.CONFIG_BITWISE.AUDIO_SECONDARY) ? 0x02 : 0x00);
                        
                        var _musicClassic = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.MUSIC_VANILLA) ? 0x00 : 0x01;
                        var _heartlessClassic = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.HEARTLESS_VANILLA) ? 0x00 : 0x01;
                        
                        if (_enemyActive)
                            SETTINGS_READ.Insert(0x09, Convert.ToByte(_heartlessClassic));

                        if (_musicActive)
                            SETTINGS_READ.Insert(0x09, Convert.ToByte(_musicClassic));

                        if (_audioActive)
                        {
                            SETTINGS_READ.Insert(0x09, Convert.ToByte(_audioToggle));

                            if (Variables.AUDIO_SUB_CONFIG != null)
                            {
                                var _toggleSeek = AUDIO_SUB_ONLY ? 0x01 : 0x02;
                                var _fetchConfig = Variables.CONFIG_MENU.Children.FirstOrDefault(x => x.Title == Variables.AUDIO_SUB_CONFIG.Title);

                                if (_audioToggle == _toggleSeek && _fetchConfig == null)
                                {
                                    Variables.CONFIG_MENU.Children.Insert(0x0A, Variables.AUDIO_SUB_CONFIG);
                                    SETTINGS_READ.Insert(0x0A, _configSecond);
                                    SUB_CONFIG_ACTIVE = true;
                                }

                                else if (_audioToggle != _toggleSeek && _fetchConfig != null)
                                {
                                    Variables.CONFIG_MENU.Children.Remove(Variables.AUDIO_SUB_CONFIG);
                                    SUB_CONFIG_ACTIVE = false;
                                }
                            }
                        }
                    }

                    Hypervisor.Write(_settingsPoint, SETTINGS_READ.ToArray(), true);

                    Variables.SharpHook[OffsetConfigUpdate].Execute();
                    Variables.SharpHook[OffsetSelectUpdate].Execute();

                    DEBOUNCE[6] = true;
                }

                // We have entered the menu.
                if (Variables.IS_LOADED)
                    ENTER_CONFIG = true;

                SETTINGS_WRITE = Hypervisor.Read<byte>(_settingsPoint, SETTINGS_READ.Count(), true);

                /*
                 *
                 * THIS ENTIRE BLOCK IS JUST FOR THE FUCKING SCROLL BAR!
                 * 
                 * This was quite the endavour to get working. Turns out that Square Enix
                 * decided to do something correctly here and killl the ENTIRE scroll bar
                 * rather than just hiding it. So I am doing my best here using **black magic**
                 * to bring it back to life.
                 * 
                 * It works quite well, though not 1:1 with the original. Could not be bothered THAT much.
                 *
                 */

                
                var _pageIndex = Hypervisor.Read<byte>(_selectPoint + 0x12, true);

                var _pageCount = (Variables.CONFIG_MENU.Children.Count - 0x09);
                var _pageFactor = 0x18 * _pageCount;

                var _pageOffset = (_pageFactor / _pageCount) * _pageIndex;

                Hypervisor.Write(_layoutPointer + 0x21498 - 0x1D0, 0x64 + _pageOffset, true);
                Hypervisor.Write(_layoutPointer + 0x2149C - 0x1D0, 0x64 + _pageOffset, true);

                Hypervisor.Write(_layoutPointer + 0x21528 - 0x1D0, 0x64 + _pageOffset, true);
                Hypervisor.Write(_layoutPointer + 0x2152C - 0x1D0, 0x64 + _pageOffset, true);

                Hypervisor.Write(_layoutPointer + 0x215B8 - 0x1D0, 0x64 - (_pageFactor + 1) + _pageOffset, true);
                Hypervisor.Write(_layoutPointer + 0x215BC - 0x1D0, 0x64 - (_pageFactor + 1) + _pageOffset, true);

                Hypervisor.Write(_layoutPointer + 0x21568 - 0x1D0, (0xC0 - _pageFactor) * 0.01F, true);
                Hypervisor.Write(_layoutPointer + 0x2156C - 0x1D0, (0xC0 - _pageFactor) * 0.01F, true);
                

                // ======================================================================================= //

                var _fieldCamBit = SETTINGS_WRITE[0x00] == 0x01 ? Variables.CONFIG_BITWISE.FIELD_CAM : Variables.CONFIG_BITWISE.OFF;
                var _rightStickBit = SETTINGS_WRITE[0x01] == 0x01 ? Variables.CONFIG_BITWISE.RIGHT_STICK : Variables.CONFIG_BITWISE.OFF;

                var _cameraVerticalBit = SETTINGS_WRITE[0x02] == 0x01 ? Variables.CONFIG_BITWISE.CAMERA_V : Variables.CONFIG_BITWISE.OFF;
                var _cameraHorizontalBit = SETTINGS_WRITE[0x03] == 0x01 ? Variables.CONFIG_BITWISE.CAMERA_H : Variables.CONFIG_BITWISE.OFF;

                var _summonBit = SETTINGS_WRITE[0x04] == 0x00 ? Variables.CONFIG_BITWISE.SUMMON_PARTIAL :
                                (SETTINGS_WRITE[0x04] == 0x01 ? Variables.CONFIG_BITWISE.SUMMON_FULL : Variables.CONFIG_BITWISE.OFF);

                var _mapBit = SETTINGS_WRITE[0x05] == 0x00 ? Variables.CONFIG_BITWISE.NAVI_MAP : Variables.CONFIG_BITWISE.OFF;

                var _autoSaveBit = SETTINGS_WRITE[0x06] == 0x00 ? Variables.CONFIG_BITWISE.AUTOSAVE_INDICATOR :
                                  (SETTINGS_WRITE[0x06] == 0x01 ? Variables.CONFIG_BITWISE.AUTOSAVE_SILENT : Variables.CONFIG_BITWISE.OFF);

                var _controllerBit = SETTINGS_WRITE[0x07] == 0x00 ? Variables.CONFIG_BITWISE.PROMPT_CONTROLLER : Variables.CONFIG_BITWISE.OFF;

                var _vibrationBit = SETTINGS_WRITE[0x08] == 0x00 ? Variables.CONFIG_BITWISE.VIBRATION : Variables.CONFIG_BITWISE.OFF;

                var _commandBit = SETTINGS_WRITE[0x09 + _offsetEnemy] == 0x01 ? Variables.CONFIG_BITWISE.COMMAND_KH2 : Variables.CONFIG_BITWISE.OFF;

                if (SETTINGS_WRITE[0x09 + _offsetEnemy] == 0x02)
                {
                    _commandBit = Variables.CONFIG_BITWISE.COMMAND_KH2;
                    Hypervisor.Write<byte>(Variables.ADDR_Config + 0x03, 0x01);
                }

                else
                    Hypervisor.Write<byte>(Variables.ADDR_Config + 0x03, 0x00);

                Variables.SAVE_MODE = SETTINGS_WRITE[0x06];
                Variables.CONTROLLER_MODE = SETTINGS_WRITE[0x07] == 0x00 ? true : false;

                var _audioBit = SETTINGS_WRITE[0x09] == 0x01 ? Variables.CONFIG_BITWISE.AUDIO_PRIMARY : 
                               (SETTINGS_WRITE[0x09] == 0x02 ? Variables.CONFIG_BITWISE.AUDIO_SECONDARY : Variables.CONFIG_BITWISE.OFF);

                var _musicBit = SETTINGS_WRITE[0x09 + _offsetAudio] == 0x00 ? Variables.CONFIG_BITWISE.MUSIC_VANILLA : Variables.CONFIG_BITWISE.OFF;
                var _enemyBit = SETTINGS_WRITE[0x09 + _offsetMusic] == 0x00 ? Variables.CONFIG_BITWISE.HEARTLESS_VANILLA : Variables.CONFIG_BITWISE.OFF;
               
                Variables.AUDIO_MODE = SETTINGS_WRITE[0x09];
                Variables.MUSIC_VANILLA = SETTINGS_WRITE[0x09 + _offsetAudio] == 0x00 ? true : false;
                Variables.ENEMY_VANILLA = SETTINGS_WRITE[0x09 + _offsetMusic] == 0x00 ? true : false;

                var _writeBitwise =
                    _fieldCamBit |
                    _rightStickBit |
                    _cameraVerticalBit |
                    _cameraHorizontalBit |
                    _summonBit |
                    _mapBit |
                    _autoSaveBit |
                    _controllerBit |
                    _vibrationBit |
                    _commandBit;

                 if (!Variables.IS_LITE)
                    _writeBitwise = _writeBitwise | _audioBit | _musicBit | _enemyBit;

                Hypervisor.Write(Variables.ADDR_Config, _writeBitwise);

                if (SUB_CONFIG_ACTIVE)
                    Hypervisor.Write(Variables.ADDR_Config + 0x02, SETTINGS_WRITE[0x0A]);
                    
                var _toggleBit = AUDIO_SUB_ONLY ? Variables.CONFIG_BITWISE.AUDIO_PRIMARY : Variables.CONFIG_BITWISE.AUDIO_SECONDARY;

                if ((Variables.AUDIO_SUB_CONFIG != null && _audioBit == _toggleBit && !SUB_CONFIG_ACTIVE) ||
                    (Variables.AUDIO_SUB_CONFIG != null && _audioBit != _toggleBit && SUB_CONFIG_ACTIVE))
                    DEBOUNCE[6] = false;
            }

            else if (_selectPoint == 0x00 && ENTER_CONFIG && SETTINGS_READ != null && SETTINGS_WRITE != null)
            {
                var _checkAudio = SETTINGS_WRITE[0x09] == SETTINGS_READ[0x09];
                var _checkSubAudio = SETTINGS_WRITE[0x0A] == SETTINGS_WRITE[0x0A];
                var _checkMusic = SETTINGS_WRITE[0x09 + _offsetAudio] == SETTINGS_READ[0x09 + _offsetAudio];
                var _checkEnemy = SETTINGS_WRITE[0x09 + _offsetMusic] == SETTINGS_READ[0x09 + _offsetMusic];


                if (!Variables.IS_LITE && ((!_checkAudio && _audioActive) || (!_checkMusic && _musicActive) || (!_checkEnemy && _enemyActive) || (SUB_CONFIG_ACTIVE && !_checkSubAudio)))
                {
                    LOCK_AUTOSAVE = true;

                    Terminal.Log("Config submitted! Applying changes through hot-reloading!", 0);

                    ENTER_CONFIG = false;
                    DEBOUNCE[6] = false;

                    // Give time for the Menu to close.
                    while (Hypervisor.Read<byte>(Variables.ADDR_MenuFlag) != 0x00) ;

                    Terminal.Log("Killing the background music.", 0);
                    Sound.KillBGM();

                    Terminal.Log("Copying down the current area state.", 0);

                    // Read the current world and event data.
                    if (AREA_READ == null)
                        AREA_READ = Hypervisor.Read<byte>(Variables.ADDR_Area, 0x0A);

                    // Make a new world data to be: Twilight Town - The Empty Realm.
                    var _newArray = new byte[] { 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

                    // If already in TT, change the target world to OC.
                    if (AREA_READ[0] == 0x02)
                        _newArray[0] = 0x01;

                    Terminal.Log("Jumping into a dummy room.", 0);

                    // Initiate the jump.
                    Hypervisor.Write(Variables.ADDR_Area, _newArray);
                    Variables.SharpHook[OffsetMapJump].Execute(BSharpConvention.MicrosoftX64, (long)(Hypervisor.PureAddress + Variables.ADDR_Area), 2, 0, 0, 0);

                    // Wait until the fade has been completed.
                    while (Hypervisor.Read<byte>(Variables.ADDR_FadeValue) != 0x80) ;

                    // Destroy the fade handler so it does not cause issues.
                    Hypervisor.DeleteInstruction((ulong)(OffsetSetFadeOff + 0x81A), 0x08);
                    Hypervisor.Write<byte>(Variables.ADDR_FadeValue, 0x80);

                    // Whilst not loaded, constantly shut off the music.
                    while (!Variables.IS_LOADED)
                        Sound.KillBGM();

                    Variables.SharpHook[OffsetSetFadeOff].Execute(0x02);

                    Terminal.Log("Jump complete! Jumping back!", 0);

                    // Atfer load, jump back to where we came from.
                    Hypervisor.Write(Variables.ADDR_Area, AREA_READ);
                    Variables.SharpHook[OffsetMapJump].Execute(BSharpConvention.MicrosoftX64, (long)(Hypervisor.PureAddress + Variables.ADDR_Area), 2, 0, 0, 0);

                    // Wait until load.
                    while (!Variables.IS_LOADED) ;

                    // Restore the fade initiater after load.
                    Hypervisor.Write<byte>((ulong)(OffsetSetFadeOff + 0x81A), [ 0xF3, 0x0F, 0x11, 0x8F, 0x0C, 0x01, 0x00, 0x00 ]);

                    Terminal.Log("All settings were applied through hot-reloading successfully!", 0);

                    // Flush the world data.
                    AREA_READ = null;
                    LOCK_AUTOSAVE = false;
                }

                else
                {
                    ENTER_CONFIG = false;
                    DEBOUNCE[6] = false;
                    AREA_READ = null;
                    LOCK_AUTOSAVE = false;
                }
            }
        }

        public static void HandleRatio()
        {
            if (!DELAY_ULTRAWIDE)
            {
                if (START_DELAY == DateTime.MinValue)
                    START_DELAY = DateTime.Now;

                var _currTime = DateTime.Now;
                var _currSeconds = (_currTime - START_DELAY).TotalSeconds;

                if (_currSeconds >= 5)
                    DELAY_ULTRAWIDE = true;
            }

            else
            {
                var _renderWidth = Hypervisor.Read<float>(Variables.ADDR_RenderResolution);
                var _renderHeight = Hypervisor.Read<float>(Variables.ADDR_RenderResolution + 0x04);

                if (_renderWidth != 0x00 && _renderHeight != 0x00)
                {
                    var _fetchRatio = (float)Math.Round(_renderWidth / _renderHeight, 2);

                    Hypervisor.Write(Variables.ADDR_Viewspace3D, _fetchRatio - 0.77F);

                    if (_fetchRatio == 1.6F)
                        Hypervisor.Write(Variables.ADDR_Viewspace3D + 0x04, 1.15F);

                    Hypervisor.Write(Variables.ADDR_Viewspace2D, (640F / (_fetchRatio - 0.77F)));
               

                    POSITIVE_OFFSET = 0x0055;
                    NEGATIVE_OFFSET = -0x0055;

                    switch (_fetchRatio)
                    {
                        case 2.37F:
                        case 2.38F:
                        case 2.39F:
                            POSITIVE_OFFSET = 0x00C3;
                            NEGATIVE_OFFSET = -0x00C3;
                            break;
                        case 3.55F:
                        case 3.56F:
                            POSITIVE_OFFSET = 0x01A5;
                            NEGATIVE_OFFSET = -0x01A5;
                            break;
                    }

                    if (Variables.PLATFORM == "STEAM")
                    {
                        Hypervisor.Write(0x15E55C, NEGATIVE_OFFSET);

                        Hypervisor.Write(0x17A586, POSITIVE_OFFSET);
                        Hypervisor.Write(0x17A5C0, POSITIVE_OFFSET);

                        Hypervisor.Write(0x17F2BB, POSITIVE_OFFSET);
                        Hypervisor.Write(0x17F313, POSITIVE_OFFSET);
                        Hypervisor.Write(0x17F35A, POSITIVE_OFFSET);
                        Hypervisor.Write(0x17F37F, POSITIVE_OFFSET);
                        Hypervisor.Write(0x17F399, POSITIVE_OFFSET);
                        Hypervisor.Write(0x17F3C9, POSITIVE_OFFSET);
                        Hypervisor.Write(0x17F3F9, POSITIVE_OFFSET);
                        Hypervisor.Write(0x17F429, POSITIVE_OFFSET);
                        Hypervisor.Write(0x17F5E5, POSITIVE_OFFSET);
                        Hypervisor.Write(0x17F619, POSITIVE_OFFSET);

                        Hypervisor.Write(0x180BBF, POSITIVE_OFFSET);
                        Hypervisor.Write(0x1819A9, POSITIVE_OFFSET);
                        Hypervisor.Write(0x18157D, POSITIVE_OFFSET);
                        Hypervisor.Write(0x1809F8, POSITIVE_OFFSET);

                        Hypervisor.Write(0x180E5A, POSITIVE_OFFSET);
                        Hypervisor.Write(0x180EFF, POSITIVE_OFFSET);

                        Hypervisor.Write(0x181D89, POSITIVE_OFFSET);

                        Hypervisor.Write(0x182E4C, POSITIVE_OFFSET);
                        Hypervisor.Write(0x182E86, POSITIVE_OFFSET);
                        Hypervisor.Write(0x182EB5, POSITIVE_OFFSET);
                        Hypervisor.Write(0x182EE4, POSITIVE_OFFSET);
                        Hypervisor.Write(0x182F13, POSITIVE_OFFSET);
                        Hypervisor.Write(0x182F42, POSITIVE_OFFSET);
                        Hypervisor.Write(0x183049, POSITIVE_OFFSET);

                        Hypervisor.Write(0x18BA66, POSITIVE_OFFSET);
                        Hypervisor.Write(0x18B757, NEGATIVE_OFFSET);

                        Hypervisor.Write(0x18C556, POSITIVE_OFFSET);
                        Hypervisor.Write(0x18C932, NEGATIVE_OFFSET);

                        Hypervisor.Write(0x18CEF6, NEGATIVE_OFFSET);
                        Hypervisor.Write(0x18D0D7, POSITIVE_OFFSET);

                        Hypervisor.Write(0x18DD06, NEGATIVE_OFFSET);
                        Hypervisor.Write(0x18DD3A, NEGATIVE_OFFSET);

                        Hypervisor.Write(0x18E58F, NEGATIVE_OFFSET);
                    }
                }
            }
        }

        public static void HandleRetry()
        {
            var _menuRead = Hypervisor.Read<int>(Variables.ADDR_MenuType);
            var _selectRead = Hypervisor.Read<byte>(Variables.ADDR_MenuSelect);

            var _soraHealth = Hypervisor.Read<byte>(Variables.ADDR_PlayerHP);
            var _gameOverRead = Hypervisor.Read<ulong>(Variables.PINT_GameOver);
            var _gameOverOptions = Hypervisor.Read<ulong>(Variables.PINT_GameOverOptions);

            var _subMenuRead = Hypervisor.Read<byte>(Variables.ADDR_SubMenuType);
            var _finishRead = Hypervisor.Read<byte>(Variables.ADDR_FinishFlag);

            var _pauseRead = Hypervisor.Read<byte>(Variables.ADDR_PauseFlag);

            var _worldRead = Hypervisor.Read<byte>(Variables.ADDR_Area);
            var _roomRead = Hypervisor.Read<byte>(Variables.ADDR_Area + 0x01);
            var _eventRead = Hypervisor.Read<ushort>(Variables.ADDR_Area + 0x04);

            var _entRetry = new Continue.Entry()
            {
                Opcode = 0x0002,
                Label = 0x8AB1,
            };
            var _entPrepare = new Continue.Entry()
            {
                Opcode = 0x0002,
                Label = 0x5727,
            };

            var _isEscape = _worldRead == 0x06 && _roomRead == 0x05 && _eventRead == 0x6F;

            var _blacklistCheck =
               (_worldRead == 0x08 && _roomRead == 0x03)
            || (_worldRead == 0x04 && _roomRead >= 0x15 && _roomRead <= 0x1A)
            || (_worldRead == 0x06 && _roomRead == 0x09 && _eventRead >= 0xBD && _eventRead <= 0xC4)
            || (_worldRead == 0x11 && _roomRead == 0x02 && _eventRead >= 0x3D && _eventRead <= 0x3F);

            if (Variables.IS_TITLE && RETRY_MODE != 0x00)
            {
                Terminal.Log("Title Screen detected whilst Retry is active! Falling back to defaults.", 0);

                Hypervisor.Write(WARP_OFFSET, WARP_FUNCTION);
                Hypervisor.Write(INVT_OFFSET, INVT_FUNCTION);

                RETRY_MODE = 0x00;
                HADES_COUNT = 0x00;
                RETRY_HOLD = false;
            }

            if (!_blacklistCheck && !Variables.IS_TITLE)
            {
                var _battleState = Variables.BATTLE_MODE == Variables.BATTLE_TYPE.BOSS && !Variables.IS_CUTSCENE;
                var _hadesComplete = !_isEscape || (_isEscape && (Variables.BATTLE_MODE == Variables.BATTLE_TYPE.FIELD || HADES_COUNT == 3));

                if (RETRY_BLOCK)
                {
                    Terminal.Log("Out of the blacklisted area. Retry functionality has been enabled.", 0);
                    RETRY_BLOCK = false;
                }

                if (_battleState && _pauseRead == 0x01 && !STATE_COPIED)
                {
                    Terminal.Log("A forced battle was started. Savestate was copied to memory.", 0);

                    if (_isEscape)
                    {
                        HADES_COUNT = 0;
                        Terminal.Log("Adjusted Retry Logic for the Hades Escape event.", 0);
                    }
                    
                    var _currentSave = Hypervisor.Read<byte>(Variables.ADDR_SaveData, 0x10FC0);

                    Hypervisor.Write(Variables.ADDR_ContData, _currentSave);
                    STATE_COPIED = true;

                    var _insertIndex = Variables.RETRY_DEFAULT ? 0 : 1;

                    Variables.CONTINUE_MENU.Children.Insert(_insertIndex, _entPrepare);
                    Variables.CONTINUE_MENU.Children.Insert(_insertIndex, _entRetry);
                }

                if (_isEscape && Variables.BATTLE_MODE == Variables.BATTLE_TYPE.FIELD && !DEBOUNCE[7])
                {
                    HADES_COUNT += 1;
                    DEBOUNCE[7] = true;
                    Terminal.Log("Incrementing the clear count for Hades Escape.", 0);
                }

                if (_isEscape && Variables.BATTLE_MODE == Variables.BATTLE_TYPE.BOSS && DEBOUNCE[7])
                    DEBOUNCE[7] = false;

                if (_gameOverRead == 0x00 && Variables.IS_EVENT && RETRY_MODE != 0x00)
                {
                    Terminal.Log("After-Retry detected! Restoring functions just-in-case.", 0);

                    Hypervisor.Write(WARP_OFFSET, WARP_FUNCTION);
                    Hypervisor.Write(INVT_OFFSET, INVT_FUNCTION);

                    RETRY_MODE = 0x00;
                    RETRY_HOLD = false;

                    Thread.Sleep(50);
                }

                else if (_gameOverRead == 0x00 &&
                                  !_battleState &&
                                   _pauseRead == 0x01 &&
                                   _hadesComplete &&
                                   STATE_COPIED &&
                                   _soraHealth != 0x00 &&
                                   RETRY_MODE == 0x00)
                {
                    Terminal.Log("The battle has ended. Restoring state.", 0);
                    Hypervisor.Write(Variables.ADDR_ContData, new byte[0x10FC0]);

                    if (_isEscape)
                    {
                        HADES_COUNT = 255;
                        Terminal.Log("The battle ended on a special edge-case.", 0);
                    }

                    Variables.CONTINUE_MENU = new Continue();

                    ROXAS_KEYBLADE = 0x0000;
                    STATE_COPIED = false;
                    RETRY_HOLD = false;
                }

                if (_gameOverRead != 0x00 && STATE_COPIED)
                {
                    var _retryButton = Variables.RETRY_DEFAULT ? 0x00 : 0x01;
                    var _prepareButton = Variables.RETRY_DEFAULT ? 0x01 : 0x02;
                    var _continueButton = Variables.RETRY_DEFAULT ? 0x02 : 0x00;

                    var _buttonCheck = _selectRead == _retryButton || _selectRead == _prepareButton;

                    if (!RETRY_HOLD)
                    Thread.Sleep(10); // REPLACE WITH SUSPEND!

                    if (_isEscape && HADES_COUNT > 0)
                    {
                        HADES_COUNT = 0;
                        Terminal.Log("Death during Hades detected! Reseting count...", 0);
                    }

                    if (!_buttonCheck && _selectRead < 0x04 && RETRY_MODE != 0x00)
                    {
                        Terminal.Log("Switched out of Retry, enabling functions.", 0);

                        Hypervisor.Write(WARP_OFFSET, WARP_FUNCTION);
                        Hypervisor.Write(INVT_OFFSET, INVT_FUNCTION);

                        RETRY_MODE = 0x00;
                        PREPARE_MODE = 0x00;
                    }

                    else if (_selectRead == _retryButton && RETRY_MODE == 0x00 && _menuRead != 0x05)
                    {
                        Terminal.Log("Switched into Retry, disabling functions.", 0);

                        Hypervisor.DeleteInstruction(WARP_OFFSET, 0x05);
                        Hypervisor.RedirectLEA(INVT_OFFSET, (uint)Variables.ADDR_ContData);

                        Demand.CURR_SHORTCUT = 0x80;

                        RETRY_MODE = 0x01;
                        PREPARE_MODE = 0x00;
                    }

                    if (_selectRead == _prepareButton && RETRY_MODE != 0x02 && _menuRead != 0x05)
                    {
                        var _currentSave = Hypervisor.Read<byte>(Variables.ADDR_ContData, 0x10FC0);

                        Hypervisor.Write(Variables.ADDR_SaveData, _currentSave);

                        RETRY_MODE = 0x02;
                        PREPARE_MODE = 0x01;

                        Demand.CURR_SHORTCUT = 0x80;

                        Terminal.Log("Prepare and Retry is ready to execute.", 0);
                    }

                    if (_subMenuRead == 0x0C && RETRY_MODE != 0x00)
                    {
                        Terminal.Log("Load Game detected! Enabling functions!", 0);

                        Hypervisor.Write(WARP_OFFSET, WARP_FUNCTION);
                        Hypervisor.Write(INVT_OFFSET, INVT_FUNCTION);

                        RETRY_MODE = 0x00;
                        PREPARE_MODE = 0x00;
                    }

                    else if (_selectRead == _retryButton && RETRY_MODE == 0x02)
                    {
                        RETRY_MODE = 0x01;
                        PREPARE_MODE = 0x00;
                    }
                   
                    if (!RETRY_HOLD)
                    {
                        Thread.Sleep(10); // REPLACE WITH RESUME!
                        RETRY_HOLD = true;
                    }
                }

                else
                {
                    ulong _initOffset = Variables.PLATFORM == "STEAM" ? 0x517U : 0x4D7U;

                    if (PREPARE_MODE == 0x01 && _menuRead != 0x08)
                    {
                        Terminal.Log("Prepare request detected! Opening the Camp Menu...", 0);

                        Hypervisor.DeleteInstruction(CAMP_OFFSET + 0x1A7, 0x07);
                        Hypervisor.DeleteInstruction(CAMP_INIT_OFFSET + _initOffset, 0x08);

                        var _campBitwise = Variables.CAMP_BITWISE.ITEMS | 
                                           Variables.CAMP_BITWISE.ABILITIES | 
                                           Variables.CAMP_BITWISE.CUSTOMIZE | 
                                           Variables.CAMP_BITWISE.PARTY;

                        Hypervisor.Write(Variables.ADDR_CampBitwise, _campBitwise);

                        Popups.PopupMenu(0, 0);
                    }

                    else if (_menuRead == 0x08 && PREPARE_MODE == 0x01)
                        PREPARE_MODE = 0x02;

                    else if (PREPARE_MODE == 0x02 && _menuRead != 0x08)
                    {
                        Terminal.Log("Prepare finished! Copying the save state...", 0);
                        var _currentSave = Hypervisor.Read<byte>(Variables.ADDR_SaveData, 0x10FC0);

                        Hypervisor.Write(CAMP_OFFSET + 0x1A7, CAMP_FUNCTION);
                        Hypervisor.Write(CAMP_INIT_OFFSET + _initOffset, CAMP_INIT_FUNCTION);

                        Hypervisor.Write(Variables.ADDR_ContData, _currentSave);
                        PREPARE_MODE = 0x03;
                    }
                }

            }

            else if (!RETRY_BLOCK && !Variables.IS_TITLE)
            {
                Terminal.Log("Blacklisted area have been detected. Retry functionality has been disabled.", 0);
                RETRY_BLOCK = true;
            }
        }

        public static void HandleFormShortcuts()
        {
            var _parityCheck = Hypervisor.Read<byte>(ICON_OFFSET + 0x1A);

            if (Variables.FORM_SHORTCUT && _parityCheck != 0xEB)
            {
                var _copyInst = Hypervisor.Read<byte>(ICON_OFFSET + 0x1D, 0x19);

                Hypervisor.Write<byte>(ICON_OFFSET + 0x1A, [0xEB, 0x19]);

                Hypervisor.Write(ICON_OFFSET + 0x1C, _copyInst);

                Hypervisor.Write<byte>(ICON_OFFSET + 0x35, [0x3C, 0x0B, 0x75, 0x02, 0xB0]);
                Hypervisor.Write<byte>(ICON_OFFSET + 0x3B, [0x88, 0x47, 0x01, 0xEB, 0xDC]);

                Hypervisor.Write<byte>(ICON_OFFSET + 0x3A, 0xCE);

                Hypervisor.Write<byte>(LIST_OFFSET + 0x18E, [0xEB, 0xAA]);
                Hypervisor.Write<byte>(LIST_OFFSET + 0x138, [0xEB, 0x4E, 0x90, 0x90]);
                Hypervisor.Write<byte>(LIST_OFFSET + 0x188, [0x81, 0xCB, 0x00, 0x00, 0x24, 0x00]);

                Hypervisor.Write<byte>(EQUIP_OFFSET + 0x33, [0x80, 0xF9, 0x15, 0x74, 0xF2]);
                Hypervisor.Write<byte>(EQUIP_OFFSET + 0x16, [0xEB, 0x1B, 0x90, 0x90, 0x90, 0x90, 0x90]);
                Hypervisor.Write<byte>(EQUIP_OFFSET + 0x38, [0x31, 0xC0, 0x48, 0x83, 0xC4, 0x28, 0xC3]);

                Hypervisor.Write<byte>(FORM_OFFSET + 0x12C, [0xEB, 0x45, 0x90, 0x90]);
                Hypervisor.Write<byte>(FORM_OFFSET + 0x173, [0x81, 0xC3, 0x00, 0x00, 0x20, 0x00, 0xEB, 0xB5]);

                Hypervisor.DeleteInstruction(CATEGORY_OFFSET + 0x4AF, 0x02);
            }
        }

        public static void HandleCrown()
        {
            var _fileFormatter = Hypervisor.ReadString(Variables.DATA_PAXPath + 0x10);
            var _formRead = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0x3524);

            if (Variables.IS_TITLE || !Variables.IS_LOADED || PAST_FORM != _formRead)
            {
                LOAD_LIST = null;
                PAST_FORM = _formRead;
            }

        RELOAD_POINT:

            if (!Variables.IS_TITLE && Variables.IS_LOADED && !Variables.IS_CUTSCENE)
            {
                LOAD_LIST =
                [
                    Operations.FetchBufferFile(_fileFormatter.Replace("%s", "P_EX100")),
                    Operations.FetchBufferFile(_fileFormatter.Replace("%s", "P_EX100_BTLF")),
                    Operations.FetchBufferFile(_fileFormatter.Replace("%s", "P_EX100_MAGF")),
                    Operations.FetchBufferFile(_fileFormatter.Replace("%s", "P_EX100_KH1F")),
                    Operations.FetchBufferFile(_fileFormatter.Replace("%s", "P_EX100_TRIF")),
                    Operations.FetchBufferFile(_fileFormatter.Replace("%s", "P_EX100_ULTF")),
                    Operations.FetchBufferFile(_fileFormatter.Replace("%s", "P_EX100_HTLF"))
                ];

                var _soraPoints = new ulong[]
                {
                    LOAD_LIST[0] != Hypervisor.MemoryOffset ? Hypervisor.Read<ulong>(LOAD_LIST[0] + 0x58, true) : 0x00,
                    LOAD_LIST[1] != Hypervisor.MemoryOffset ? Hypervisor.Read<ulong>(LOAD_LIST[1] + 0x58, true) : 0x00,
                    LOAD_LIST[2] != Hypervisor.MemoryOffset ? Hypervisor.Read<ulong>(LOAD_LIST[2] + 0x58, true) : 0x00,
                    LOAD_LIST[3] != Hypervisor.MemoryOffset ? Hypervisor.Read<ulong>(LOAD_LIST[3] + 0x58, true) : 0x00,
                    LOAD_LIST[4] != Hypervisor.MemoryOffset ? Hypervisor.Read<ulong>(LOAD_LIST[4] + 0x58, true) : 0x00,
                    LOAD_LIST[5] != Hypervisor.MemoryOffset ? Hypervisor.Read<ulong>(LOAD_LIST[5] + 0x58, true) : 0x00,
                    LOAD_LIST[6] != Hypervisor.MemoryOffset ? Hypervisor.Read<ulong>(LOAD_LIST[6] + 0x58, true) : 0x00,
                };

                var _crownRead = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0x36B2, 0x03);
                var _crownSum = _crownRead[0] + _crownRead[1] + _crownRead[2];

                if (LOAD_LIST[_formRead] == 0xFFFFFFFFFFFFFFFF || _soraPoints[_formRead] > 0x7FFF00000000)
                {
                    LOAD_LIST = null;
                    goto RELOAD_POINT;
                }

                foreach (var _point in _soraPoints)
                {
                    if (_point != 0x00)
                    {
                        var _barOffset = Hypervisor.Read<uint>(_point + 0x08, true);
                        var _soraOffset = Hypervisor.Read<uint>(_point + 0x38, true) - _barOffset;

                        var _faceCheck = Hypervisor.Read<uint>(_point + 0x24, true);

                        if (_faceCheck != 0x65636166)
                            return;

                        var _topValue = 0x00 + _crownSum * 0x5A;
                        var _bottomValue = 0x5D + _crownSum * 0x5A;

                        for (uint i = 0; i < 3; i++)
                        {
                            Hypervisor.Write(_point + _soraOffset + 0x38 + (0x2C * i), _topValue, true);
                            Hypervisor.Write(_point + _soraOffset + 0x40 + (0x2C * i), _bottomValue, true);
                        }
                    }
                }
            }
        }
    
        public static void HandleRetribution()
        {
            if (Variables.RETRIBUTION || Variables.ABSOLUTION)
            {
                if (!Variables.IS_TITLE)
                {
                    var _readMenu = Hypervisor.Read<byte>(Variables.ADDR_MenuFlag);
                    var _readSubMenu = Hypervisor.Read<byte>(Variables.ADDR_SubMenuType);

                    var _readPicture = Hypervisor.Read<ushort>(Variables.ADDR_LoadedPicture);
                    var _seekItem = _readPicture == 420 ? 0x300 : 0x301;

                    var _isMenuGood = _readMenu == 0x01 && (_readSubMenu == 0x02 || _readSubMenu == 0x05);
                    var _isStatusGood = _readPicture == 420 || _readPicture == 421;

                    if (PARAMS_ALL.Count == 0x00)
                    {
                        Terminal.Log("Fetching Keyblade Information to use with Addon Keyblades...", 0);

                        for (int i = 0; i < Variables.KEY_DICTIONARY.Count; i++)
                        {
                            var _keyData = Variables.KEY_DICTIONARY.ElementAt(i);

                            var _getParam = Operations.FetchItemParams(_keyData.Value);
                            var _paramID = Hypervisor.Read<ushort>(_getParam, true);

                            PARAMS_ALL.Add(_paramID);
                        }
                    }

                    if (PARAM_ABSOLUTION == 0xFFFF)
                    {
                        ABSOLUTION_RAM = Operations.FetchItem(0x0301);
                        RETRIBUTION_RAM = Operations.FetchItem(0x0300);

                        PARAM_ABSOLUTION = Hypervisor.Read<ushort>(Variables.ADDR_SaveData + 0xE204);
                        PARAM_RETRIBUTION = Hypervisor.Read<ushort>(Variables.ADDR_SaveData + 0xE200);

                        if (PARAM_ABSOLUTION == 0x0000)
                        {
                            Hypervisor.Read<ushort>(Variables.ADDR_SaveData + 0xE204, 0x50);
                            PARAM_ABSOLUTION = 0x50;
                        }

                        if (PARAM_RETRIBUTION == 0x0000)
                        {
                            Hypervisor.Read<ushort>(Variables.ADDR_SaveData + 0xE200, 0x50);
                            PARAM_RETRIBUTION = 0x50;
                        }

                        Hypervisor.Write(RETRIBUTION_RAM + 0x06, PARAM_RETRIBUTION, true);
                        Hypervisor.Write(ABSOLUTION_RAM + 0x06, PARAM_ABSOLUTION, true);
                    }

                    var _equipArray = new ushort[]
                    {
                        Hypervisor.Read<ushort>(Variables.ADDR_SaveData + 0x24F0),
                        Hypervisor.Read<ushort>(Variables.ADDR_SaveData + 0x32F4),
                        Hypervisor.Read<ushort>(Variables.ADDR_SaveData + 0x33D4),
                        Hypervisor.Read<ushort>(Variables.ADDR_SaveData + 0x339C)
                    };

                    var _absolutionCheck = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0x3580 + 0x139) == 0x00 && _equipArray.FirstOrDefault(x => x == 0x0301) == 0x00;
                    var _retributionCheck = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0x3580 + 0x138) == 0x00 && _equipArray.FirstOrDefault(x => x == 0x0300) == 0x00;

                    if (_absolutionCheck && Variables.ABSOLUTION)
                    {
                        Terminal.Log("Giving Absolution...", 0);
                        Hypervisor.Write<byte>(Variables.ADDR_SaveData + 0x3580 + 0x139, 0x01);
                    }

                    if (_retributionCheck && Variables.RETRIBUTION)
                    {
                        Terminal.Log("Giving Retribution...", 0);
                        Hypervisor.Write<byte>(Variables.ADDR_SaveData + 0x3580 + 0x138, 0x01);
                    }

                    if (INDEX_ABSOLUTION == 0xFF)
                        INDEX_ABSOLUTION = PARAMS_ALL.IndexOf(PARAM_ABSOLUTION);

                    if (INDEX_RETRIBUTION == 0xFF)
                        INDEX_RETRIBUTION = PARAMS_ALL.IndexOf(PARAM_RETRIBUTION);

                    if (_readMenu == 0x01 && _readSubMenu != 0xFF && PARAMS_LIST.Count == 0)
                    {
                        Terminal.Log("Creating the Key Inventory Array...", 0);

                        for (int i = 0; i < Variables.KEY_DICTIONARY.Count; i++)
                        {
                            var _keyData = Variables.KEY_DICTIONARY.ElementAt(i);
                            var _slotCheck = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0x3580 + _keyData.Key);
                            var _tryFetch = _equipArray.FirstOrDefault(x => x == _keyData.Value);

                            if (_slotCheck == 0x01)
                                PARAMS_LIST.Add(PARAMS_ALL[i]);

                            else if (_tryFetch != 0x00)
                                PARAMS_LIST.Add(PARAMS_ALL[i]);
                        }
                    }

                    else if (_readMenu == 0x00 && PARAMS_LIST.Count > 0)
                    {
                        PARAMS_LIST.Clear();
                        DEBOUNCE[2] = false;
                    }

                    if (_isMenuGood)
                    {
                        if (_isStatusGood && Variables.IS_PRESSED(Variables.BUTTON.TRIANGLE) && !DEBOUNCE[2])
                        {
                            var _currParam = _seekItem == 0x0300 ? INDEX_RETRIBUTION : INDEX_ABSOLUTION;
                            Terminal.Log("Cycling the mimic of " + (_seekItem == 0x0300 ? "Retribution." : "Absolution."), 0);

                            _currParam++;

                            if (_currParam >= PARAMS_LIST.Count)
                                _currParam = 0x00;

                            Hypervisor.Write((_seekItem == 0x0300 ? RETRIBUTION_RAM : ABSOLUTION_RAM) + 0x06, PARAMS_LIST[_currParam], true);

                            if (_seekItem == 0x0300)
                            {
                                Hypervisor.Write(Variables.ADDR_SaveData + 0xE200, PARAMS_LIST[_currParam]);
                                PARAM_RETRIBUTION = PARAMS_LIST[_currParam];
                                INDEX_RETRIBUTION = _currParam;
                            }

                            else
                            {
                                Hypervisor.Write(Variables.ADDR_SaveData + 0xE204, PARAMS_LIST[_currParam]);
                                PARAM_ABSOLUTION = PARAMS_LIST[_currParam];
                                INDEX_ABSOLUTION = _currParam;
                            }

                            Sound.PlaySFX(0x02);
                            Variables.SharpHook[OffsetItemUpdate].Execute();

                            DEBOUNCE[2] = true;
                        }

                        else if ((!_isStatusGood || !Variables.IS_PRESSED(Variables.BUTTON.TRIANGLE)) && DEBOUNCE[2])
                            DEBOUNCE[2] = false;
                    }
                }
                
                else
                {
                    PARAM_ABSOLUTION = 0xFFFF;
                    PARAM_RETRIBUTION = 0xFFFF;
                }
            }
        }
    }
}
