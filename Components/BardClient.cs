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
    internal class BardClient
    {
        Socket client;

        PacketReader reader;

        public int clientId = -1;

        Thread clientThread;
        public BardClient(PacketReader reader)
        {
            this.reader = reader;
        }

        public bool IsConnected()
        {
            if (client == null)
            {
                return false;
            }
            return client.IsConnected();
        }
        public void Connect(string ip = "localhost", int port = 7777)
        {
            clientThread = new Thread(async () =>
            {
                IPHostEntry host = Dns.GetHostEntry(ip);
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                client = new Socket(ipAddress.AddressFamily, SocketType.Stream, BardSettings.PROTOCOL_TYPE);
                Debug.Log("Connecting to " + remoteEP.ToString(), LogSource.Client);
                try
                {
                    await client.ConnectAsync(remoteEP);
                    Debug.Log("Connected!", LogSource.Client);
                    ClientLoop();
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
                        client.Close();
                        client = null;
                        return;
                    }
                }
            });
            clientThread.Start();
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
            if (client.Connected)
            {
                byte[] rawData = new byte[25];

                Send(rawData, BuiltinPackets.DISCONNECT);
                while (client.IsConnected())
                {
                    await Task.Delay(1);
                }
                Debug.Log("Disconnected!", LogSource.Client);
            }
            else
            {
                Debug.Log("Client is not connected.", LogSource.Client, LogType.Warning);
            }
        }
        private async void UpdateClient()
        {
            if (client == null) return;
            try
            {
                byte[] buffer = new byte[BardSettings.MAX_PACKET_SIZE];
                int data = await client.ReceiveAsync(buffer, SocketFlags.None);
                while (data > 0)
                {
                    byte packetSize = buffer[0];
                    HandlePacket(client, buffer.Take(packetSize + 1).ToArray());
                    buffer = buffer.Skip(packetSize).ToArray();
                    data -= packetSize;
                }
            }
            catch (ObjectDisposedException)
            {
                client = null;
                Debug.Log("ObjectDisposedException: Removing client...", LogSource.Client, LogType.Warning);
            }
            catch (SocketException)
            {
                Debug.Log("SocketException: Closing client...", LogSource.Client, LogType.Warning);
                if (client != null)
                {
                    client.Close();
                }
            }
            catch
            {
                Debug.Log("Unknown error.", LogSource.Client, LogType.Error);
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
            reader.onReceivedPacketClient?.Invoke(this, reader.ConvertPacket(sender, packet[0], packet[1], packet.Skip(2).ToArray()));
        }

    }
}
