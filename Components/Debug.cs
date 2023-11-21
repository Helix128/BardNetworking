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
        public static void Log(object message, LogSource source, LogType type = LogType.Info)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log("[Bard|" + source.ToString() + "|" + type.ToString() + "] " + message);
         
#else  
            Console.WriteLine("[Bard " + source.ToString() + "|" + type.ToString().ToUpperInvariant() + "] " + message);
#endif
        }
    }
}
