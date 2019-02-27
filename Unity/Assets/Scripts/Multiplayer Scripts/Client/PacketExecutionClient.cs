using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class PacketExecutionClient
{
    public static Action<PacketBase> StartAPlayer_Command = packetBase => ClientManager.instance.StartAPlayer_Command(packetBase.stringInfo[0], packetBase.intInfo, packetBase.floatInfo[0]);
    public static Action<PacketBase> RefreshPlayers_Command = packBase => ClientManager.instance.RefreshPlayers_Command(packBase.intInfo, packBase.floatInfo[0]);
    public static Action<PacketBase> UpdateAmmo_Comand = packBase => ClientManager.instance.UpdateAmmo_Command(packBase.typeInfo[0], packBase.intInfo[0], packBase.intInfo[1]);
    public static Action<PacketBase> ChangeWeapon_Command = packBase => ClientManager.instance.ChangeWeapon_Command(packBase.intInfo[0], packBase.intInfo[1]);
    public static Action<PacketBase> GameStart_Command = packBase => ClientManager.instance.GameStart_Command();
    public static Action<PacketBase> GameEnded_Command = packBase => ClientManager.instance.GameEnded_Command(packBase.connectionID);
    public static Action<PacketBase> Restart_Command = packBase => ClientManager.instance.Restart_Command();
    public static Action<PacketBase> DisconnectRestart_Command = packBase => ClientManager.instance.DisconnectRestart_Command();
    public static Action<PacketBase> Friend_List_Command = packBase => ClientManager.instance.FriendList_Command(packBase.stringInfo);
    public static Action<PacketBase> HighScore_Command = packBase => ClientManager.instance.HighScore_Command(packBase.stringInfo);
}
