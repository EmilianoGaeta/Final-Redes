using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;

public class MultiplayerManager : MonoBehaviour
{
    public Dictionary<int, Player> myPlayers = new Dictionary<int, Player>();

    public ConnectionType connectionType;
    public static NetworkClient myClient;
    public static MultiplayerManager instance;
    public int maxPlayers = 2;
    public Text countdownText;
    public GameObject restartButton;
    public GameObject waitingForOtherPlayer;
    [Header("Objects To Spawn")]
    public GameObject prefabPlayer;
    public List<GameObject> objectsToSpawn;

    private GameObject _networkUI;
    private GameObject _disconnect;
    private InputField _ip;

    private InputField _password;
    private InputField _user;

    private List<NetworkConnection> _connections = new List<NetworkConnection>();

    public enum ConnectionType
    {
        None,
        Server,
        Client,
        Both
    }

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

        waitingForOtherPlayer.SetActive(false);
        _disconnect = GameObject.Find("Disconnect");
        _networkUI = GameObject.Find("NetWork UI");

        Transform userypass = GameObject.Find("User y Pass").transform;
        _user = userypass.Find("User").GetComponent<InputField>();
        _password = userypass.Find("Pass").GetComponent<InputField>();

        _ip = _networkUI.transform.Find("IP").GetComponent<InputField>();


        _disconnect.SetActive(false);

    }

    public void Disconnect()
    {
        if (connectionType == ConnectionType.Server)
        {
            NetworkServer.Shutdown();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else if(connectionType == ConnectionType.Client)
        {
            myClient.Disconnect();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void SetupServer()
    {
        connectionType = ConnectionType.Server;

        packetActions = new Dictionary<PacketIDs, Action<PacketBase>>();
        AddPacketActions();

        ConnectionConfig config = new ConnectionConfig();
        config.AddChannel(QosType.ReliableSequenced);
        config.AddChannel(QosType.Unreliable);
        NetworkServer.Configure(config, maxPlayers);
        NetworkServer.Listen(8080);

        myClient = ClientScene.ConnectLocalServer();

        _networkUI.SetActive(false);
        _disconnect.SetActive(true);
    }

    public void SetupClient()
    {
        connectionType = ConnectionType.Client;
        packetActions = new Dictionary<PacketIDs, Action<PacketBase>>();
        myClient = new NetworkClient();
        AddPacketActions();
        myClient.Connect(_ip.text, 8080);
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
            new PacketBase(MultiplayerManager.PacketIDs.DisconnectRestart_Command).SendAsServer();
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
        new PacketBase(MultiplayerManager.PacketIDs.RefreshPlayers_Command).Add(ServerLogic.instance.values).Add(ServerLogic.instance.shootCoolDown)
            .SendAsServer();

        if (_connections.Count == 2)
        {
            new PacketBase(MultiplayerManager.PacketIDs.GameStart_Command).SendAsServer();
            StartCoroutine(CountDown(3));
        }
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
                        new PacketBase(MultiplayerManager.PacketIDs.DisconnectRestart_Command).SendAsServer();
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


    private void OnConnectPlayer(NetworkMessage netMsg)
    {
        new PacketBase(MultiplayerManager.PacketIDs.Server_CheckUser).Add(_user.text).Add(_password.text).SendAsClient();

        //UI
        _disconnect.SetActive(true);
        GameObject.Find("User y Pass").SetActive(false);
        waitingForOtherPlayer.SetActive(true);
    }


    private void AddPacketActions()
    {
        if (connectionType == ConnectionType.Server)
        {
            NetworkServer.RegisterHandler(MsgType.Connect, OnConnect);
            NetworkServer.RegisterHandler(MsgType.Disconnect, Ondisconnect);

            //Packets Server
            packetActions.Add(PacketIDs.Server_CheckUser, PacketExecution.Server_CheckUser);
            packetActions.Add(PacketIDs.Server_StartPlayer, PacketExecution.Server_StartPlayer);
            packetActions.Add(PacketIDs.Server_ShootCommand, PacketExecution.Server_ShootCommand);
            packetActions.Add(PacketIDs.Server_ChangeWeapon, PacketExecution.Server_ChangeWeapon);
            packetActions.Add(PacketIDs.Server_Move, PacketExecution.Server_Move);
            packetActions.Add(PacketIDs.Server_RestartButton, PacketExecution.Server_RestartButton);
        }
        else if(connectionType == ConnectionType.Client)
        {
            myClient.RegisterHandler(MsgType.Connect, OnConnectPlayer);

            //Packets Client
            packetActions.Add(PacketIDs.StartPlayer_Command, PacketExecution.StartAPlayer_Command);
            packetActions.Add(PacketIDs.RefreshPlayers_Command, PacketExecution.RefreshPlayers_Command);
            packetActions.Add(PacketIDs.UpdateAmmo_Command, PacketExecution.UpdateAmmo_Comand);
            packetActions.Add(PacketIDs.ChangeWeapon_Command, PacketExecution.ChangeWeapon_Command);
            packetActions.Add(PacketIDs.GameStart_Command, PacketExecution.GameStart_Command);
            packetActions.Add(PacketIDs.Damaged_Command, PacketExecution.Damaged_Command);
            packetActions.Add(PacketIDs.GameEnded_Command, PacketExecution.GameEnded_Command);
            packetActions.Add(PacketIDs.Restart_Command, PacketExecution.Restart_Command);
            packetActions.Add(PacketIDs.DisconnectRestart_Command, PacketExecution.DisconnectRestart_Command);
        }

        for (short i = 1000; i < 1000 + (short)PacketIDs.Count; i++)
        {
            if (connectionType == ConnectionType.Server)
            {
                NetworkServer.RegisterHandler(i, OnPacketReceived);
            }
            else if (connectionType == ConnectionType.Client)
            {
                myClient.RegisterHandler(i, OnPacketReceived);
            }
            else
            {
                NetworkServer.RegisterHandler(i, OnPacketReceived);
                myClient.RegisterHandler(i, OnPacketReceived);
            }
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

    public void GameStart_Command()
    {
        StartCoroutine(CountDown(3));
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

        if (connectionType == ConnectionType.Client) restartButton.SetActive(true);
    }

    public void RestartButton()
    {
        new PacketBase(MultiplayerManager.PacketIDs.Server_RestartButton).SendAsClient();
        restartButton.SetActive(false);
        waitingForOtherPlayer.SetActive(true);
    }

    public void Restart_Command()
    {
        waitingForOtherPlayer.SetActive(false);
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

    IEnumerator CountDown(int countStart)
    {
        countdownText.enabled = true;
        waitingForOtherPlayer.SetActive(false);

        var wait = new WaitForSeconds(1);
        var count = countStart;
        while (count > 0)
        {
             countdownText.text = count.ToString();
            yield return wait;
            count--;
        }

        countdownText.text = "Fight";
        yield return wait;
        countdownText.enabled = false;
        foreach (var player in myPlayers)
        {
            player.Value.CanPlay(true);
        }
    }
    
}
