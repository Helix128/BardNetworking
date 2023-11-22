using BardNetworking.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BardNetworking.Unity
{
    public class BardNetwork : MonoBehaviour
    {
        public static BardNetwork instance;
        PacketReader reader;
        BardServer server;
        BardClient client;

        public string serverIp = "localhost";
        public int serverPort = 7777;

        public List<Object> spawnablePrefabs;
        private void Start()
        {
            reader = new PacketReader();
            UnityPackets.RegisterPackets(reader);
            server = new BardServer(reader);
            client = new BardClient(reader);

            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);
            DontDestroyOnLoad(gameObject);
        }
        public void StartServer()
        {
            server.Start(serverPort);
        }
        public void StopServer()
        {
            server.Stop();
        }
        public void Connect()
        {
            client.Connect(serverIp, serverPort);
        }
        public void Disconnect()
        {
            client.Disconnect();
        }
        public void StartHost()
        {
            StartServer();
            Connect();
        }
        public void StopHost()
        {
            StopServer();
            Disconnect();
        }

        public bool IsServerActive()
        {
            return server.IsRunning();
        }
        public bool IsClientActive()
        {
            return client.IsConnected();
        }

        public void NetworkSpawn(int owner, Object go, Vector3 position, Quaternion rotation)
        {
            GameObject newGo = Instantiate(go, position, rotation) as GameObject;
            BardIdentity identity;
            if (newGo.TryGetComponent(out identity))
            {
                identity.ownerId = owner;
            }
            if (IsServerActive())
            {
             //  server.SendToAll()
            }
        }
        public void NetworkSpawn(int owner, int go, Vector3 position, Quaternion rotation)
        {
            GameObject newGo = Instantiate(spawnablePrefabs[go], position, rotation) as GameObject;
            BardIdentity identity;
            if (newGo.TryGetComponent(out identity))
            {
                identity.ownerId = owner;
            }
            if (IsServerActive())
            {
             
            }
          
        }
    }
}
