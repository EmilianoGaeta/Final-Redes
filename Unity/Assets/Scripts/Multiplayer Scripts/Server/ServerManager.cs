using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ServerManager : MonoBehaviour
{
    public Dictionary<int, Player> myPlayers = new Dictionary<int, Player>();

    public static NetworkClient myClient;
    public static ServerManager instance;
    public int maxPlayers = 2;

    [Header("Objects To Spawn")]
    public GameObject prefabPlayer;
    public List<GameObject> objectsToSpawn;

    private GameObject _disconnect;


    private List<NetworkConnection> _connections = new List<NetworkConnection>();

    public void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    void Start()
    {
        UnityEngine.Application.runInBackground = true;
        ClientScene.RegisterPrefab(prefabPlayer);

        for (int i = 0; i < objectsToSpawn.Count; i++)
        {
            ClientScene.RegisterPrefab(objectsToSpawn[i]);
        }

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

    public void OnUserChecked(bool acepted)
    {
        if (!acepted)
        {
            _connections.RemoveAt(_connections.Count - 1);
            new PacketBase(PacketIDs.DisconnectRestart_Command).SendAsServer();
            return;
        }

        print("Se conecto alguien con el ID: " + _connections[_connections.Count - 1].connectionId);

        NetworkServer.connections[_connections[_connections.Count - 1].connectionId].isReady = true;

        foreach (var player in myPlayers)
        {
            NetworkServer.SpawnWithClientAuthority(player.Value.gameObject, NetworkServer.connections[player.Key]);
        }

        Player myPlayer = Instantiate(prefabPlayer).GetComponent<Player>();
        myPlayer.connectionId = _connections[_connections.Count - 1].connectionId;

        PlayerPos(myPlayer);

        myPlayers.Add(_connections[_connections.Count - 1].connectionId, myPlayer);

        NetworkServer.SpawnWithClientAuthority(myPlayer.gameObject, _connections[_connections.Count - 1]);

        //Para sincronizar jugadores
        new PacketBase(PacketIDs.RefreshPlayers_Command).Add(ServerLogic.instance.values).Add(ServerLogic.instance.shootCoolDown).SendAsServer();

        if (_connections.Count == 2)
            new PacketBase(PacketIDs.GameStart_Command).SendAsServer();
    }

    private void Ondisconnect(NetworkMessage netMsg)
    {
        if (myPlayers.ContainsKey(netMsg.conn.connectionId))
        {
            Debug.Log("The player " + myPlayers[netMsg.conn.connectionId].myname + " is disconnected");
            NetworkServer.Destroy(myPlayers[netMsg.conn.connectionId].gameObject);
            myPlayers.Remove(netMsg.conn.connectionId);
            if(_connections.Count == 2)
            {
                for (int i = 0; i < _connections.Count; i++)
                {
                    if (netMsg.conn.connectionId == _connections[i].connectionId)
                    {
                        _connections.RemoveAt(i);
                        new PacketBase(PacketIDs.DisconnectRestart_Command).SendAsServer();
                        break;
                    }
                }
            }
            else if(_connections.Count == 1)
            {
                _connections.Clear();
            }
        }

        var walls = GameObject.FindObjectsOfType<DestroyableObject>();
        foreach (var wall in walls)
            NetworkServer.Destroy(wall.gameObject);
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

        for (short i = 1000; i < 1000 + (short)PacketIDs.Count; i++)
        {
            NetworkServer.RegisterHandler(i, OnPacketReceived);
        }
    }

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

    Dictionary<PacketIDs, Action<PacketBase>> packetActions = new Dictionary<PacketIDs, Action<PacketBase>>();

    private void OnPacketReceived(NetworkMessage netMsg)
    {
        PacketBase msg = netMsg.ReadMessage<PacketBase>();
        msg.connectionID = netMsg.conn.connectionId;

        if (packetActions.ContainsKey((PacketIDs)msg.messageID))
            packetActions[(PacketIDs)msg.messageID](msg);
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

    public void UpdateAmmo_Command(TypeOfGun.myType type, int playerid, int less)
    {
        myPlayers[playerid].ammoAmount[type] -= less;
        myPlayers[playerid].UpdateAmmo();
    }

    public void ChangeWeapon_Command(int playerid, int weapon)
    {
        myPlayers[playerid].ChangeWeapon(weapon);
    }

    public void PlayerDammaged_Command(int playerId, int amount)
    {
        myPlayers[playerId].life = amount;
        myPlayers[playerId].Damaged();
    }

    public void GameEnded_Command(int playerId)
    {
        Text nameText = GameObject.Find("Winner").transform.Find("Name").GetComponent<Text>();
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
        GameObject.Find("Winner").transform.Find("Name").GetComponent<Text>().enabled = false;

        foreach (var player in myPlayers)
        {
            PlayerPos(player.Value);
        }
    }

    public void DisconnectRestart_Command()
    {
        myClient.Disconnect();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
