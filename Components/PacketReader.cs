using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace BardNetworking.Components
{
    internal class PacketReader
    {
        public PacketReader(bool registerDefaultPackets = true)
        {
            if (registerDefaultPackets)
            {
                BuiltinPackets.RegisterBuiltinPackets(this);
            }
        }
        public class Packet : EventArgs
        {
            public Socket sender;
            public byte header;
            public byte size;
            public byte[] data;
        }

        public Packet ConvertPacket(Socket socket, byte size, byte header, byte[] data)
        {
            return new Packet() { data = data, header = header, size = size, sender = socket };
        }

        public EventHandler<Packet> onReceivedPacket;

    }
}
