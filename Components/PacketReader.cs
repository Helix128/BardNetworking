using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Reflection;

namespace BardNetworking.Components
{
    public class Packet : EventArgs
    {
        public Socket sender;
        public byte header;
        public byte size;
        public byte[] data;
        public Packet(Socket sender, byte size, byte header, byte[] data)
        {
            this.sender = sender;
            this.header = header;
            this.size = size;
            this.data = data;
        }
        public byte[] GetBytes()
        {
            byte[] bytes = data;
            data.Prepend(header);
            data.Prepend((byte)(data.Length+2));
            return bytes;
        }
    }

    internal class PacketReader
    {   
        public PacketReader()
        {
                BuiltinPackets.RegisterPackets(this);
        }

     
        public Packet ConvertPacket(Socket socket, byte size, byte header, byte[] data)
        {
            return new Packet(socket,size,header,data);
        }

        public EventHandler<Packet> onReceivedPacketClient;
        public EventHandler<Packet> onReceivedPacketServer;

    }
}
