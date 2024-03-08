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
        static string[] unityColors = new string[] { "#C6F508", "#F5A608", "#F54108" };
        public static void Log(object message, LogSource source, LogType type = LogType.Info)
        {
            if (BardSettings.DEBUG)
            {
                #if UNITY_EDITOR
                switch (type)
                {
                    case LogType.Info:
                        UnityEngine.Debug.Log("<b><color=" + unityColors[0] + ">[Bard " + source.ToString() + "|" + type.ToString() + "]</b> " + message + "</color>");
                        break;
                    case LogType.Warning:
                        UnityEngine.Debug.LogWarning("<b><color=" + unityColors[1] + ">[Bard " + source.ToString() + "|" + type.ToString() + "]</b> " + message + "</color>");
                        break;
                    case LogType.Error:
                        UnityEngine.Debug.LogError("<b><color=" + unityColors[2] + ">[Bard " + source.ToString() + "|" + type.ToString() + "]</b> " + message + "</color>");
                        break;
                    default:
                        break;
                }


                #else
                 Console.WriteLine("[Bard " + source.ToString() + "|" + type.ToString().ToUpperInvariant() + "] " + message);
                #endif
            }
        }
    }
}
