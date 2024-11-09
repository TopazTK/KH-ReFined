using ReFined.Common;
using ReFined.KH2.Information;
using ReFined.Libraries;

using BSharpConvention = Binarysharp.MSharp.Assembly.CallingConvention.CallingConventions;

namespace ReFined.KH2.InGame
{
    public static class Operations
    {
        public static IntPtr OffsetFindFile;
        public static IntPtr OffsetGetFileSize;

        /// <summary>
        /// Fetches a string from a given MSG file.
        /// </summary>
        /// <param name="StartMSG">The address in which the MSG file starts. Must be a valid MSG file.</param>
        /// <param name="StringID">The ID of the String to fetch the pointer of.</param>
        /// <returns>The string requested in KHSCII.</returns>
        /// <exception cref="InvalidDataException"></exception>
        public static byte[] FetchStringMSG(ulong StartMSG, ushort StringID)
        {
            var _msnAbsolute = Hypervisor.Read<ulong>(StartMSG);

            var _checkFirst = Hypervisor.Read<int>(_msnAbsolute, true);
            var _checkSecond = Hypervisor.Read<int>(_msnAbsolute - 0x30, true);

            if (_checkFirst != 0x01 || _checkSecond != 0x01524142)
                return null;

            var _fetchCount = Hypervisor.Read<int>(_msnAbsolute + 0x04, true);
            var _fetchData = Hypervisor.Read<byte>(_msnAbsolute + 0x08, _fetchCount * 0x08, true);

            var _offsetLocal = _fetchData.FindValue<int>(StringID);

            var _offsetString = Hypervisor.Read<int>(_msnAbsolute + _offsetLocal + 0x0C, true);

            int _readOffset = 0;
            List<byte> _returnList = new List<byte>();

            while (true)
            {
                var _byte = Hypervisor.Read<byte>(_msnAbsolute + (ulong)(_offsetString + _readOffset), true);

                _returnList.Add(_byte);

                if (_byte == 0x00)
                    break;

                else
                    _readOffset++;
            }

            return _returnList.ToArray();
        }

        /// <summary>
        /// Fetches the absolute pointer of a String stored inside a MSG file.
        /// Usually needed for functions such as "ShowInformation".
        /// </summary>
        /// <param name="StartMSG">The address in which the MSG file starts. Must be a valid MSG file.</param>
        /// <param name="StringID">The ID of the String to fetch the pointer of.</param>
        /// <returns>The absolute pointer of the given string.</returns>
        public static long FetchPointerMSG(ulong StartMSG, ushort StringID)
        {
            var _msnAbsolute = Hypervisor.Read<ulong>(StartMSG);

            var _checkFirst = Hypervisor.Read<int>(_msnAbsolute, true);
            var _checkSecond = Hypervisor.Read<int>(_msnAbsolute - 0x30, true);

            if (_checkFirst != 0x01 || _checkSecond != 0x01524142)
                return 0x00;

            var _fetchCount = Hypervisor.Read<int>(_msnAbsolute + 0x04, true);
            var _fetchData = Hypervisor.Read<byte>(_msnAbsolute + 0x08, _fetchCount * 0x08, true);

            var _offsetLocal = _fetchData.FindValue<int>(StringID);

            var _offsetString = Hypervisor.Read<int>(_msnAbsolute + _offsetLocal + 0x0C, true);

            return (long)_msnAbsolute + _offsetString;
        }

        /// <summary>
        /// Fetches the offset of a String stored inside a MSG file.
        /// </summary>
        /// <param name="StartMSG">The address in which the MSG file starts. Must be a valid MSG file.</param>
        /// <param name="StringID">The ID of the String to fetch the pointer of.</param>
        /// <returns>The offset of the given string (Absolute).</returns>
        public static int FetchOffsetMSG(ulong StartMSG, ushort StringID)
        {
            var _msnAbsolute = Hypervisor.Read<ulong>(StartMSG);

            var _checkFirst = Hypervisor.Read<int>(_msnAbsolute, true);
            var _checkSecond = Hypervisor.Read<int>(_msnAbsolute - 0x30, true);

            if (_checkFirst != 0x01 || _checkSecond != 0x01524142)
                return 0x00;

            var _fetchCount = Hypervisor.Read<int>(_msnAbsolute + 0x04, true);
            var _fetchData = Hypervisor.Read<byte>(_msnAbsolute + 0x08, _fetchCount * 0x08, true);

            var _offsetLocal = _fetchData.FindValue(StringID);

            var _offsetString = Hypervisor.Read<int>(_msnAbsolute + _offsetLocal + 0x0C, true);

            return _offsetString;
        }

        /// <summary>
        /// Finds the info of a String stored inside a MSG file.
        /// </summary>
        /// <param name="StartMSG">The address in which the MSG file starts. Must be a valid MSG file.</param>
        /// <param name="StringID">The ID of the String to fetch the pointer of.</param>
        /// <returns></returns>
        public static ulong FindInfoMSG(ulong StartMSG, ushort StringID)
        {
            var _msnAbsolute = Hypervisor.Read<ulong>(StartMSG);

            var _checkFirst = Hypervisor.Read<int>(_msnAbsolute, true);
            var _checkSecond = Hypervisor.Read<int>(_msnAbsolute - 0x30, true);

            if (_checkFirst != 0x01 || _checkSecond != 0x01524142)
                return 0x00;

            var _fetchCount = Hypervisor.Read<int>(_msnAbsolute + 0x04, true);
            var _fetchData = Hypervisor.Read<byte>(_msnAbsolute + 0x08, _fetchCount * 0x08, true);

            var _offsetLocal = _fetchData.FindValue<int>(StringID);

            return _msnAbsolute + _offsetLocal + 0x08;
        }

        /// <summary>
        /// Finds a file in the Buffer Cache.
        /// </summary>
        /// <param name="Input">The name of the file.</param>
        /// <returns>The absolute position of the file indicator in memory. "0" if not found.</returns>
        public static ulong FindFile(string Input)
        {
            var _memoryOffset = Hypervisor.PureAddress & 0x7FFF00000000;
            var _returnValue = Variables.SharpHook[OffsetFindFile].Execute<uint>(BSharpConvention.MicrosoftX64, Input, -1);
            return _returnValue == 0x00 ? 0x00 : _memoryOffset + _returnValue;
        }

        /// <summary>
        /// Gets the size of the file, used primarily to check if a file actually exists.
        /// </summary>
        /// <param name="Input">The name of the file.</param>
        /// <returns>A 32-bit integer containing the size in bytes, "0" if the file is not found.</returns>
        public static int GetFileSize(string Input) => Variables.SharpHook[OffsetGetFileSize].Execute<int>(Input);

    }
}
