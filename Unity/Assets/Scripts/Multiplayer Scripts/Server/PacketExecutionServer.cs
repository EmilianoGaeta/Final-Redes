using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class PacketExecutionServer
{
    public static Action<PacketBase> Server_CheckUser = packetBase => ServerLogic.instance.Server_CheckUser(packetBase.connectionID,packetBase.stringInfo[0], packetBase.stringInfo[1]);
    public static Action<PacketBase> Server_StartPlayer = packetBase => ServerLogic.instance.Server_StartPlayer(packetBase.stringInfo[0], packetBase.intInfo[0]);
    public static Action<PacketBase> Server_ShootCommand = packBase => ServerLogic.instance.Server_ShootCommand(packBase.typeInfo[0], packBase.intInfo[0]);
    public static Action<PacketBase> Server_ChangeWeapon= packBase => ServerLogic.instance.Server_ChangeWeapon(packBase.intInfo[0], packBase.intInfo[1]);
    public static Action<PacketBase> Server_Move = packBase => ServerLogic.instance.ServerMove(packBase.connectionID, packBase.floatInfo[0], packBase.floatInfo[1], packBase.vectorInfo[0]);
    public static Action<PacketBase> Server_RestartButton = packBase => ServerLogic.instance.Server_RestartButton();
    public static Action<PacketBase> GetUserHighScore_Command = packBase => ServerLogic.instance.Server_GetUserHighScorer(packBase.connectionID, packBase.stringInfo[0]);
    public static Action<PacketBase> GetHighScores_Command = packBase => ServerLogic.instance.Server_GetHighScores_Command(packBase.connectionID);
    public static Action<PacketBase> FriendList_Command = packBase => ServerLogic.instance.Server_FriendList(packBase.connectionID, packBase.stringInfo[0]);
    public static Action<PacketBase> UserReadyToPlay_Command = packBase => ServerLogic.instance.Server_UserReadyToPlay_Command(packBase.connectionID,packBase.stringInfo[0]);

}
