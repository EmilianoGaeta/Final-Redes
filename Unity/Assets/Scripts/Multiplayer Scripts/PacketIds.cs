public enum PacketIDs : short
{
    //To Server
    Server_CheckUser,
    Server_StartPlayer,
    Server_ShootCommand,
    Server_ChangeWeapon,
    Server_Move,
    Server_RestartButton,
    //To Client
    StartPlayer_Command,
    UpdateAmmo_Command,
    ChangeWeapon_Command,
    GameStart_Command,
    RefreshPlayers_Command,
    Damaged_Command,
    GameEnded_Command,
    Restart_Command,
    DisconnectRestart_Command,
    Count
}