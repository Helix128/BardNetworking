using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BardNetworking
{
    internal class BardSettings
    {
        //254 bytes for data, 2 bytes for packet size + header
        //Must be less than or equal to 255 due to space constraints

        public const int MAX_PACKET_SIZE = 255;

        public const bool DEBUG = true;

        public const ProtocolType PROTOCOL_TYPE = ProtocolType.Tcp;
    }
}
