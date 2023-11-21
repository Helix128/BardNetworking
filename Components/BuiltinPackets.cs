using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Packet = BardNetworking.Components.PacketReader.Packet;
namespace BardNetworking.Components
{
    internal class BuiltinPackets
    {

        public static readonly byte TEXT = 253;

        public static readonly byte DISCONNECT = 254;

        public static void RegisterBuiltinPackets(PacketReader reader)
        {
            reader.onReceivedPacket += ProcessDisconnectPacket;
            reader.onReceivedPacket += ProcessTextPacket;
        }
        static void ProcessDisconnectPacket(object x, Packet packet)
        {
            if (packet.header == DISCONNECT)
            {
                packet.sender.Close();
            }
        }
        static void ProcessTextPacket(object x, Packet packet)
        {
            if (packet.header == TEXT)
            {
                Console.WriteLine(Encoding.UTF8.GetString(packet.data.ToArray()));
            }
        }

    }
}
