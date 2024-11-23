using System.Text.RegularExpressions;

namespace ReFined.Common
{
    public static class Extensions
    {
        public static byte GetBitwise(this ushort Input, ushort First, ushort Second = 0xCAFE, ushort Third = 0xDEAD) => (byte)((Input & First) == First ? 0x00 : (Input & Second) == Second ? 0x01 : (Input & Third) == Third ? 0x02 : 0x00);

        public static byte[] ToKHSCII(this string Input)
        {
            var _specialDict = new Dictionary<char, byte>
            {
                { ' ', 0x01 },
                { '\n', 0x02 },
                { '-', 0x54 },
                { '!', 0x48 },
                { '?', 0x49 },
                { '%', 0x4A },
                { '/', 0x4B },
                { '.', 0x4F },
                { ',', 0x50 },
                { ';', 0x51 },
                { ':', 0x52 },
                { '\'', 0x57 },
                { '(', 0x5A },
                { ')', 0x5B },
                { '[', 0x62 },
                { ']', 0x63 },
                { 'à', 0xB7 },
                { 'á', 0xB8 },
                { 'â', 0xB9 },
                { 'ä', 0xBA },
                { 'è', 0xBB },
                { 'é', 0xBC },
                { 'ê', 0xBD },
                { 'ë', 0xBE },
                { 'ì', 0xBF },
                { 'í', 0xC0 },
                { 'î', 0xC1 },
                { 'ï', 0xC2 },
                { 'ñ', 0xC3 },
                { 'ò', 0xC4 },
                { 'ó', 0xC5 },
                { 'ô', 0xC6 },
                { 'ö', 0xC7 },
                { 'ù', 0xC8 },
                { 'ú', 0xC9 },
                { 'û', 0xCA },
                { 'ü', 0xCB },
                { 'ç', 0xE8 },
                { 'À', 0xD0 },
                { 'Á', 0xD1 },
                { 'Â', 0xD2 },
                { 'Ä', 0xD3 },
                { 'È', 0xD4 },
                { 'É', 0xD5 },
                { 'Ê', 0xD6 },
                { 'Ë', 0xD7 },
                { 'Ì', 0xD8 },
                { 'Í', 0xD9 },
                { 'Î', 0xDA },
                { 'Ï', 0xDB },
                { 'Ñ', 0xDC },
                { 'Ò', 0xDD },
                { 'Ó', 0xDE },
                { 'Ô', 0xDF },
                { 'Ö', 0xE0 },
                { 'Ù', 0xE1 },
                { 'Ú', 0xE2 },
                { 'Û', 0xE3 },
                { 'Ü', 0xE4 },
                { '¡', 0xE5 },
                { '¿', 0xE6 },
                { 'Ç', 0xE7 }
            };

            var _outList = new List<byte>();
            var _charCount = 0;

            // Throughout the text, do:
            while (_charCount < Input.Length)
            {
                var _char = Input[_charCount];

                // Simple character conversion through mathematics.
                if (_char >= 'a' && _char <= 'z')
                {
                    _outList.Add((byte)(_char + 0x39));
                    _charCount++;
                }

                else if (_char >= 'A' && _char <= 'Z')
                {
                    _outList.Add((byte)(_char - 0x13));
                    _charCount++;
                }

                else if (_char >= '0' && _char <= '9')
                {
                    _outList.Add((byte)(_char + 0x60));
                    _charCount++;
                }

                // If it hits a "{", we will know it's a command, not a character.
                else if (_char == '{')
                {
                    // A command is 6 characters long, in the format of "{0xTT}",
                    // with the "TT" being the 2-digit encode for that command.
                    var _command = Input.Substring(_charCount, 0x06);

                    if (Regex.IsMatch(_command, "^{0x[a-fA-F0-9][a-fA-F0-9]}$"))
                    {
                        var _value = _command.Substring(0x01, 0x04);
                        _outList.Add(Convert.ToByte(_value, 0x10));
                        _charCount += 6;
                    }
                }

                // Should it be anything we do not know, we look through
                // the special dictionary.
                else
                {
                    if (_specialDict.ContainsKey(_char))
                        _outList.Add(_specialDict[_char]);

                    else
                        _outList.Add(0x01);
                    _charCount++;
                }
            }

            // When the list ends, we add a terminator and return the string.
            _outList.Add(0x00);
            return _outList.ToArray();
        }

        public static string FromKHSCII(this byte[] Input)
        {
            var _specialDict = new Dictionary<byte, char>
            {
                { 0x01 , ' ' },
                {  0x02, '\n' },
                { 0x54 , '-' },
                { 0x2C , '-' },
                { 0x48 , '!' },
                { 0x49 , '?' },
                { 0x4A , '%' },
                { 0x4B , '/' },
                { 0x4F , '.' },
                { 0x50 , ',' },
                { 0x51 , ';' },
                { 0x52 , ':' },
                {  0x57, '\'' },
                { 0x5A , '('},
                { 0x5B , ')'},
                { 0x62 , '['},
                { 0x63 , ']'},
                { 0xB7 , 'à'},
                { 0xB8 , 'á'},
                { 0xB9 , 'â'},
                { 0xBA , 'ä'},
                { 0xBB , 'è'},
                { 0xBC , 'é'},
                { 0xBD , 'ê'},
                { 0xBE , 'ë'},
                { 0xBF , 'ì'},
                { 0xC0 , 'í'},
                { 0xC1 , 'î'},
                { 0xC2 , 'ï'},
                { 0xC3 , 'ñ'},
                { 0xC4 , 'ò'},
                { 0xC5 , 'ó'},
                { 0xC6 , 'ô'},
                { 0xC7 , 'ö'},
                { 0xC8 , 'ù'},
                { 0xC9 , 'ú'},
                { 0xCA , 'û'},
                { 0xCB , 'ü'},
                { 0xE8 , 'ç'},
                { 0xD0 , 'À'},
                { 0xD1 , 'Á'},
                { 0xD2 , 'Â'},
                { 0xD3 , 'Ä'},
                { 0xD4 , 'È'},
                { 0xD5 , 'É'},
                { 0xD6 , 'Ê'},
                { 0xD7 , 'Ë'},
                { 0xD8 , 'Ì'},
                { 0xD9 , 'Í'},
                { 0xDA , 'Î'},
                { 0xDB , 'Ï'},
                { 0xDC , 'Ñ'},
                { 0xDD , 'Ò'},
                { 0xDE , 'Ó'},
                { 0xDF , 'Ô'},
                { 0xE0 , 'Ö'},
                { 0xE1 , 'Ù'},
                { 0xE2 , 'Ú'},
                { 0xE3 , 'Û'},
                { 0xE4 , 'Ü'},
                { 0xE5 , '¡'},
                { 0xE6 , '¿'},
                { 0xE7 , 'Ç'}
            };
            var _outList = new List<char>();
            var _charCount = 0;

            // Throughout the text, do:
            while (_charCount < Input.Length)
            {
                var _char = Input[_charCount];

                // Simple character conversion through mathematics.
                if (_char >= 0x9A && _char <= 0xB3)
                {
                    _outList.Add((char)(_char - 0x39));
                    _charCount++;
                }

                else if (_char >= 0x2E && _char <= 0x47)
                {
                    _outList.Add((char)(_char + 0x13));
                    _charCount++;
                }

                else if (_char >= 0x90 && _char <= 0x99)
                {
                    _outList.Add((char)(_char - 0x60));
                    _charCount++;
                }

                else
                {
                    if (_specialDict.ContainsKey(_char))
                        _outList.Add(_specialDict[_char]);

                    else
                        _outList.Add(' ');
                    _charCount++;
                }
            }

            return new String(_outList.ToArray());
        }

        public static int FetchIndexOf<T>(this List<T> Source, Func<T, bool> Predicate)
        {
            var _fetchElement = Source.First(Predicate);
            return Source.IndexOf(_fetchElement);
        }

        public static ulong FindValue(this byte[] Source, byte[] Value)
        {
            ulong _charSlot = (ulong)(Source.Length - Value.Length + 1);

            for (ulong i = 0; i < _charSlot; i++)
            {
                if (Source[i] != Value[0])
                    continue;

                for (ulong j = (ulong)Value.Length - 1; j >= 1; j--)
                {
                    if (Source[i + j] != Value[j])
                        break;

                    if (j == 1)
                        return i;
                }
            }
            return 0xFFFFFFFFFFFFFFFF;
        }

        public static ulong FindValue<T>(this byte[] Source, T Value)
        {
            var _pattern = (byte[])typeof(BitConverter).GetMethod("GetBytes", new[] { typeof(T) }).Invoke(null, new object[] { Value });
            ulong _charSlot = (ulong)(Source.Length - _pattern.Length + 1);

            for (ulong i = 0; i < _charSlot; i++)
            {
                if (Source[i] != _pattern[0])
                    continue;

                for (ulong j = (ulong)_pattern.Length - 1; j >= 1; j--)
                {
                    if (Source[i + j] != _pattern[j])
                        break;

                    if (j == 1)
                        return i;
                }
            }
            return 0xFFFFFFFFFFFFFFFF;
        }
    }
}
