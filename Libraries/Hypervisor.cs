using System.Diagnostics;
using System.Globalization;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;

namespace ReFined.Libraries
{
    public static class Hypervisor
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint flNewProtect, ref int lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        static IntPtr Handle;
        public static Process Process;
        public static ulong PureAddress;
        public static ulong MemoryOffset;

        static byte[]? _patternBuffer = null;

        /// <summary>
        /// Initialize the Hypervisor on a process.
        /// </summary>
        /// <param name="Input">The input process.</param>
        public static void AttachProcess(Process Input)
        {
            Process = Input;
            Handle = Input.Handle;
            PureAddress = (ulong)Input.MainModule.BaseAddress;
            MemoryOffset = PureAddress & 0x7FFF00000000;
        }

        /// <summary>
        /// Reads a value with the type of T from an address.
        /// Unsafe, must be used with caution.
        /// </summary>
        /// <typeparam name="T">Type of the value to read.</typeparam>
        /// <param name="Address">The address of the value to read.</param>
        /// <param name="Absolute">If the address is absolute, false by default.</param>
        /// <returns>The value as it is read from memory.</returns>
        public static T Read<T>(ulong Address, bool Absolute = false) where T : struct
        {
            var _address = (IntPtr)Address;

            if (!Absolute)
                _address = (IntPtr)(PureAddress + Address);

            var _dynoMethod = new DynamicMethod("SizeOfType", typeof(int), []);
            ILGenerator _ilGen = _dynoMethod.GetILGenerator();

            _ilGen.Emit(OpCodes.Sizeof, typeof(T));
            _ilGen.Emit(OpCodes.Ret);

            var _outSize = (int)_dynoMethod.Invoke(null, null);

            var _outArray = new byte[_outSize];
            int _outRead = 0;

            ReadProcessMemory(Handle, _address, _outArray, _outSize, ref _outRead);

            var _outType = typeof(T);

            if (_outType.IsEnum)
            {
                var _gcHandle = GCHandle.Alloc(_outArray, GCHandleType.Pinned);
                var _retData = (T)Marshal.PtrToStructure(_gcHandle.AddrOfPinnedObject(), Enum.GetUnderlyingType(_outType));

                _gcHandle.Free();

                return _retData;
            }

            else
            {
                var _gcHandle = GCHandle.Alloc(_outArray, GCHandleType.Pinned);
                var _retData = (T)Marshal.PtrToStructure(_gcHandle.AddrOfPinnedObject(), typeof(T));

                _gcHandle.Free();

                return _retData;
            }
        }

        /// <summary>
        /// Reads an array with the type of T[] from an address.
        /// Unsafe, must be used with caution.
        /// </summary>
        /// <typeparam name="T">Type of the array to read.</typeparam>
        /// <param name="Address">The address of the value to read.</param>
        /// <param name="Size">The size of the array to read.</param>
        /// <param name="Absolute">If the address is absolute, false by default.</param>
        /// <returns>The array as it is read from memory.</returns>
        public static T[] Read<T>(ulong Address, int Size, bool Absolute = false) where T : struct
        {
            var _address = (IntPtr)Address;

            if (!Absolute)
                _address = (IntPtr)(PureAddress + Address);

            var _dynoMethod = new DynamicMethod("SizeOfType", typeof(int), []);
            ILGenerator _ilGen = _dynoMethod.GetILGenerator();

            _ilGen.Emit(OpCodes.Sizeof, typeof(T));
            _ilGen.Emit(OpCodes.Ret);

            var _outSize = (int)_dynoMethod.Invoke(null, null);

            var _outArray = new byte[Size * _outSize];
            int _outRead = 0;

            ReadProcessMemory(Handle, _address, _outArray, Size * _outSize, ref _outRead);

            var _outType = typeof(T);

            if (_outType.IsEnum)
            {
                var _enumType = Enum.GetUnderlyingType(_outType);
                var _retArray = Array.CreateInstance(_enumType, Size);

                for (int i = 0; i < Size; i++)
                {
                    var _pickArray = _outArray.Skip(i * _outSize).Take(_outSize).ToArray();

                    var _gcHandle = GCHandle.Alloc(_pickArray, GCHandleType.Pinned);
                    var _convData = (T)Marshal.PtrToStructure(_gcHandle.AddrOfPinnedObject(), _enumType);

                    _retArray.SetValue(_convData, i);
                    _gcHandle.Free();
                }

                var _convArray = new T[Size];
                _retArray.CopyTo(_convArray, 0);

                return _convArray;
            }

            else
            {
                var _retArray = new T[Size];

                for (int i = 0; i < Size; i++)
                {
                    var _pickArray = _outArray.Skip(i * _outSize).Take(_outSize).ToArray();

                    var _gcHandle = GCHandle.Alloc(_pickArray, GCHandleType.Pinned);
                    var _convData = (T)Marshal.PtrToStructure(_gcHandle.AddrOfPinnedObject(), typeof(T));

                    _retArray[i] = _convData;
                    _gcHandle.Free();
                }

                return _retArray;

            }
        }

        /// <summary>
        /// Writes a value with the type of T to an address.
        /// Unsafe, must be used with caution.
        /// </summary>
        /// <typeparam name="T">Type of the value to write. Must have a size.</typeparam>
        /// <param name="Address">The address which the value will be written to.</param>
        /// <param name="Value">The value to write.</param>
        /// <param name="Absolute">If the address is absolute, false by default.</param>
        public static void Write<T>(ulong Address, T Value, bool Absolute = false) where T : struct
        {
            var _address = (IntPtr)Address;

            if (!Absolute)
                _address = (IntPtr)(PureAddress + Address);

            var _dynoMethod = new DynamicMethod("SizeOfType", typeof(int), []);
            ILGenerator _ilGen = _dynoMethod.GetILGenerator();

            _ilGen.Emit(OpCodes.Sizeof, typeof(T));
            _ilGen.Emit(OpCodes.Ret);

            var _inSize = (int)_dynoMethod.Invoke(null, null);
            int _inWrite = 0;

            if (_inSize > 1)
            {
                var _inArray = (byte[])typeof(BitConverter).GetMethod("GetBytes", new[] { typeof(T) }).Invoke(null, new object[] { Value });
                WriteProcessMemory(Handle, _address, _inArray, _inArray.Length, ref _inWrite);
            }

            else
            {
                var _inArray = new byte[] { (byte)Convert.ChangeType(Value, typeof(byte)) };
                WriteProcessMemory(Handle, _address, _inArray, _inArray.Length, ref _inWrite);
            }
        }

        /// <summary>
        /// Writes an array with the type of T to an address.
        /// Unsafe, must be used with caution.
        /// </summary>
        /// <typeparam name="T">Type of the array to write. Must have a size.</typeparam>
        /// <param name="Address">The address which the Array will be written to.</param>
        /// <param name="Value">The array to write.</param>
        /// <param name="Absolute">If the address is absolute, false by default.</param>
        public static void Write<T>(ulong Address, T[] Value, bool Absolute = false) where T : struct
        {
            var _address = (IntPtr)Address;

            if (!Absolute)
                _address = (IntPtr)(PureAddress + Address);

            var _dynoMethod = new DynamicMethod("SizeOfType", typeof(int), []);
            ILGenerator _ilGen = _dynoMethod.GetILGenerator();

            _ilGen.Emit(OpCodes.Sizeof, typeof(T));
            _ilGen.Emit(OpCodes.Ret);

            var _inSize = (int)_dynoMethod.Invoke(null, null);
            int _inWrite = 0;

            if (_inSize > 1)
            {
                for (int i = 0; i < Value.Length; i++)
                {
                    var _inArray = (byte[])typeof(BitConverter).GetMethod("GetBytes", [typeof(T)]).Invoke(null, [Value[i]]);
                    WriteProcessMemory(Handle, _address + _inSize * i, _inArray, _inArray.Length, ref _inWrite);
                }
            }

            else
                WriteProcessMemory(Handle, _address, Value as byte[], Value.Length, ref _inWrite);
        }

        /// <summary>
        /// Reads a terminated string from address.
        /// </summary>
        /// <param name="Address">The address which the value will be read from.</param>
        /// <param name="Absolute">Whether the address is an absolute address or not. Defaults to false.</param>
        /// <returns></returns>
        public static string ReadString(ulong Address, bool Absolute = false)
        {
            IntPtr _address = (IntPtr)(PureAddress + Address);

            if (Absolute)
                _address = (IntPtr)(Address);

            var _length = 0;

            while (Read<byte>((ulong)(_address + _length), true) != 0x00)
                _length++;

            var _outArray = new byte[_length];
            int _outRead = 0;

            ReadProcessMemory(Handle, _address, _outArray, _length, ref _outRead);

            return Encoding.Default.GetString(_outArray);
        }

        /// <summary>
        /// Writes a string to an address.
        /// </summary>
        /// <param name="Address">The address which the value will be written to.</param>
        /// <param name="Value">The string to write.</param>
        /// <param name="Absolute">Whether the address is an absolute address or not. Defaults to false.</param>
        public static void WriteString(ulong Address, string Value, bool Absolute = false, bool Unicode = false)
        {
            IntPtr _address = (IntPtr)(PureAddress + Address);

            if (Absolute)
                _address = (IntPtr)(Address);

            int _inWrite = 0;

            var _stringArray = Encoding.GetEncoding(437).GetBytes(Value);

            if (Unicode)
                _stringArray = Encoding.Unicode.GetBytes(Value);


            WriteProcessMemory(Handle, _address, _stringArray, _stringArray.Length, ref _inWrite);
        }

        /// <summary>
        /// Calculated a 64-bit pointer with the given offsets.
        /// All offsets are added and the resulting address is read.
        /// </summary>
        /// <param name="Address">The starting point to the pointer.</param>
        /// <param name="Offsets">All the offsets of the pointer, null by default.</param>
        /// <param name="Absolute">If the address is absolute, false by default.</param>
        /// <returns>The final calculated pointer.</returns>
        public static ulong GetPointer64(ulong Address, uint[] Offsets = null, bool Absolute = false)
        {
            var _returnPoint = Read<ulong>(Address, Absolute);

            if (Offsets == null)
                return _returnPoint;

            for (int i = 0; i < Offsets.Length - 1; i++)
                _returnPoint = Read<ulong>(_returnPoint + Offsets[i], true);

            return _returnPoint + Offsets.Last();
        }

        /// <summary>
        /// Calculated a 32-bit pointer with the given offsets.
        /// All offsets are added and the resulting address is read.
        /// </summary>
        /// <param name="Address">The starting point to the pointer.</param>
        /// <param name="Offsets">All the offsets of the pointer, null by default.</param>
        /// <param name="Absolute">If the address is absolute, false by default.</param>
        /// <returns>The final calculated pointer.</returns>
        public static uint GetPointer32(ulong Address, uint[] Offsets = null, bool Absolute = false)
        {
            var _returnPoint = Read<uint>(Address, Absolute);

            if (Offsets == null)
                return _returnPoint;

            for (int i = 0; i < Offsets.Length - 1; i++)
                _returnPoint = Read<uint>(_returnPoint + Offsets[i], true);

            return _returnPoint + Offsets.Last();
        }

        /// <summary>
        /// Redirects a LEA instruction to another address. Given it expects a relative pointer.
        /// </summary>
        /// <param name="Address">The instruction address.</param>
        /// <param name="Destination">The address in memory it will be reditected to.</param>
        /// <param name="Absolute">If the address is absolute, false by default.</param>
        public static void RedirectLEA(ulong Address, uint Destination, bool Absolute = false)
        {
            var _instEnding = (uint)Address + 0x07;
            var _instMath = Destination - _instEnding;
            Write(Address + 0x03, BitConverter.GetBytes(_instMath), Absolute);
        }        
        
        /// <summary>
        /// Redirects a MOV instruction to another address. Given it expects a relative pointer.
        /// </summary>
        /// <param name="Address">The instruction address.</param>
        /// <param name="Destination">The address in memory it will be reditected to.</param>
        /// <param name="Absolute">If the address is absolute, false by default.</param>
        public static void RedirectMOV(ulong Address, uint Destination, bool Absolute = false)
        {
            var _instEnding = (uint)Address + 0x06;
            var _instMath = Destination - _instEnding;
            Write(Address + 0x02, BitConverter.GetBytes(_instMath), Absolute);
        }
                
        /// <summary>
        /// Redirects a CMP instruction to another address. Given it expects a relative pointer.
        /// </summary>
        /// <param name="Address">The instruction address.</param>
        /// <param name="Destination">The address in memory it will be reditected to.</param>
        /// <param name="Absolute">If the address is absolute, false by default.</param>
        public static void RedirectCMP(ulong Address, uint Destination, bool Absolute = false)
        {
            var _instEnding = (uint)Address + 0x07;
            var _instMath = Destination - _instEnding;
            Write(Address + 0x02, BitConverter.GetBytes(_instMath), Absolute);
        }

        /// <summary>
        /// NOPs an instruction, given it's length.
        /// </summary>
        /// <param name="Address">The instruction address.</param>
        /// <param name="Absolute">If the address is absolute, false by default.</param>
        public static void DeleteInstruction(ulong Address, int Length, bool Absolute = false) => Write(Address, Enumerable.Repeat<byte>(0x90, Length).ToArray(), Absolute);

        /// <summary>
        /// Finds a signature in memory. Uses the string signature pattern.
        /// Refuses to return more than one result.
        /// </summary>
        /// <param name="Input">The signature to find.</param>
        /// <returns>The address of said signature.</returns>
        /// <exception cref="InvalidDataException">The pattern hit more or less than 1 result.</exception>
        public static IntPtr FindSignature(string Input)
        {
            if (_patternBuffer == null)
                _patternBuffer = Read<byte>(0x00, Process.MainModule.ModuleMemorySize);

            var _sigBytes = Input.Split(' ');
            int[] _sigList = new int[_sigBytes.Length];

            for (int i = 0; i < _sigList.Length; i++)
            {
                if (_sigBytes[i] == "??")
                    _sigList[i] = -1;
                else
                    _sigList[i] = int.Parse(_sigBytes[i], NumberStyles.HexNumber);
            }

            var results = new List<IntPtr>();

            for (int a = 0; a < _patternBuffer.Length; a++)
            {
                for (int b = 0; b < _sigList.Length; b++)
                {
                    if (_sigList[b] != -1 && _sigList[b] != _patternBuffer[a + b])
                        break;
                    if (b + 1 == _sigList.Length)
                    {
                        var result = new IntPtr(a);
                        results.Add(result);
                    }
                }
            }

            if (results.Count != 1)
                throw new InvalidDataException("ERROR: Signature scan error -- Either none or more than one found!");

            return results[0];
        }

        /// <summary>
        /// Unlocks a particular block to be written.
        /// </summary>
        /// <param name="Address">The address of the subject block.</param>
        /// <param name="Absolute">If the address is absolute, false by default.</param>
        public static void UnlockBlock(ulong Address, bool Absolute = false)
        {
            var _address = (IntPtr)Address;

            if (!Absolute)
                _address = (IntPtr)(PureAddress + Address);

            int _oldProtect = 0;
            VirtualProtectEx(Handle, _address, 0x100000, 0x40, ref _oldProtect);
        }
    }
}