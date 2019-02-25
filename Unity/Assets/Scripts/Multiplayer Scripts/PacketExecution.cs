using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class PacketExecution
{
    //To Server
    public static Action<PacketBase> Server_CheckUser = packetBase => ServerLogic.instance.Server_CheckUser(packetBase.stringInfo[0], packetBase.stringInfo[1]);
    public static Action<PacketBase> Server_StartPlayer = packetBase => ServerLogic.instance.Server_StartPlayer(packetBase.stringInfo[0], packetBase.intInfo[0]);
    public static Action<PacketBase> Server_ShootCommand = packBase => ServerLogic.instance.Server_ShootCommand(packBase.typeInfo[0], packBase.intInfo[0]);
    public static Action<PacketBase> Server_ChangeWeapon= packBase => ServerLogic.instance.Server_ChangeWeapon(packBase.intInfo[0], packBase.intInfo[1]);
    public static Action<PacketBase> Server_Move = packBase => ServerLogic.instance.ServerMove(packBase.connectionID, packBase.floatInfo[0], packBase.floatInfo[1], packBase.vectorInfo[0]);
    public static Action<PacketBase> Server_RestartButton = packBase => ServerLogic.instance.Server_RestartButton();

    //To Clients
    public static Action<PacketBase> StartAPlayer_Command = packetBase => MultiplayerManager.instance.StartAPlayer_Command(packetBase.stringInfo[0], packetBase.intInfo, packetBase.floatInfo[0]);
    public static Action<PacketBase> RefreshPlayers_Command = packBase => MultiplayerManager.instance.RefreshPlayers_Command(packBase.intInfo, packBase.floatInfo[0]);
    public static Action<PacketBase> UpdateAmmo_Comand = packBase => MultiplayerManager.instance.UpdateAmmo_Command(packBase.typeInfo[0], packBase.intInfo[0], packBase.intInfo[1]);
    public static Action<PacketBase> ChangeWeapon_Command = packBase => MultiplayerManager.instance.ChangeWeapon_Command(packBase.intInfo[0], packBase.intInfo[1]);
    public static Action<PacketBase> GameStart_Command = packBase => MultiplayerManager.instance.GameStart_Command();
    public static Action<PacketBase> Damaged_Command = packetBase => MultiplayerManager.instance.PlayerDammaged_Command(packetBase.intInfo[0], packetBase.intInfo[1]);
    public static Action<PacketBase> GameEnded_Command = packBase => MultiplayerManager.instance.GameEnded_Command(packBase.connectionID);
    public static Action<PacketBase> Restart_Command = packBase => MultiplayerManager.instance.Restart_Command();
    public static Action<PacketBase> DisconnectRestart_Command = packBase => MultiplayerManager.instance.DisconnectRestart_Command();
}
