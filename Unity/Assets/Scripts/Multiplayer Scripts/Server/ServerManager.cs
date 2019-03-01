using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ServerManager : MonoBehaviour
{

    private int cantJug = 0;

    public Dictionary<int, Player> myPlayers = new Dictionary<int, Player>();

    public static NetworkClient myClient;
    public static ServerManager instance;
    public int maxPlayers = 2;

    [Header("Objects To Spawn")]
    public GameObject prefabPlayer;
    public List<GameObject> objectsToSpawn;

    private GameObject _disconnect;
    private GameObject _hostButton;

    Dictionary<PacketIDs, Action<PacketBase>> packetActions = new Dictionary<PacketIDs, Action<PacketBase>>();

    private List<NetworkConnection> _connections = new List<NetworkConnection>();

    public void Awake()
    {
        instance = this;
    }
    void Start()
    {
        UnityEngine.Application.runInBackground = true;
        ClientScene.RegisterPrefab(prefabPlayer);

        for (int i = 0; i < objectsToSpawn.Count; i++)
        {
            ClientScene.RegisterPrefab(objectsToSpawn[i]);
        }

        _hostButton = GameObject.Find("Host");

        _disconnect = GameObject.Find("Disconnect");
        _disconnect.SetActive(false);
    }


    public void Disconnect()
    {
        NetworkServer.Shutdown();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SetupServer()
    {
        packetActions = new Dictionary<PacketIDs, Action<PacketBase>>();
        AddPacketActions();

        ConnectionConfig config = new ConnectionConfig();
        config.AddChannel(QosType.ReliableSequenced);
        config.AddChannel(QosType.Unreliable);
        NetworkServer.Configure(config, maxPlayers);
        NetworkServer.Listen(8080);

        myClient = ClientScene.ConnectLocalServer();

        _disconnect.SetActive(true);
        _hostButton.SetActive(false);
    }

    private void OnConnect(NetworkMessage netMsg)
    {      
        if (NetworkServer.connections.Count == 1)
        {
            print("Server Initialized");
            return;
        }
        _connections.Add(netMsg.conn);
    }

    public void UserRejected(int connectionId)
    {
        for (int i = 0; i < _connections.Count; i++)
        {
            if (_connections[i].connectionId == connectionId)
            {

                _connections[i].Disconnect();
                break;
            }
        }
    }

    public void PlayerReadyToGame(int id, string user)
    {
        NetworkServer.connections[id].isReady = true;

        cantJug++;

        foreach (var player in myPlayers)
        {
            NetworkServer.SpawnWithClientAuthority(player.Value.gameObject, NetworkServer.connections[player.Key]);
        }

        Player myPlayer = Instantiate(prefabPlayer).GetComponent<Player>();
        myPlayer.myname = user;
        myPlayer.connectionId = id;

        PlayerPos(myPlayer);

        myPlayers.Add(id, myPlayer);

        NetworkServer.SpawnWithClientAuthority(myPlayer.gameObject, _connections.Where(x => x.connectionId == id).First());

        //Para sincronizar jugadores
        new PacketBase(PacketIDs.RefreshPlayers_Command).Add(ServerLogic.instance.values).Add(ServerLogic.instance.shootCoolDown).SendAsServer();

        if (cantJug == 2)
        {
            new PacketBase(PacketIDs.GameStart_Command).SendAsServer();
            ServerLogic.instance.endGame = false;
        }
    }

    private void Ondisconnect(NetworkMessage netMsg)
    {
        ServerLogic.instance.OnUserDisconected(netMsg.conn.connectionId);

        for (int i = 0; i < _connections.Count; i++)
        {
            if(_connections[i].connectionId == netMsg.conn.connectionId)
            {
                _connections.RemoveAt(i);

                if (myPlayers.ContainsKey(netMsg.conn.connectionId))
                {
                    cantJug--;
                    NetworkServer.Destroy(myPlayers[netMsg.conn.connectionId].gameObject);
                    myPlayers.Remove(netMsg.conn.connectionId);
                    foreach (var player in myPlayers)
                    {
                        PlayerPos(player.Value);
                        player.Value.OnServerStart(player.Value.myname, player.Value.connectionId, ServerLogic.instance.values, ServerLogic.instance.shootCoolDown);
                    }
                    new PacketBase(PacketIDs.RefreshPlayers_Command).Add(ServerLogic.instance.values).Add(ServerLogic.instance.shootCoolDown).SendAsServer();
                }
            }
        }
        var walls = GameObject.FindObjectsOfType<DestroyableObject>();
           foreach (var wall in walls)
               NetworkServer.Destroy(wall.gameObject);
        var bullets = GameObject.FindObjectsOfType<Bullet>();
        foreach (var bullet in bullets)
            NetworkServer.Destroy(bullet.gameObject);
        var grenades = GameObject.FindObjectsOfType<Grenade>();
        foreach (var grenade in grenades)
            NetworkServer.Destroy(grenade.gameObject);
    }

    private void AddPacketActions()
    {
        NetworkServer.RegisterHandler(MsgType.Connect, OnConnect);
        NetworkServer.RegisterHandler(MsgType.Disconnect, Ondisconnect);

        //Packets Server
        packetActions.Add(PacketIDs.Server_CheckUser, PacketExecutionServer.Server_CheckUser);
        packetActions.Add(PacketIDs.Server_StartPlayer, PacketExecutionServer.Server_StartPlayer);
        packetActions.Add(PacketIDs.Server_ShootCommand, PacketExecutionServer.Server_ShootCommand);
        packetActions.Add(PacketIDs.Server_ChangeWeapon, PacketExecutionServer.Server_ChangeWeapon);
        packetActions.Add(PacketIDs.Server_Move, PacketExecutionServer.Server_Move);
        packetActions.Add(PacketIDs.Server_RestartButton, PacketExecutionServer.Server_RestartButton);
        packetActions.Add(PacketIDs.Server_GetUserHighScore, PacketExecutionServer.Server_GetUserHighScores);
        packetActions.Add(PacketIDs.Server_GetHighScores, PacketExecutionServer.Server_GetHighScores);
        packetActions.Add(PacketIDs.Server_FriendList, PacketExecutionServer.Server_FriendList);
        packetActions.Add(PacketIDs.Server_UserReadyToPlay, PacketExecutionServer.Server_UserReadyToPlay);
        packetActions.Add(PacketIDs.Server_ADD_FRIEND, PacketExecutionServer.Server_Add_Friend);
        packetActions.Add(PacketIDs.Server_DELETE_FRIEND, PacketExecutionServer.Server_Delete_Friend);
        packetActions.Add(PacketIDs.Server_ACCEPTREJECT_FRIENDSHIP, PacketExecutionServer.Server_AcceptReject_Friendship);

        for (short i = 1000; i < 1000 + (short)PacketIDs.Count; i++)
        {
            NetworkServer.RegisterHandler(i, OnPacketReceived);
        }
    }

    private void OnPacketReceived(NetworkMessage netMsg)
    {
        PacketBase msg = netMsg.ReadMessage<PacketBase>();
        msg.connectionID = netMsg.conn.connectionId;

        if (packetActions.ContainsKey((PacketIDs)msg.messageID))
            packetActions[(PacketIDs)msg.messageID](msg);
    }


    //Player
    private void PlayerPos(Player myPlayer)
    {
        if (myPlayer.connectionId == 1)
        {
            myPlayer.transform.position = new Vector3(-8, 0, 0);
            myPlayer.transform.eulerAngles = Vector3.zero;

        }
        else if (myPlayer.connectionId == 2)
        {
            myPlayer.transform.position = new Vector3(8, 0, 0);
            myPlayer.transform.eulerAngles = new Vector3(0, 0, 180);
        }
    }

    public void StartAPlayer_Command(string name, int[] values, float shootCoolDown)
    {
        List<int> v = new List<int>(values);
        v.RemoveAt(0);
        myPlayers[values[0]].OnServerStart(name, values[0], v.ToArray(), shootCoolDown);
        myPlayers[values[0]].CanPlay(false);
    }

    public void RefreshPlayers_Command(int[] values, float shootCoolDown)
    {
        var allPlayersOnScene = GameObject.FindObjectsOfType<Player>();

        foreach (var player in allPlayersOnScene)
        {
            if (!myPlayers.ContainsKey(player.connectionId))
            {
                myPlayers.Add(player.connectionId, player);
                player.OnServerStart(player.myname, player.connectionId, values, shootCoolDown);
            }
        }
    }

    public void GameEnded_Command(int playerId)
    {
        Text nameText = GameObject.Find("Winner").transform.Find("WinnerName").GetComponent<Text>();
        string winner = "";
        foreach (var player in myPlayers)
        {
            player.Value.CanPlay(false);

            if(player.Value.connectionId != playerId)
                winner = player.Value.myname;
        }

        nameText.enabled = true;
        nameText.text = "El ganador es: " + winner;
    }

    public void Restart_Command()
    {
        GameObject.Find("Winner").transform.Find("WinnerName").GetComponent<Text>().enabled = false;

        foreach (var player in myPlayers)
        {
            PlayerPos(player.Value);
        }
    }

}
