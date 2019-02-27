public enum PacketIDs : short
{
    //To Server
    Server_CheckUser,
    Server_StartPlayer,
    Server_ShootCommand,
    Server_ChangeWeapon,
    Server_Move,
    Server_RestartButton,
    GetUserHighScore_Command,
    GetHighScores_Command,
    //To Client
    StartPlayer_Command,
    UpdateAmmo_Command,
    ChangeWeapon_Command,
    GameStart_Command,
    RefreshPlayers_Command,
    GameEnded_Command,
    Restart_Command,
    DisconnectRestart_Command,
    FriendList_Command,
    WriteHighScore_Command,
    Count
}