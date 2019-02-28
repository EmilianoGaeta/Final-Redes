public enum PacketIDs : short
{
    //To Server
    Server_CheckUser,
    Server_StartPlayer,
    Server_ShootCommand,
    Server_ChangeWeapon,
    Server_Move,
    Server_RestartButton,
    Server_GetUserHighScore,
    Server_GetHighScores,
    Server_FriendList,
    Server_UserReadyToPlay,
    //To Client
    StartPlayer_Command,
    UpdateAmmo_Command,
    ChangeWeapon_Command,
    GameStart_Command,
    RefreshPlayers_Command,
    GameEnded_Command,
    Restart_Command,
    WriteHighScore_Command,
    FriendList_Command,
    Conected_Command,
    Count
}