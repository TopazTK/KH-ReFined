using ReFined.Common;
using ReFined.Libraries;
using ReFined.KH2.Information;
using ReFined.KH2.Menus;
using BSharpConvention = Binarysharp.MSharp.Assembly.CallingConvention.CallingConventions;
using ReFined.KH2.InGame;

namespace ReFined.KH2.Functions
{
    public static class Critical
    {
        public static IntPtr OffsetCampMenu;
        public static IntPtr OffsetShutMusic;
        public static IntPtr OffsetMapJump;
        public static IntPtr OffsetSetFadeOff;
        public static IntPtr OffsetConfigUpdate;
        public static IntPtr OffsetSelectUpdate;

        public static ulong WARP_OFFSET;
        public static ulong INVT_OFFSET;
        public static ulong CMD_OFFSET;

        public static ulong ICON_OFFSET;
        public static ulong LIST_OFFSET;
        public static ulong EQUIP_OFFSET;
        public static ulong CATEGORY_OFFSET;
        public static ulong FORM_OFFSET;

        public static byte[]? WARP_FUNCTION;
        public static byte[]? INVT_FUNCTION;

        static Variables.CONFIG_BITWISE CONFIG_BIT;
        static bool CONFIG_TOGGLE;
        static bool CONFIG_WRITTEN;

        static bool[] DEBOUNCE = new bool[0x20];

        static ushort SAVE_ROOM;
        static ushort SAVE_WORLD;
        static byte SAVE_ITERATOR;

        static bool LOADED_SETTINGS;

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

        static ulong[] LOAD_LIST = new ulong[7];
        static byte PAST_FORM;

        public static void HandleMagicSort()
        {
            var _loadRead = Hypervisor.Read<byte>(Variables.ADDR_LoadFlag);

            var _inputRead = Hypervisor.Read<ushort>(Variables.ADDR_Input);
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

            if (_loadRead == 0x01 && MAGIC_STORE == null && _firstMagic != 0x00)
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

            if (_loadRead == 0x01 && ROOM_LOADED && _menuPointer != 0x00)
            {
                if (MAGIC_STORE != null)
                {
                    Terminal.Log("Roomchange detected! Restoring the Magic Menu.", 1);
                    Hypervisor.Write(Variables.ADDR_MagicCommands, MAGIC_STORE);
                }

                ROOM_LOADED = false;
            }

            else if (_loadRead == 0x00 && !ROOM_LOADED)
                ROOM_LOADED = true;

            if (_menuPointer != 0x00 && _loadRead == 0x01)
            {
                var _menuRead = Hypervisor.Read<byte>(_menuPointer, true);

                if (_menuRead == 0x01)
                {
                    var _magicIndex = Hypervisor.Read<byte>(Variables.ADDR_MagicIndex);
                    var _magicMax = Hypervisor.Read<byte>(_menuPointer + 0x10, true);

                    var _inputCheck = (_inputRead & 0x0110) == 0x0110 ? 0x01 : (_inputRead & 0x0140) == 0x0140 ? 0x02 : 0x00;
                    var _triggerCheck = (_inputRead & 0x0100) == 0x0100;

                    var _insCheck = Hypervisor.Read<byte>(CMD_OFFSET + 0x1B1);

                    if (_triggerCheck && _insCheck != 0x90)
                    {
                        Terminal.Log("L2 Detected within Magic Menu! Disabling input registry.", 1);

                        foreach (var _off in MAGIC_OFFSET)
                        Hypervisor.DeleteInstruction(CMD_OFFSET + _off, 0x03);
                    }

                    else if (!_triggerCheck && _insCheck == 0x90)
                    {
                        Terminal.Log("L2 has been let go! Enabling input registry.", 1);

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
            var _loadRead = Hypervisor.Read<byte>(Variables.ADDR_LoadFlag);
            var _pauseCheck = Hypervisor.Read<byte>(Variables.ADDR_PauseFlag);
            var _roomCheck = Hypervisor.Read<byte>(Variables.ADDR_Area + 0x01);

            var _blacklistCheck =
                (_worldCheck == 0x08 && _roomCheck == 0x03) ||
                (_worldCheck == 0x0C && _roomCheck == 0x02) ||
                (_worldCheck == 0x12 && _roomCheck >= 0x13 && _roomCheck <= 0x1D) ||
                (_worldCheck == 0x02 && _roomCheck <= 0x01);

            if (!Variables.IS_TITLE && _loadRead == 0x01 && !_blacklistCheck)
            {
                var _battleRead = Hypervisor.Read<byte>(Variables.ADDR_BattleFlag);
                var _cutsceneRead = Hypervisor.Read<byte>(Variables.ADDR_CutsceneFlag);

                _loadRead = Hypervisor.Read<byte>(Variables.ADDR_LoadFlag);


                var _saveableBool = (Variables.SAVE_MODE != 0x02) &&
                                    _battleRead == 0x00 &&
                                    _loadRead == 0x01 &&
                                    _cutsceneRead == 0x00 &&
                                    _worldCheck >= 0x02 &&
                                    _pauseCheck == 0x00 &&
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
            var _selectButton = Hypervisor.Read<byte>(0xB1D5E4);

            CONFIG_TOGGLE = _selectButton == 0x00 ? true : false;

            if (Variables.IS_TITLE)
            {
                if (CONFIG_WRITTEN)
                {
                    Terminal.Log("Back on Title Screen! Flushing Intro configuration...", 0x00);
                    CONFIG_BIT = 0x00;
                    CONFIG_WRITTEN = false;
                }

                var _musicMode = Variables.CONFIG_BITWISE.OFF;
                var _enemyMode = Variables.CONFIG_BITWISE.OFF;

                var _readState = Hypervisor.Read<int>(0x820500, 0x100);

                var _vibration = _readState[0x01] == 0x00 ? Variables.CONFIG_BITWISE.VIBRATION : Variables.CONFIG_BITWISE.OFF;

                var _autoSave = _readState[0x02] == 0x00 ? Variables.CONFIG_BITWISE.AUTOSAVE_INDICATOR :
                               (_readState[0x02] == 0x01 ? Variables.CONFIG_BITWISE.AUTOSAVE_SILENT : Variables.CONFIG_BITWISE.OFF);

                var _controlPrompt = _readState[0x03] == 0x00 ? Variables.CONFIG_BITWISE.PROMPT_CONTROLLER : Variables.CONFIG_BITWISE.OFF;

                if (!Variables.IS_LITE)
                {
                    var _addonOffset = 0;

                    var _musicActive = Variables.INTRO_MENU.Children.FirstOrDefault(x => x.Flair == 0x0135) == null ? false : true;
                    var _enemyActive = Variables.INTRO_MENU.Children.FirstOrDefault(x => x.Flair == 0x0136) == null ? false : true;

                    if (_musicActive)
                    {
                        _musicMode = _readState[0x04] == 0x00 ? Variables.CONFIG_BITWISE.MUSIC_VANILLA : Variables.CONFIG_BITWISE.OFF;
                        _addonOffset++;
                    }

                    if (_enemyActive)
                    {
                        _enemyMode = _readState[0x05] == 0x00 ? Variables.CONFIG_BITWISE.HEARTLESS_VANILLA: Variables.CONFIG_BITWISE.OFF;
                        _addonOffset++;
                    }

                    Demand.SKIP_ROXAS = _readState[0x04 + _addonOffset] == 0x01 ? true : false;
                }

                CONFIG_BIT = Variables.CONFIG_BITWISE.SUMMON_FULL | Variables.CONFIG_BITWISE.NAVI_MAP | _vibration | _autoSave | _controlPrompt | _musicMode | _enemyMode;
            }

            if (!Variables.IS_TITLE && !CONFIG_WRITTEN && CONFIG_TOGGLE)
            {
                var _areaRead = Hypervisor.Read<uint>(Variables.ADDR_Area);

                if (_areaRead == 0x0102 || (_areaRead == 0x2002))
                {
                    Terminal.Log("Intro Handler detected a new game! Writing the configuration.", 0);

                    Hypervisor.Write(Variables.ADDR_Config, CONFIG_BIT);

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
            var _configBitwise = Hypervisor.Read<Variables.CONFIG_BITWISE>(Variables.ADDR_Config);

            var _selectPoint = Hypervisor.Read<ulong>(Variables.PINT_SubMenuOptionSelect);

            var _loadRead = Hypervisor.Read<byte>(Variables.ADDR_LoadFlag);
            var _pauseRead = Hypervisor.Read<byte>(Variables.ADDR_PauseFlag);
            var _menuRead = Hypervisor.Read<byte>(Variables.ADDR_SubMenuType);

            var _settingsPoint = Hypervisor.Read<ulong>(Variables.PINT_ConfigMenu);
            var _difficultyRead = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0x2498);

            var _musicActive = Variables.CONFIG_MENU.Children.FirstOrDefault(x => x.Title == 0x0118) == null ? false : true;
            var _enemyActive = Variables.CONFIG_MENU.Children.FirstOrDefault(x => x.Title == 0x011D) == null ? false : true;

            if (!Variables.IS_TITLE && !LOADED_SETTINGS)
            {
                Terminal.Log("Fetching the config from the save file.", 0);

                if (!Variables.IS_LITE)
                {
                    Variables.MUSIC_VANILLA = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.MUSIC_VANILLA);
                    Variables.ENEMY_VANILLA = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.HEARTLESS_VANILLA);
                }

                Variables.CONTROLLER_MODE = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.PROMPT_CONTROLLER);
                Variables.SAVE_MODE = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.AUTOSAVE_INDICATOR) ? 0x00 : (_configBitwise.HasFlag(Variables.CONFIG_BITWISE.AUTOSAVE_SILENT) ? 0x01 : 0x02);

                Terminal.Log("Fetch successful!", 0);

                LOADED_SETTINGS = true;
            }

            else if (Variables.IS_TITLE && LOADED_SETTINGS)
                LOADED_SETTINGS = false;

            if (_menuRead == 0x24 && _pauseRead == 0x01 && _selectPoint != 0x00)
            {

                if (_settingsPoint != 0x00 && !DEBOUNCE[6])
                {
                    Terminal.Log("Config Menu has been opened! Setting up necessary stuff.", 0);

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
                        if (_enemyActive)
                        {
                            var _heartlessClassic = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.HEARTLESS_VANILLA) ? 0x00 : 0x01;
                            SETTINGS_READ.Insert(0x09, Convert.ToByte(_heartlessClassic));
                        }

                        if (_musicActive)
                        {
                            var _musicClassic = _configBitwise.HasFlag(Variables.CONFIG_BITWISE.MUSIC_VANILLA) ? 0x00 : 0x01;
                            SETTINGS_READ.Insert(0x09, Convert.ToByte(_musicClassic));
                        }
                    }

                    Hypervisor.Write(_settingsPoint, SETTINGS_READ.ToArray(), true);

                    Variables.SharpHook[OffsetConfigUpdate].Execute();
                    Variables.SharpHook[OffsetSelectUpdate].Execute();

                    DEBOUNCE[6] = true;
                }

                // We have entered the menu.
                if (_loadRead == 0x01)
                    ENTER_CONFIG = true;

                SETTINGS_WRITE = Hypervisor.Read<byte>(_settingsPoint, SETTINGS_READ.Count(), true);

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

                var _musicBit = SETTINGS_WRITE[0x09] == 0x00 ? Variables.CONFIG_BITWISE.MUSIC_VANILLA : Variables.CONFIG_BITWISE.OFF;
                var _enemyBit = SETTINGS_WRITE[0x0A] == 0x00 ? Variables.CONFIG_BITWISE.HEARTLESS_VANILLA : Variables.CONFIG_BITWISE.OFF;

                var _commandBit = SETTINGS_WRITE[0x09 + Convert.ToInt32(_musicActive) + Convert.ToInt32(_enemyActive)] == 0x01 ? Variables.CONFIG_BITWISE.COMMAND_KH2 : Variables.CONFIG_BITWISE.OFF;

                Variables.SAVE_MODE = SETTINGS_WRITE[0x06];
                Variables.CONTROLLER_MODE = SETTINGS_WRITE[0x07] == 0x00 ? true : false;

                Variables.MUSIC_VANILLA = SETTINGS_WRITE[0x09] == 0x00 ? true : false;
                Variables.ENEMY_VANILLA = SETTINGS_WRITE[0x0A] == 0x00 ? true : false;

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
                    _writeBitwise = _writeBitwise | _musicBit | _enemyBit;

                Hypervisor.Write(Variables.ADDR_Config, _writeBitwise);
            }

            else if (_selectPoint == 0x00 && ENTER_CONFIG && SETTINGS_READ != null && SETTINGS_WRITE != null)
            {
                var _checkMusic = SETTINGS_WRITE[0x09] == SETTINGS_READ[0x09];
                var _checkEnemy = SETTINGS_WRITE[0x0A] == SETTINGS_READ[0x0A];

                if (!Variables.IS_LITE && ((!_checkMusic && _musicActive) || (!_checkEnemy && _checkEnemy)))
                {
                    LOCK_AUTOSAVE = true;

                    Terminal.Log("Config submitted! Applying changes through hot-reloading!", 0);

                    ENTER_CONFIG = false;
                    DEBOUNCE[6] = false;

                    // Give time for the Menu to close.
                    while (Hypervisor.Read<byte>(0x717418) != 1) ;

                    Terminal.Log("Killing the background music.", 0);
                    Variables.SharpHook[OffsetShutMusic].Execute();

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
                    while (Hypervisor.Read<byte>(0xABB3C7) != 0x80) ;

                    // Destroy the fade handler so it does not cause issues.
                    Hypervisor.DeleteInstruction((ulong)(OffsetSetFadeOff + 0x81A), 0x08);
                    Hypervisor.Write<byte>(0xABB3C7, 0x80);

                    // Whilst not loaded, constantly shut off the music.
                    while (Hypervisor.Read<byte>(Variables.ADDR_LoadFlag) != 1)
                        Variables.SharpHook[OffsetShutMusic].Execute();

                    Variables.SharpHook[OffsetSetFadeOff].Execute(0x02);

                    Terminal.Log("Jump complete! Jumping back!", 0);

                    // Atfer load, jump back to where we came from.
                    Hypervisor.Write(Variables.ADDR_Area, AREA_READ);
                    Variables.SharpHook[OffsetMapJump].Execute(BSharpConvention.MicrosoftX64, (long)(Hypervisor.PureAddress + Variables.ADDR_Area), 2, 0, 0, 0);

                    // Wait until load.
                    while (Hypervisor.Read<byte>(Variables.ADDR_LoadFlag) != 1) ;

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
            ulong _2ldAddr = 0x8A0974;
            ulong _renderAddr = 0x8A098C;
            ulong _viewportAddr = 0x8A099C;

            var _renderWidth = Hypervisor.Read<float>(_renderAddr);
            var _renderHeight = Hypervisor.Read<float>(_renderAddr + 0x04);

            if (_renderWidth != 0x00 && _renderHeight != 0x00)
            {
                var _fetchRatio = (float)Math.Round(_renderWidth / _renderHeight, 2);

                Hypervisor.Write(_viewportAddr, _fetchRatio);

                if (_fetchRatio == 1.6F)
                    Hypervisor.Write(_viewportAddr + 4, 2F);

                Hypervisor.Write(_2ldAddr, 640F / _fetchRatio);

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

                Hypervisor.Write(0x18B906 + 0x2400, NEGATIVE_OFFSET); // Information Bar
                Hypervisor.Write(0x18B93A + 0x2400, NEGATIVE_OFFSET); // Information Bar

                Hypervisor.Write(0x15C15C + 0x2400, NEGATIVE_OFFSET); // Command Menu

                Hypervisor.Write(0x17F989 + 0x2400, POSITIVE_OFFSET); // Party HUD

                Hypervisor.Write(0x178186 + 0x2400, POSITIVE_OFFSET); // Radar Cursor
                Hypervisor.Write(0x1781C0 + 0x2400, POSITIVE_OFFSET); // Radar Map

                Hypervisor.Write(0x180A4C + 0x2400, POSITIVE_OFFSET); // Enemy HP [Header]
                Hypervisor.Write(0x180A86 + 0x2400, POSITIVE_OFFSET); // Enemy HP ???
                Hypervisor.Write(0x180AB5 + 0x2400, POSITIVE_OFFSET); // Enemy HP [Backdrop]
                Hypervisor.Write(0x180AE4 + 0x2400, POSITIVE_OFFSET); // Enemy HP [Tail]
                Hypervisor.Write(0x180B13 + 0x2400, POSITIVE_OFFSET); // Enemy HP [Damage]
                Hypervisor.Write(0x180B42 + 0x2400, POSITIVE_OFFSET); // Enemy HP [Main Bar]
                Hypervisor.Write(0x180C49 + 0x2400, POSITIVE_OFFSET); // Enemy HP [Extra Bars]

                Hypervisor.Write(0x17CEBB + 0x2400, POSITIVE_OFFSET); // Sora Backdrop
                Hypervisor.Write(0x17CF13 + 0x2400, POSITIVE_OFFSET); // Sora HP [Backdrop]
                Hypervisor.Write(0x17CF5A + 0x2400, POSITIVE_OFFSET); // Sora HP [Complete]
                Hypervisor.Write(0x17CF7F + 0x2400, POSITIVE_OFFSET); // Sora Face
                Hypervisor.Write(0x17CF99 + 0x2400, POSITIVE_OFFSET); // Sora MP [Header]
                Hypervisor.Write(0x17CFC9 + 0x2400, POSITIVE_OFFSET); // Sora MP [Backdrop]
                Hypervisor.Write(0x17CFF9 + 0x2400, POSITIVE_OFFSET); // Sora MP [Bar]
                Hypervisor.Write(0x17D029 + 0x2400, POSITIVE_OFFSET); // Sora ???
                Hypervisor.Write(0x17D1E5 + 0x2400, POSITIVE_OFFSET); // Sora Drive
                Hypervisor.Write(0x17D219 + 0x2400, POSITIVE_OFFSET); // Sora ???

                Hypervisor.Write(0x17E7BF + 0x2400, POSITIVE_OFFSET); // Drive Return
                Hypervisor.Write(0x17F5A9 + 0x2400, POSITIVE_OFFSET); // Drive ???
                Hypervisor.Write(0x17F17D + 0x2400, POSITIVE_OFFSET); // ???
                Hypervisor.Write(0x17E5F8 + 0x2400, POSITIVE_OFFSET); // Drive Transform

                Hypervisor.Write(0x17EA5A + 0x2400, POSITIVE_OFFSET); // Summon Backdrop
                Hypervisor.Write(0x17EAFF + 0x2400, POSITIVE_OFFSET); // Summon Face

                Hypervisor.Write(0x18A156 + 0x2400, POSITIVE_OFFSET); // Mission Gauge - Align Right
                Hypervisor.Write(0x18A532 + 0x2400, NEGATIVE_OFFSET); // Mission Gauge - Align Left

                Hypervisor.Write(0x189666 + 0x2400, POSITIVE_OFFSET); // Mission Count - Align Right
                Hypervisor.Write(0x189357 + 0x2400, NEGATIVE_OFFSET); // Mission Count - Align Left

                Hypervisor.Write(0x18ACD7 + 0x2400, POSITIVE_OFFSET); // All Timers - Align Right
                Hypervisor.Write(0x18AAF6 + 0x2400, NEGATIVE_OFFSET); // All Timers - Align Left

                Hypervisor.Write(0x18C18F + 0x2400, NEGATIVE_OFFSET); // Get Munny
            }
        }

        public static void HandleRetry()
        {
            var _menuRead = Hypervisor.Read<int>(Variables.ADDR_MenuType);
            var _selectRead = Hypervisor.Read<byte>(Variables.ADDR_MenuSelect);

            var _gameOverRead = Hypervisor.Read<ulong>(Variables.PINT_GameOver);
            var _gameOverOptions = Hypervisor.Read<ulong>(Variables.PINT_GameOverOptions);

            var _subMenuRead = Hypervisor.Read<byte>(Variables.ADDR_SubMenuType);
            var _finishRead = Hypervisor.Read<byte>(Variables.ADDR_FinishFlag);

            var _pauseRead = Hypervisor.Read<byte>(Variables.ADDR_PauseFlag);
            var _battleRead = Hypervisor.Read<byte>(Variables.ADDR_BattleFlag);
            var _cutsceneRead = Hypervisor.Read<byte>(Variables.ADDR_CutsceneFlag);

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
                Label = 0x0127,
            };

            var _isEscape = _worldRead == 0x06 && _roomRead == 0x05 && _eventRead == 0x6F;

            var _blacklistCheck =
               (_worldRead == 0x04 && _roomRead >= 0x15 && _roomRead <= 0x1A)
            || (_worldRead == 0x06 && _roomRead == 0x09 && _eventRead >= 0xBD && _eventRead <= 0xC4);

            if (!_blacklistCheck && !Variables.IS_TITLE)
            {
                var _battleState = _battleRead == 0x02 && _cutsceneRead == 0x00;

                if (RETRY_BLOCK)
                {
                    Terminal.Log("Out of the blacklisted area. Retry functionality has been enabled.", 0);
                    RETRY_BLOCK = false;
                }

                if (_battleState && _pauseRead == 0x00 && !STATE_COPIED)
                {
                    Terminal.Log("A forced battle was started. Savestate was copied to memory.", 0);

                    if (_isEscape)
                    {
                        HADES_COUNT = 0;
                        Terminal.Log("Adjusted Retry Logic for the Hades Escape event.", 0);
                    }
                    
                    var _currentSave = Hypervisor.Read<byte>(Variables.ADDR_SaveData, 0x10FC0);

                    Hypervisor.Write(0x7A0000, _currentSave);
                    STATE_COPIED = true;

                    var _insertIndex = Variables.RETRY_DEFAULT ? 0 : 1;

                    Variables.CONTINUE_MENU.Children.Insert(_insertIndex, _entPrepare);
                    Variables.CONTINUE_MENU.Children.Insert(_insertIndex, _entRetry);
                }

                else if (!_battleState && _pauseRead == 0x00 && STATE_COPIED && !(_isEscape && _battleRead == 0x01) && RETRY_MODE == 0x00)
                {
                    Terminal.Log("The battle has ended. Savestate has been restored.", 0);
                    Hypervisor.Write(0x7A0000, new byte[0x10FC0]);

                    if (RETRY_MODE != 0x00)
                    {
                        Hypervisor.Write(WARP_OFFSET, WARP_FUNCTION);
                        Hypervisor.Write(INVT_OFFSET, INVT_FUNCTION);
                        RETRY_MODE = 0x00;
                    }

                    Variables.CONTINUE_MENU = new Continue();

                    STATE_COPIED = false;
                    ROXAS_KEYBLADE = 0x0000;
                }

                if (_isEscape && _battleRead == 0x01 && !DEBOUNCE[7])
                {
                    HADES_COUNT += 1;
                    DEBOUNCE[7] = true;
                    Terminal.Log("Incrementing the clear count for Hades Escape.", 0);
                }

                if (_isEscape && _battleRead == 0x02 && DEBOUNCE[7])
                    DEBOUNCE[7] = false;
               
                if (_gameOverRead == 0x00 && ((_finishRead == 0x01 || _cutsceneRead != 0x00) && RETRY_MODE != 0x00) || (_isEscape && HADES_COUNT == 3))
                {
                    Terminal.Log("The battle has ended. Disabled functions have been re-enabled.", 0);

                    if (_isEscape)
                    {
                        HADES_COUNT = 255;
                        Terminal.Log("The battle ended on a special edge-case.", 0);
                    }

                    Hypervisor.Write(WARP_OFFSET, WARP_FUNCTION);
                    Hypervisor.Write(INVT_OFFSET, INVT_FUNCTION);

                    RETRY_MODE = 0x00;
                }

                if (_gameOverRead != 0x00 && STATE_COPIED)
                {
                    var _retryButton = Variables.RETRY_DEFAULT ? 0x00 : 0x01;
                    var _prepareButton = Variables.RETRY_DEFAULT ? 0x01 : 0x02;
                    var _continueButton = Variables.RETRY_DEFAULT ? 0x02 : 0x00;

                    var _buttonCheck = _selectRead == _retryButton || _selectRead == _prepareButton;

                    if (!_buttonCheck && _selectRead < 0x04 && RETRY_MODE != 0x00)
                    {
                        Terminal.Log("Switched out of Retry, enabling functions.", 0);

                        Hypervisor.Write(WARP_OFFSET, WARP_FUNCTION);
                        Hypervisor.Write(INVT_OFFSET, INVT_FUNCTION);

                        RETRY_MODE = 0x00;
                        PREPARE_MODE = 0x00;
                    }

                    else if (_buttonCheck && RETRY_MODE == 0x00)
                    {
                        Terminal.Log("Switched into Retry, disabling functions.", 0);

                        Hypervisor.DeleteInstruction(WARP_OFFSET, 0x05);
                        Hypervisor.RedirectLEA(INVT_OFFSET, 0x7A0000);

                        RETRY_MODE = 0x01;
                    }

                    if (_selectRead == _prepareButton && RETRY_MODE != 0x02)
                    {
                        var _currentSave = Hypervisor.Read<byte>(0x7A0000, 0x10FC0);

                        Hypervisor.Write(Variables.ADDR_SaveData, _currentSave);

                        RETRY_MODE = 0x02;
                        PREPARE_MODE = 0x01;

                        Terminal.Log("Prepare and Retry is ready to execute.", 0);
                    }

                    else if (_selectRead == _retryButton && RETRY_MODE == 0x02)
                    {
                        RETRY_MODE = 0x01;
                        PREPARE_MODE = 0x00;
                    }
                }

                else
                {
                    if (PREPARE_MODE == 0x01 && _menuRead != 0x08)
                    {
                        Terminal.Log("Prepare request detected! Opening the Camp Menu...", 0);
                        Variables.SharpHook[OffsetCampMenu].Execute(BSharpConvention.MicrosoftX64, 0, 0);
                    }

                    else if (_menuRead == 0x08 && PREPARE_MODE == 0x01)
                        PREPARE_MODE = 0x02;

                    else if (PREPARE_MODE == 0x02 && _menuRead != 0x08)
                    {
                        Terminal.Log("Prepare finished! Copying the save state...", 0);
                        var _currentSave = Hypervisor.Read<byte>(Variables.ADDR_SaveData, 0x10FC0);
                        Hypervisor.Write(0x7A0000, _currentSave);
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
            // Prepare the suffic according to the language.
            var _suffixFile = (Variables.AUDIO_MODE == 0x00) ? ".a.us" : ".a.jp";

            // Read the values.
            var _loadRead = Hypervisor.Read<byte>(Variables.ADDR_LoadFlag);
            var _cutsceneRead = Hypervisor.Read<byte>(Variables.ADDR_CutsceneFlag);
            var _formRead = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0x3524);

            // If on the title, or the room ain't loaded, or the form changed: Wipe the cache.
            if (Variables.IS_TITLE || _loadRead == 0x00 || PAST_FORM != _formRead)
            {
                LOAD_LIST = null;
                PAST_FORM = _formRead;
            }

        RELOAD_POINT:

            // If not in a cutscene or the Title Screen, and the room is loaded:
            if (!Variables.IS_TITLE && _loadRead == 0x01 && _cutsceneRead == 0x00)
            {
                // Find the .a.xx files in game cache.
                LOAD_LIST =
                [
                    Operations.FindFile("obj/P_EX100" + _suffixFile),
                    Operations.FindFile("obj/P_EX100_BTLF" + _suffixFile),
                    Operations.FindFile("obj/P_EX100_MAGF" + _suffixFile),
                    Operations.FindFile("obj/P_EX100_KH1F" + _suffixFile),
                    Operations.FindFile("obj/P_EX100_TRIF" + _suffixFile),
                    Operations.FindFile("obj/P_EX100_ULTF" + _suffixFile),
                    Operations.FindFile("obj/P_EX100_HTLF" + _suffixFile)
                ];

                // Fetch the pointers to the files.
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

                // Calculate the crown.
                var _crownRead = Hypervisor.Read<byte>(Variables.ADDR_SaveData + 0x36B2, 0x03);
                var _crownSum = _crownRead[0] + _crownRead[1] + _crownRead[2];

                // If something occured whilst reading: Re-fetch everything.
                if (LOAD_LIST[_formRead] == 0xFFFFFFFFFFFFFFFF || _soraPoints[_formRead] > 0x7FFF00000000)
                {
                    LOAD_LIST = null;
                    goto RELOAD_POINT;
                }

                // For every .a.xx found:
                foreach (var _point in _soraPoints)
                {
                    // If the .a.xx file is valid:
                    if (_point != 0x00)
                    {
                        // Fetch the offsets to the coords.
                        var _barOffset = Hypervisor.Read<uint>(_point + 0x08, true);
                        var _soraOffset = Hypervisor.Read<uint>(_point + 0x38, true) - _barOffset;

                        var _faceCheck = Hypervisor.Read<uint>(_point + 0x24, true);

                        // Ensure the .a.xx has a face.
                        if (_faceCheck != 0x65636166)
                            return;

                        // Calculate the positions.
                        var _topValue = 0x00 + _crownSum * 0x5A;
                        var _bottomValue = 0x5D + _crownSum * 0x5A;

                        // Write the positions.
                        for (uint i = 0; i < 3; i++)
                        {
                            Hypervisor.Write(_point + _soraOffset + 0x38 + (0x2C * i), _topValue, true);
                            Hypervisor.Write(_point + _soraOffset + 0x40 + (0x2C * i), _bottomValue, true);
                        }
                    }
                }
            }
        }
    }
}
