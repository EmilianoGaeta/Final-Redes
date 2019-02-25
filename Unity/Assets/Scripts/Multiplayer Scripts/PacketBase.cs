using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PacketBase : MessageBase
{
    public PacketBase(MultiplayerManager.PacketIDs ids)
    {
        messageID = (short)ids;
    }

    public PacketBase()
    {

    }

    public short messageID;
    public int connectionID;
    public Vector3[] vectorInfo;
    public float[] floatInfo;
    public NetworkInstanceId[] networkInfo;
    public string[] stringInfo;
    public bool[] boolInfo;
    public Color[] colorInfo;
    public int[] intInfo;
    public TypeOfGun.myType[] typeInfo;
}
