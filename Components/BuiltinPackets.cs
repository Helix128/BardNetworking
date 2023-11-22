using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;



namespace BardNetworking.Components
{
    public class PacketType
    {
        public byte id;

        public PacketType(byte id)
        {
            this.id = id;
        }
        public static implicit operator byte(PacketType packetType) => packetType.id;
        public static implicit operator PacketType(byte id) => new PacketType(id);
    }
    internal class BuiltinPackets
    {
        
        //Logic
        public static PacketType ASSIGN_ID;
        public static PacketType DISCONNECT;

        //Data
        public static PacketType TEXT;

        public static void RegisterPackets(PacketReader reader)
        {
            BindingFlags bindingFlags = BindingFlags.Public |
                            BindingFlags.NonPublic |
                            BindingFlags.Static;
            int index = 0;
            List<FieldInfo> fields = new List<FieldInfo>();
             foreach(Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                FieldInfo[] insideFields = (t.GetFields(bindingFlags));
                for (int i = 0; i < insideFields.Length; i++)
                {
                    if (insideFields[i].FieldType == typeof(PacketType))
                    {
                        Debug.Log("Registering packet " + insideFields[i].Name + "...",LogSource.Server);
                        fields.Add(insideFields[i]);
                    }
                }
            }
            foreach (FieldInfo field in fields)
            {
                field.SetValue(null, new PacketType((byte)index));
                index++;
            }       
            Debug.Log("Registered " + index + " packets.", LogSource.Server, LogType.Info);
            reader.onReceivedPacketServer += ProcessDisconnectPacket;
            reader.onReceivedPacketServer += ProcessTextPacket;
            reader.onReceivedPacketClient += ProcessTextPacket;
            reader.onReceivedPacketClient += ProcessIDPacket;
        }
        static void ProcessDisconnectPacket(object x, Packet packet)
        {
            if (packet.header == DISCONNECT)
            {
                Debug.Log("Disconnecting client...", LogSource.Server);
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
        static void ProcessIDPacket(object x, Packet packet)
        {
            if(packet.header == ASSIGN_ID)
            {
                BardClient client = (BardClient)x;
                client.clientId = BitConverter.ToInt32(packet.data);
                Debug.Log("Assigned client ID (" + client.clientId + ")", LogSource.Client);
            }
        }
      
    }
}
