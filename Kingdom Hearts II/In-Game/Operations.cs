using ReFined.Libraries;
using ReFined.KH2.Information;
using BSharpConvention = Binarysharp.MSharp.Assembly.CallingConvention.CallingConventions;
using ReFined.Common;

namespace ReFined.KH2.InGame
{
    public static class Operations
    {
        public static nint FUNC_FINDFILE;
        public static nint FUNC_OBJENTRYGET;
        public static nint FUNC_GETFILESIZE;
        public static nint FUNC_MESSAGEGETDATA;

        public static string GetStringHuman(short StringID)
        {
            var _messageOffset = Variables.SharpHook[FUNC_MESSAGEGETDATA].Execute(StringID);

            if (_messageOffset == IntPtr.Zero)
                return null;

            var _messageAbsolute = Hypervisor.MemoryOffset + (ulong)_messageOffset;

            ulong _readOffset = 0;
            List<byte> _returnList = new List<byte>();

            while (true)
            {
                var _byte = Hypervisor.Read<byte>(_messageAbsolute + _readOffset, true);

                _returnList.Add(_byte);

                if (_byte == 0x00)
                    break;

                else
                    _readOffset++;
            }

            return _returnList.ToArray().FromKHSCII();
        }

        public static byte[] GetStringLiteral(short StringID)
        {
            var _messageOffset = Variables.SharpHook[FUNC_MESSAGEGETDATA].Execute(StringID);
            
            if (_messageOffset == IntPtr.Zero)
                return null;

            var _messageAbsolute = Hypervisor.MemoryOffset + (ulong)_messageOffset;

            ulong _readOffset = 0;
            List<byte> _returnList = new List<byte>();

            while (true)
            {
                var _byte = Hypervisor.Read<byte>(_messageAbsolute + _readOffset, true);

                _returnList.Add(_byte);

                if (_byte == 0x00)
                    break;

                else
                    _readOffset++;
            }

            return _returnList.ToArray();
        }

        public static ulong GetStringPointer(short StringID)
        {
            var _messageOffset = Variables.SharpHook[FUNC_MESSAGEGETDATA].Execute(StringID);

            if (_messageOffset == IntPtr.Zero)
                return 0x00;

            return Hypervisor.MemoryOffset + (ulong)_messageOffset;
        }

        /// <summary>
        /// Finds a file in the Buffer Cache.
        /// </summary>
        /// <param name="Input">The name of the file.</param>
        /// <returns>The absolute position of the file indicator in memory. "0" if not found.</returns>
        public static ulong FetchBufferFile(string Input)
        {
            var _memoryOffset = Hypervisor.PureAddress & 0x7FFF00000000;
            var _returnValue = Variables.SharpHook[FUNC_FINDFILE].Execute<uint>(BSharpConvention.MicrosoftX64, Input, -1);
            return _returnValue == 0x00 ? 0x00 : _memoryOffset + _returnValue;
        }

        /// <summary>
        /// Gets the size of the file, used primarily to check if a file actually exists.
        /// </summary>
        /// <param name="Input">The name of the file.</param>
        /// <returns>A 32-bit integer containing the size in bytes, "0" if the file is not found.</returns>
        public static int GetFileSize(string Input) => Variables.SharpHook[FUNC_GETFILESIZE].Execute<int>(Input);

    
        /// <summary>
        /// Finds the absolute position of a Objentry Entry based on the given ID.
        /// If it cannot find said ID, it will return 0.
        /// </summary>
        /// <param name="ObjectID">ID of the object to find.</param>
        /// <returns>Absolute position in memory if found, 0x00 otherwise.</returns>
        public static ulong FindInObjentry(short ObjectID)
        {
            var _fetchObject = Variables.SharpHook[FUNC_OBJENTRYGET].Execute(ObjectID);

            if (_fetchObject == IntPtr.Zero)
                return 0x00;

            else
                return Hypervisor.MemoryOffset + (ulong)_fetchObject;
        }
    }
}
