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

		bool isActive = false;
        PacketReader reader;

        public BardClient(PacketReader reader)
        {
            this.reader = reader;
        }

        public bool IsConnected()
		{
			return client.Connected&&isActive;
		}
		public async void Connect(string ip = "localhost", int port = 7777)
		{
			IPHostEntry host = Dns.GetHostEntry(ip);
			IPAddress ipAddress = host.AddressList[0];
			IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

			client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			Debug.Log("Connecting to " + remoteEP.ToString(),LogSource.Client);
			await client.ConnectAsync(remoteEP);
			isActive = true;
			Debug.Log("Connected!", LogSource.Client);
			while (client.IsConnected()&&isActive)
			{
				UpdateClient();
				await Task.Delay(1);
            }
        }
        public void Disconnect()
		{
            byte[] rawData = Encoding.UTF8.GetBytes("aa");
            Debug.Log("Disconnecting...",LogSource.Client);
			Send(rawData,BuiltinPackets.DISCONNECT);
		}
		private async void UpdateClient()
		{
            if (client == null) return;
            try
			{
				byte[] buffer = new byte[BardSettings.MAX_PACKET_SIZE];
				int data = await client.ReceiveAsync(buffer);
				if (data > 0)
				{
					HandlePacket(client,buffer.Take(data).ToArray());
				}
			}
			catch
			{
				if (client != null)
				{
					client.Close();
				}
				client = null;
				isActive = false;
			}
		}
		public void Send(byte[] data,byte header = 0)
        {
			byte[] finalData = data.Prepend(header).ToArray();
            client.Send(finalData);
		}
		public void Send(string text)
		{  
			byte[] rawData = Encoding.UTF8.GetBytes(text);
			Send(rawData,BuiltinPackets.TEXT);
		}
		public virtual void HandlePacket(Socket sender, byte[] packet)
		{
            reader.onReceivedPacket?.Invoke(null, reader.ConvertPacket(sender, packet[0], packet.Skip(1).ToArray()));
        }
	
		}
}
