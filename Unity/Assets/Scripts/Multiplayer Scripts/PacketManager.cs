using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
public static class PacketManager
{
    public static PacketBase Add<T>(this PacketBase packet, T info)
    {
        if (info.GetType().Equals(typeof(string)))
            return packet.AddString((string)(object)info);
        else if (info.GetType().Equals(typeof(float)))
            return packet.AddFloat((float)(object)info);
        else if (info.GetType().Equals(typeof(Vector3)))
            return packet.AddVector((Vector3)(object)info);
        else if (info.GetType().Equals(typeof(NetworkInstanceId)))
            return packet.AddNetwork((NetworkInstanceId)(object)info);
        else if (info.GetType().Equals(typeof(bool)))
            return packet.AddBool((bool)(object)info);
        else if (info.GetType().Equals(typeof(Color)))
            return packet.AddColor((Color)(object)info);
        else if (info.GetType().Equals(typeof(int)))
            return packet.AddInt((int)(object)info);
        else if (info.GetType().Equals(typeof(int[])))
            return packet.AddArrayInt((int[])(object)info);
        else if (info.GetType().Equals(typeof(TypeOfGun.myType)))
            return packet.AddType((TypeOfGun.myType)(object)info);
        else return packet;
    }
    static PacketBase AddInt(this PacketBase packet, int info)
    {
        var list = packet.intInfo != null ? packet.intInfo.ToList() : new List<int>();
        list.Add(info);
        packet.intInfo = list.ToArray();
        return packet;
    }
    static PacketBase AddArrayInt(this PacketBase packet, int[] info)
    {
        var list = packet.intInfo != null ? packet.intInfo.ToList() : new List<int>();
        list.AddRange(info);
        packet.intInfo = list.ToArray();
        return packet;
    }


    static PacketBase AddType(this PacketBase packet, TypeOfGun.myType info)
    {
        var list = packet.typeInfo != null ? packet.typeInfo.ToList() : new List<TypeOfGun.myType>();
        list.Add(info);
        packet.typeInfo = list.ToArray();
        return packet;
    }

    static PacketBase AddColor(this PacketBase packet, Color info)
    {
        var list = packet.colorInfo != null ? packet.colorInfo.ToList() : new List<Color>();
        list.Add(info);
        packet.colorInfo = list.ToArray();
        return packet;
    }

    static PacketBase AddFloat(this PacketBase packet, float info)
    {
        var list = packet.floatInfo != null ? packet.floatInfo.ToList() : new List<float>();
        list.Add(info);
        packet.floatInfo = list.ToArray();
        return packet;
    }

    static PacketBase AddVector(this PacketBase packet, Vector3 info)
    {
        var list = packet.vectorInfo != null ? packet.vectorInfo.ToList() : new List<Vector3>();
        list.Add(info);
        packet.vectorInfo = list.ToArray();
        return packet;
    }

    static PacketBase AddNetwork(this PacketBase packet, NetworkInstanceId info)
    {
        var list = packet.networkInfo != null ? packet.networkInfo.ToList() : new List<NetworkInstanceId>();
        list.Add(info);
        packet.networkInfo = list.ToArray();
        return packet;
    }

    static PacketBase AddString(this PacketBase packet, string info)
    {
        var list = packet.stringInfo != null ? packet.stringInfo.ToList() : new List<string>();
        list.Add(info);
        packet.stringInfo = list.ToArray();
        return packet;
    }

    static PacketBase AddBool(this PacketBase packet, bool info)
    {
        var list = packet.boolInfo != null ? packet.boolInfo.ToList() : new List<bool>();
        list.Add(info);
        packet.boolInfo = list.ToArray();
        return packet;
    }

    public static void SendAsServer(this PacketBase packet, int connId = -1, bool reliable = true)
    {
        short packetID = 1000;
        packetID += packet.messageID;

        if (connId != -1)
            NetworkServer.SendToClient(connId, packetID, packet);
        else
            if (reliable)
            NetworkServer.SendToAll(packetID, packet);
        else
            NetworkServer.SendUnreliableToAll(packetID, packet);
    }

    public static void SendAsClient(this PacketBase packet, bool reliable = true)
    {
        short packetID = 1000;
        packetID += packet.messageID;

        if (reliable)
            MultiplayerManager.myClient.Send(packetID, packet);
        else
            MultiplayerManager.myClient.SendUnreliable(packetID, packet);
    }

    public static void Send(this PacketBase packet, bool reliable = true, int connId = -1)
    {
        short packetID = 1000;
        packetID += packet.messageID;

        if (MultiplayerManager.instance.connectionType == MultiplayerManager.ConnectionType.Server)
        {
            if (connId != -1)
                NetworkServer.SendToClient(connId, packetID, packet);
            else
                if (reliable)
                NetworkServer.SendToAll(packetID, packet);
            else
                NetworkServer.SendUnreliableToAll(packetID, packet);
        }
        else if (MultiplayerManager.instance.connectionType == MultiplayerManager.ConnectionType.Client)
            if (reliable)
                MultiplayerManager.myClient.Send(packetID, packet);
            else
                MultiplayerManager.myClient.SendUnreliable(packetID, packet);
    }
}
