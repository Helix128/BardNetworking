using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BardNetworking.Components
{
    public class Packet : EventArgs
    {
        public Socket sender;
        public byte header;
        List<byte> dataList;
        public byte[] data;

        int readPos = 0;
        public Packet(Socket sender, byte[] rawData)
        {
            this.sender = sender;
            this.header = rawData[1];
            this.data = rawData.Skip(2).ToArray();
            dataList = data.ToList();
            readPos = 0;
        }
        public Packet(Socket sender, byte header)
        {
            this.sender = sender;
            this.header = header;
            dataList = new List<byte>();
            this.data = new byte[BardSettings.MAX_PACKET_SIZE];
            readPos = 0;
        }
        public byte[] GetBytes()
        {
            if (dataList.Count > 0)
            {
                data = dataList.ToArray();
            }
            byte[] bytes = data;
            bytes = bytes.Prepend(header).ToArray();
            bytes = bytes.Prepend((byte)(data.Length+2)).ToArray();
            return bytes;
        }
        public void Write(byte x)
        {
           dataList.Add(x);
        }
        public void Write(int x)
        {   
            byte[] bytes = BitConverter.GetBytes(x);
            for (int i = 0; i < bytes.Length; i++)
            { 
                dataList.Add(bytes[i]);
            }
        }
        public void Write(float x)
        {
            byte[] bytes = BitConverter.GetBytes(x);
            for (int i = 0; i < bytes.Length; i++)
            {
                dataList.Add(bytes[i]);
            }
       
        }
        public void Write<T>(object value)
        {
            int rawsize = Marshal.SizeOf(value);
            byte[] rawdata = new byte[rawsize];
            GCHandle handle =
                GCHandle.Alloc(rawdata,
                GCHandleType.Pinned);
            Marshal.StructureToPtr(value,
                handle.AddrOfPinnedObject(),
                false);
            handle.Free();

                byte[] temp = new byte[BardSettings.MAX_PACKET_SIZE];
                Array.Copy(rawdata, temp, temp.Length);
            dataList.AddRange(temp);
            
        }
        public T Read<T>()
        {
            GCHandle handle = GCHandle.Alloc(data.Skip(readPos).Take(Marshal.SizeOf<T>()), GCHandleType.Pinned);
            T structure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return structure;
        }
        public byte ReadByte()
        {
            if (readPos > data.Length - 1)
            {
                return byte.MaxValue;
            }
            readPos++;
            return data[readPos-1];
        }
        public int ReadInt()
        {
            if (readPos > data.Length - 1)
            {
                return int.MaxValue;
            }
            int value = BitConverter.ToInt32(data.Skip(readPos).Take(sizeof(int)).ToArray());
            readPos += sizeof(int);
            return value;
        }
        public float ReadFloat()
        {
            if (readPos > data.Length-1)
            {
                return float.MaxValue;
            }
            float value = BitConverter.ToSingle(data, readPos);
            readPos += sizeof(float);
            return value;
        }
    }

    internal class PacketReader
    {   
        public PacketReader()
        {
         BuiltinPackets.RegisterPackets(this);
        }

     
        public Packet ConvertPacket(Socket socket, byte[] rawData)
        {
            return new Packet(socket,rawData);
        }

        public EventHandler<Packet> onReceivedPacketClient;
        public EventHandler<Packet> onReceivedPacketServer;

    }
}
