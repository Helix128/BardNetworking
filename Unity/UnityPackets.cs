using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BardNetworking;
using BardNetworking.Components;
using BardNetworking.Unity;
class UnityPackets
{
    static PacketType TRANSFORM_PACKET; //vector3, vector3

    static PacketType SPAWN_PACKET; //int, vector3
    static PacketType DESPAWN_PACKET;

    public static void RegisterPackets(PacketReader reader)
    {
        reader.onReceivedPacketServer+=ProcessUnityPacketsServer;
        reader.onReceivedPacketClient+=ProcessUnityPacketsClient;
    }
    public static void ProcessUnityPacketsServer(object server, Packet packet)
    {
        if (packet.header == SPAWN_PACKET)
        {
            var bardServer = (BardServer)server;
            bardServer.SendToAll(packet);
        }
    }
    public static void ProcessUnityPacketsClient(object client, Packet packet)
    {
        if (packet.header == SPAWN_PACKET)
        {
            int owner = packet.ReadInt();
            int objectId = packet.ReadInt();
            Vector3 position = packet.Read<Vector3>();
            BardNetwork.instance.NetworkSpawn(owner, objectId, position, Quaternion.identity);
        }
    }
}
