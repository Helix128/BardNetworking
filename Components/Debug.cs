using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BardNetworking.Components
{
    public enum LogType
    {
        Info,
        Warning,
        Error
    }

    public enum LogSource
    {
        Client,
        Server
    }
    internal class Debug
    {
        static ConsoleColor[] consoleColors = new ConsoleColor[] { ConsoleColor.Green, ConsoleColor.Yellow, ConsoleColor.Red };
        public static void Log(string message, LogSource source, LogType type = LogType.Info)
        {
#if UNITY_EDITOR
         UnityEngine.Debug.Log(message);
         
#else  
            Console.ForegroundColor = consoleColors[(int)type];
            Console.WriteLine("[Bard|" + source.ToString() + "|" + type.ToString() + "] " + message);
            Console.ForegroundColor = ConsoleColor.White;
#endif
        }
    }
}
