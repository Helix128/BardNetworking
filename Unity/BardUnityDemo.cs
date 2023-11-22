using BardNetworking.Components;
using System.Threading.Tasks;
using UnityEngine;
namespace BardNetworking.Unity
{
    public class BardUnityDemo : MonoBehaviour
    {
        BardNetwork network;
        
        private void Start()
        {
           network =GetComponent<BardNetwork>();    
        }
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(60, 60, 240, 240));

            if (!network.IsServerActive()) { if (GUILayout.Button("Start server")) { network.StartServer(); } } else { if (GUILayout.Button("Stop server")) { network.StopServer(); } }
            if (!network.IsClientActive()) { if (GUILayout.Button("Start client")) { network.Connect(); } } else { if (GUILayout.Button("Stop client")) { network.Disconnect(); } }
            if (!network.IsClientActive()&&!network.IsServerActive()) { if (GUILayout.Button("Start host")) { network.StartHost(); } } else { if (!network.IsClientActive() && !network.IsServerActive()) if(GUILayout.Button("Stop host")) { network.Disconnect(); } }
            GUILayout.EndArea();
        }
    }
}