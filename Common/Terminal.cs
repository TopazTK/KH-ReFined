using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReFined.Common
{
    public static class Terminal
    {
        static string _logFileName = "";

        public static void Log(string Input, byte Type)
        {
            try
            {
                var _documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var _logDir = Path.Combine(_documentsPath, "Kingdom Hearts/Logs");

                var _formatStr = "{0}{1}: {2}";

                var _dateStr = DateTime.Now.ToString("dd-MM-yyyy");
                var _timeStr = "[" + DateTime.Now.ToString("hh:mm:ss") + "] ";

                var _session = 1;

                var _typeStr = "";

                if (_logFileName == "")
                {
                    _logFileName = "ReFined-" + _dateStr + ".txt";

                FILE_CHECK:
                    if (File.Exists(Path.Combine(_logDir, _logFileName)))
                    {
                        _logFileName = "ReFined-" + _dateStr + "_SESSION_" + _session + ".txt";
                        _session++;

                        goto FILE_CHECK;
                    }
                }

                switch (Type)
                {
                    case 0:
                        _typeStr = "MESSAGE";
                        break;

                    case 1:
                        _typeStr = "WARNING";
                        break;

                    case 2:
                        _typeStr = "ERROR";
                        break;
                }

                Console.Write(_timeStr);
                Console.ForegroundColor = Type == 0x00 ? ConsoleColor.Green : (Type == 0x01 ? ConsoleColor.Yellow : ConsoleColor.Red);
                Console.Write(_typeStr + ": ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(Input);

                using (StreamWriter _write = File.AppendText(Path.Combine(_logDir, _logFileName)))
                    _write.WriteLine(String.Format(_formatStr, _timeStr, _typeStr, Input));

            }

            catch (Exception) { }
        }

        public static void Log(Exception Input)
        {
            try
            {
                var _documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var _logDir = Path.Combine(_documentsPath, "Kingdom Hearts/Logs");

                var _formatStr = "[{0}] {1}";

                var _dateStr = DateTime.Now.ToString("dd-MM-yyyy");
                var _timeStr = DateTime.Now.ToString("hh:mm:ss");

                var _session = 1;

                if (_logFileName == "")
                {
                    _logFileName = "ReFined-" + _dateStr + ".txt";

                FILE_CHECK:
                    if (File.Exists(Path.Combine(_logDir, _logFileName)))
                    {
                        _logFileName = "ReFined-" + _dateStr + "_SESSION_" + _session + ".txt";
                        _session++;

                        goto FILE_CHECK;
                    }
                }

                var _exString = Input.ToString().Replace("   ", "").Replace(System.Environment.NewLine, " ");

                Console.Write("[" + _timeStr + "] ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("EXCEPTION: ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(_exString);

                using (StreamWriter _write = File.AppendText(Path.Combine(_logDir, _logFileName)))
                    _write.WriteLine(String.Format(_formatStr, _timeStr, _exString));
            }

            catch (Exception) { }
        }
    }
}
