using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace BardNetworking.Components
{
    internal class BardClient
    {
        Socket client;

        PacketReader reader;

        public BardClient(PacketReader reader)
        {
            this.reader = reader;
        }

        public bool IsConnected()
        {
            return client.Connected;
        }
        public async Task<bool> Connect(string ip = "localhost", int port = 7777)
        {
            IPHostEntry host = Dns.GetHostEntry(ip);
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            client = new Socket(ipAddress.AddressFamily, SocketType.Stream, BardSettings.PROTOCOL_TYPE);
            Debug.Log("Connecting to " + remoteEP.ToString(), LogSource.Client);
            try
            {
                await client.ConnectAsync(remoteEP);
            }
            catch (Exception ex)
            {
                try
                {
                    Debug.Log("Retrying connection...", LogSource.Client, LogType.Warning);
                    await client.ConnectAsync(remoteEP);
                }
                catch
                {
                    Debug.Log("Could not connect to server.", LogSource.Client, LogType.Error);
                    return false;

                }

            }


            Debug.Log("Connected!", LogSource.Client);
            ClientLoop();
            return true;


        }

        async void ClientLoop()
        {
            while (client.IsConnected())
            {
                UpdateClient();
                await Task.Delay(1);
            }
        }

        public async void Disconnect()
        {
            byte[] rawData = new byte[25];

            Send(rawData, BuiltinPackets.DISCONNECT);
            while (client.IsConnected())
            {
                await Task.Delay(1);
            }
            Debug.Log("Disconnected!", LogSource.Client);
        }
        private async void UpdateClient()
        {
            if (client == null) return;
            try
            {
                byte[] buffer = new byte[BardSettings.MAX_PACKET_SIZE];
                int data = await client.ReceiveAsync(buffer);

                while (data > 0)
                {

                    byte packetSize = buffer[0];

                    HandlePacket(client, buffer.Take(new Range(0, packetSize + 1)).ToArray());
                    buffer = buffer.Skip(packetSize).ToArray();
                    data -= packetSize;
                }
            }
            catch
            {
                if (client != null)
                {
                    client.Close();
                }
                client = null;

            }
        }
        public void Send(byte[] data, byte header = 0)
        {
            byte[] finalData = data.Prepend(header).ToArray();
            finalData = finalData.Prepend((byte)(data.Length + 2)).ToArray();
            try
            {
                client.Send(finalData);
            }
            catch (SocketException)
            {
                Disconnect();
            }
        }
        public void Send(string text)
        {
            byte[] rawData = Encoding.UTF8.GetBytes(text);
            Send(rawData, BuiltinPackets.TEXT);
        }
        public virtual void HandlePacket(Socket sender, byte[] packet)
        {
            reader.onReceivedPacket?.Invoke(null, reader.ConvertPacket(sender, packet[0], packet[1], packet.Skip(2).ToArray()));
        }

    }
}
