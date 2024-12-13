using System.Text;

using ReFined.Common;
using ReFined.Libraries;
using ReFined.KH2.InGame;

namespace ReFined.KH2.Information
{
    public static class Generators
    {
        public static ulong OffsetSaveSound;

        private static IEnumerable<uint> _fetchTable()
        {
            for (var x = 0; ; x++)
            {
                var r = x << 24;

                for (var j = 0; j < 0xff; j++)
                    r = r << 1 ^ (r < 0 ? 0x4C11DB7 : 0);

                yield return (uint)r;
            }
        }

        /// <summary>
        /// Generates the CRC32 Checksum for a Save File, courtesy of Xeeynamo.
        /// </summary>
        /// <param name="data">The save file itself.</param>
        /// <param name="offset">The offset of the true data.</param>
        /// <param name="checksum">The previous checksum, if any.</param>
        /// <returns>The CRC32 Checksum of the given data.</returns>
        public static uint SaveCRC32(byte[] data, int offset, uint checksum)
        {
            uint[] array = _fetchTable().Take(0x100).ToArray();

            for (var i = 0; i < offset; i++)
                checksum = array[(checksum >> 24) ^ data[i]] ^ (checksum << 8);

            return checksum ^ uint.MaxValue;
        }
        
        /// <summary>
        /// Generates and writes the current game state to both RAM and ROM Save Files.
        /// </summary>
        public static void GenerateSave()
        {
            // Prepare the strings.
            var _saveName = "BISLPM-66675FM-98";
            var _savePath = Hypervisor.ReadString(Hypervisor.GetPointer64(Variables.PINT_SaveInformation, [0x40]), true) + (Variables.PLATFORM == "EPIC" ? "\\KHIIFM.png" : "\\KHIIFM_WW.png");

            // Calculate the Unix Date.
            var _currDate = DateTime.Now;
            var _unix = new DateTime(1970, 1, 1);
            var _writeDate = Convert.ToUInt64((_currDate - _unix).TotalSeconds);

            // Prepare the variables for Save Info.
            var _saveSlot = 0;
            var _saveInfoLength = 0x158;
            var _saveDataLength = 0x10FC0;

            var _saveInfoStartRAM = Hypervisor.GetPointer64(Variables.PINT_SaveInformation, [0x10, 0x168]);
            var _saveDataStartRAM = Hypervisor.GetPointer64(Variables.PINT_SaveInformation, [0x10, 0x19630]);

            var _saveInfoStartFILE = 0x1C8;
            var _saveDataStartFILE = 0x19690;

            // Read the world.
            var _worldRead = Hypervisor.Read<byte>(Variables.ADDR_Area);

            // Read the save from RAM.
            var _saveData = Hypervisor.Read<byte>(Variables.ADDR_SaveData, _saveDataLength);

            // Read the save slot.
            var _saveSlotRAM = Hypervisor.Read<byte>(_saveInfoStartRAM + (ulong)(_saveInfoLength * _saveSlot), 0x11, true);

            if (!Encoding.Default.GetString(_saveSlotRAM).Contains("66675FM"))
            {
                Terminal.Log("Main file doesn't have a save! Autosave aborted.", 1);

                if (Variables.SAVE_MODE == 0x00 && _worldRead != 0x0F)
                {
                    Hypervisor.Write<byte>(OffsetSaveSound + 0x029, 0x28);
                    Thread.Sleep(5);
                    Popups.PopupInformation(0x5703);
                    Thread.Sleep(5);
                    Hypervisor.Write<byte>(OffsetSaveSound + 0x029, 0x22);
                }

                return;
            }

            // Seek out the physical slot of the save to make.
            while (_saveSlotRAM[0] != 0x00 && !Encoding.Default.GetString(_saveSlotRAM).Contains("66675FM-98"))
            {
                _saveSlot++;
                _saveSlotRAM = Hypervisor.Read<byte>(_saveInfoStartRAM + (ulong)(_saveInfoLength * _saveSlot), 0x11, true);
            }

            // Calculate the checksums.
            var _magicArray = _saveData.Take(0x08).ToArray();
            var _dataArray = _saveData.Skip(0x0C).ToArray();

            var _checkMagic = SaveCRC32(_magicArray, 8, uint.MaxValue);
            var _checkData = SaveCRC32(_dataArray, _dataArray.Length, _checkMagic ^ uint.MaxValue);

            #region RAM Save
            // Fetch the address for the save info.
            var _saveInfoAddrRAM = _saveInfoStartRAM + (ulong)(_saveInfoLength * _saveSlot);
            var _saveDataAddrRAM = _saveDataStartRAM + (ulong)(_saveDataLength * _saveSlot);

            // Write out the save information.
            Hypervisor.Write(_saveInfoAddrRAM, Encoding.Default.GetBytes(_saveName), true);

            // Write the date in which the save was made.
            Hypervisor.Write(_saveInfoAddrRAM + 0x40, _writeDate, true);
            Hypervisor.Write(_saveInfoAddrRAM + 0x48, _writeDate, true);

            // Write the length of the save.
            Hypervisor.Write(_saveInfoAddrRAM + 0x50, _saveDataLength, true);

            // Write the header.
            Hypervisor.Write(_saveDataAddrRAM, Encoding.ASCII.GetBytes("KH2J"), true);
            Hypervisor.Write<uint>(_saveDataAddrRAM + 0x04, 0x3A, true);

            // Write the checksum.
            Hypervisor.Write(_saveDataAddrRAM + 0x08, _checkData, true);

            // Write, the save.
            Hypervisor.Write(_saveDataAddrRAM + 0x0C, _dataArray, true);
            #endregion

            #region File Save

            // Fetch the address for the save info.
            var _saveInfoAddr = _saveInfoStartFILE + _saveInfoLength * _saveSlot;
            var _saveDataAddr = _saveDataStartFILE + _saveDataLength * _saveSlot;

            // Create the writer.
            using (var _stream = new FileStream(_savePath, FileMode.Open))
            using (var _write = new BinaryWriter(_stream))
            {
                // Write out the save information.
                _stream.Position = _saveInfoAddr;
                _write.Write(Encoding.ASCII.GetBytes(_saveName));

                // The date in which the save was made.
                _stream.Position = _saveInfoAddr + 0x40;
                _write.Write(_writeDate);
                _stream.Position = _saveInfoAddr + 0x48;
                _write.Write(_writeDate);

                // The length of the save.
                _stream.Position = _saveInfoAddr + 0x50;
                _write.Write(_saveDataLength);

                // Write the header.
                _stream.Position = _saveDataAddr;
                _write.Write(Encoding.ASCII.GetBytes("KH2J"));
                _stream.Position = _saveDataAddr + 0x04;
                _write.Write(0x3A);

                // Write the checksum.
                _stream.Position = _saveDataAddr + 0x08;
                _write.Write(_checkData);

                // Write, the save.
                _stream.Position = _saveDataAddr + 0x0C;
                _write.Write(_dataArray);
            }
            #endregion

            if (Variables.SAVE_MODE == 0x00 && _worldRead != 0x0F)
            {
                Hypervisor.Write<byte>(OffsetSaveSound + 0x029, 0x28);
                Thread.Sleep(5);
                Popups.PopupInformation(0x5702);
                Thread.Sleep(5);
                Hypervisor.Write<byte>(OffsetSaveSound + 0x029, 0x22);
            }
        }
    }
}
