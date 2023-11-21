using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Reflection;

namespace BardNetworking.Components
{
    internal class BardServer
    {

        Socket server;

        public List<Socket> clients;

        PacketReader reader;

        Thread serverThread;
        public BardServer(PacketReader reader)
        {   
            this.reader = reader;
        }

        public async void Start(int port = 7777)
        {
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
  
            server = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Debug.Log("Starting server on " + localEndPoint+".", LogSource.Client);
            server.Bind(localEndPoint);
            server.Listen(32);
            Debug.Log("Server started!", LogSource.Server);
            clients = new List<Socket>();
            serverThread = new Thread(async() =>
            {
                while (true && server.IsBound)
                {
                    UpdateServer();
                    await Task.Delay(1);
                }
            });
            serverThread.Start();
        }

        public void Stop()
        {
            Debug.Log("Stopping...", LogSource.Client);
            server.Shutdown(SocketShutdown.Both);
            server.Close();
            Debug.Log("Stopped.", LogSource.Client);
        }
        private void UpdateServer()
        {
            AcceptConnections();
            UpdateAllClients();
        }
        private async void AcceptConnections()
        {  
            Socket newClient = await server.AcceptAsync();
            clients.Add(newClient);
        }

       private async void UpdateAllClients()
        {
            for (int i = 0; i < clients.Count; i++)
            {   Socket client = clients[i];
                byte[] clientPacket = await UpdateClient(client);
                HandlePacket(client,clientPacket);
            }

        }
        private async Task<byte[]> UpdateClient(Socket client)
        {
            if (client.Connected && client != null)
            {
                if (client.Available > 0)
                {
                    byte[] buffer = new byte[BardSettings.MAX_PACKET_SIZE];
                    int receivedBytes = await client.ReceiveAsync(buffer);
                    return buffer.Take(receivedBytes).ToArray();
                }
                else
                {
 
                    return new byte[0];
                }
            }
            else
            {
                if (client != null)
                {
                    clients.Remove(client);
                }
                return new byte[0];
            }
        }
        public void SendToAll(byte[] data, byte header = 0)
        {
            if (!server.IsBound) return;
            byte[] finalData = data.Prepend(header).ToArray();
            foreach (Socket client in clients)
            {
                client.Send(finalData);
            }
        }
        public void SendToAll(string text)
        {
            byte[] rawData = Encoding.UTF8.GetBytes(text);
            SendToAll(rawData, BuiltinPackets.TEXT);
        }
        protected virtual void HandlePacket(Socket sender, byte[] packet)
        {
            if (packet.Length == 0) return;
            reader.onReceivedPacket?.Invoke(null,reader.ConvertPacket(sender, packet[0], packet.Skip(1).ToArray()));    
        }
     
    }
}
