using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

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
  
            server = new Socket(ipAddress.AddressFamily, SocketType.Stream, BardSettings.PROTOCOL_TYPE);
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

       private void UpdateAllClients()
        {
            for (int i = 0; i < clients.Count; i++)
            {   Socket client = clients[i];
                UpdateClient(client);
            }

        }
        private async void UpdateClient(Socket client)
        {
            try
            {
                if (client.IsConnected()&&client.Connected&&client.RemoteEndPoint!=null)
                {
                    if (client.Available > 0)
                    {
                        byte[] buffer = new byte[BardSettings.MAX_PACKET_SIZE];
                        int data = await client.ReceiveAsync(buffer);
                        int index = 0;
                        while (data > 0)
                        {
                            int packetSize = buffer[0];
                            index += packetSize;
                            HandlePacket(client, buffer.Take(new Range(0, packetSize+1)).ToArray());
                            buffer = buffer.Skip(packetSize).ToArray();
                            data -= packetSize;
                        }
                    }
                }
                else
                {
                    clients.Remove(client);

                }
            }
            catch (ObjectDisposedException e)
            {
                clients.Remove(client); 
            }
         
        }
        public void SendToAll(byte[] data, byte header = 0)
        {
            if (!server.IsBound) return;
          	byte[] finalData = data.Prepend(header).ToArray();
			finalData = finalData.Prepend((byte)(data.Length+2)).ToArray();
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
            reader.onReceivedPacket?.Invoke(null,reader.ConvertPacket(sender, packet[0],packet[1], packet.Skip(2).ToArray()));    
        }
     
    }
}
