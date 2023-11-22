using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace BardNetworking.Components
{
    internal class BardServer
    {

        Socket server;
        
        public List<Client> clients;
        public Dictionary<int, Socket> connections;
        public struct Client
        {
            public int id;
            public Socket socket;
            public Client(Socket socket, int id)
            {
                this.socket = socket;
                this.id = id;
            }
        }
        int clientIndex = 0;
        PacketReader reader;

        Thread serverThread;

      

        public bool IsRunning()
        {
            if (server == null)
            {
                return false;
            }
            return server.IsBound;
        }
        public BardServer(PacketReader reader)
        {
            this.reader = reader;
        }

        public void Start(int port = 7777)
        {
            serverThread = new Thread(async () =>
            {
                IPHostEntry host = Dns.GetHostEntry("localhost");
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
                server = new Socket(ipAddress.AddressFamily, SocketType.Stream, BardSettings.PROTOCOL_TYPE);
                Debug.Log("Starting server on " + localEndPoint + ".", LogSource.Client);
                server.Bind(localEndPoint);
                server.Listen(32);
                Debug.Log("Server started!", LogSource.Server);
                clients = new List<Client>();
                connections = new Dictionary<int, Socket>();
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
            Debug.Log("Stopping...", LogSource.Server);
            server.Close();
            server = null;
            Debug.Log("Stopped.", LogSource.Server);
        }
        private void UpdateServer()
        {
            AcceptConnections();
            UpdateAllClients();
        }
        private async void AcceptConnections()
        {
            Socket newClient = await server.AcceptAsync();
            clients.Add(new Client(newClient,clientIndex));
            connections.Add(clientIndex, newClient);
            Packet idPacket = new Packet(server, BuiltinPackets.ASSIGN_ID);
            idPacket.Write(clientIndex);
            Send(newClient,idPacket);
            Debug.Log("Client with id " + clientIndex + " connected.",LogSource.Server);
            clientIndex++;
        }

        private void UpdateAllClients()
        {
            for (int i = 0; i < clients.Count; i++)
            {
                Client client = clients[i];
                UpdateClient(client);
            }

        }
        private async void UpdateClient(Client client)
        {
            try
            {
                if (client.socket.IsConnected() && client.socket.Connected && client.socket.RemoteEndPoint != null)
                {
                    if (client.socket.Available > 0)
                    {
                        byte[] buffer = new byte[BardSettings.MAX_PACKET_SIZE];
                        int data = await client.socket.ReceiveAsync(buffer,SocketFlags.None);
                        int index = 0;
                        while (data > 0)
                        {
                            int packetSize = buffer[0];
                            index += packetSize;
                            HandlePacket(client.socket, buffer.Take(packetSize+1).ToArray());
                            buffer = buffer.Skip(packetSize).ToArray();
                            data -= packetSize;
                        }
                    }
                }
                else
                {
                    connections.Remove(client.id);
                    clients.Remove(client);
                }
            }
            catch (ObjectDisposedException e)
            {
                connections.Remove(client.id);
                clients.Remove(client);
            }

        }
        public void Send(Socket target, byte[] data, byte header = 0)
        {
            if (!server.IsBound) return;
            byte[] finalData = data.Prepend(header).ToArray();
            finalData = finalData.Prepend((byte)(data.Length + 2)).ToArray();
            target.Send(finalData);
        }
        public void Send(int target, byte[] data, byte header = 0)
        {
            if (!server.IsBound) return;
            byte[] finalData = data.Prepend(header).ToArray();
            finalData = finalData.Prepend((byte)(data.Length + 2)).ToArray();
            connections[target].Send(finalData);
        }

        public void Send(Socket target, Packet packet)
        {
            if (!server.IsBound) return;
            target.Send(packet);
        }
        public void Send(int target, Packet packet)
        {
            if (!server.IsBound) return;
            connections[target].Send(packet);
        }

        public void SendToAll(byte[] data, byte header = 0)
        {
            if (!server.IsBound) return;
            byte[] finalData = data.Prepend(header).ToArray();
            finalData = finalData.Prepend((byte)(data.Length + 2)).ToArray();
            foreach (Client client in clients.ToList())
            {
                client.socket.Send(finalData);
            }
        }
        public void SendToAll(Packet packet)
        {
            if (!server.IsBound) return;
            foreach (Client client in clients.ToList())
            {
                client.socket.Send(packet);
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

            reader.onReceivedPacketServer?.Invoke(this, reader.ConvertPacket(sender,packet));
        }

    }
}
