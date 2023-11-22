using BardNetworking.Components;
using System.Threading.Tasks;
using UnityEngine;
namespace BardNetworking.Demos
{
    public class BardUnityDemo : MonoBehaviour
    {
        BardServer server;
        BardClient client;
        PacketReader reader;

        public static PacketType SPAWN_PACKET;
        
        private void Start()
        {
            reader = new PacketReader();
         
            server = new BardServer(reader);
            client = new BardClient(reader);
        }
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(60, 60, 240, 240));

            if (!server.IsRunning()) { if (GUILayout.Button("Start server")) { server.Start(); } } else { if (GUILayout.Button("Stop server")) { server.Stop(); } }
            if (!client.IsConnected()) { if (GUILayout.Button("Start client")) { client.Connect(); } } else { if (GUILayout.Button("Stop client")) { client.Disconnect(); } }
            GUILayout.EndArea();
        }
    }
}