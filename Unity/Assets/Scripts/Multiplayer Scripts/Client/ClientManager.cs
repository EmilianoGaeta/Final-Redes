﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ClientManager : MonoBehaviour
{
    public Dictionary<int, Player> myPlayers = new Dictionary<int, Player>();

    public static NetworkClient myClient;
    public static ClientManager instance;
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

        waitingForOtherPlayer.SetActive(false);
        _disconnect = GameObject.Find("Disconnect");
        _networkUI = GameObject.Find("NetWork UI");

        if(SceneManager.GetActiveScene().buildIndex == SceneManager.GetSceneByName("Login").buildIndex)
        {
            Transform userypass = GameObject.Find("User y Pass").transform;
            _user = userypass.Find("User").GetComponent<InputField>();
            _password = userypass.Find("Pass").GetComponent<InputField>();

            _ip = _networkUI.transform.Find("IP").GetComponent<InputField>();
        }

        _disconnect.SetActive(false);

    }


    public void Disconnect()
    {
        myClient.Disconnect();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SetupClient()
    {
        packetActions = new Dictionary<PacketIDs, Action<PacketBase>>();
        myClient = new NetworkClient();
        AddPacketActions();
        myClient.Connect(_ip.text, 8080);
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


    private void OnConnectPlayer(NetworkMessage netMsg)
    {
        SceneManager.LoadScene(SceneManager.GetSceneByName("Game").buildIndex);

        new PacketBase(PacketIDs.Server_CheckUser).Add(_user.text).Add(_password.text).SendAsClient();

        //UI
        _disconnect.SetActive(true);
        GameObject.Find("User y Pass").SetActive(false);
        waitingForOtherPlayer.SetActive(true);
    }


    private void AddPacketActions()
    {
        myClient.RegisterHandler(MsgType.Connect, OnConnectPlayer);

        //Packets Client
        packetActions.Add(PacketIDs.StartPlayer_Command, PacketExecutionClient.StartAPlayer_Command);
        packetActions.Add(PacketIDs.RefreshPlayers_Command, PacketExecutionClient.RefreshPlayers_Command);
        packetActions.Add(PacketIDs.UpdateAmmo_Command, PacketExecutionClient.UpdateAmmo_Comand);
        packetActions.Add(PacketIDs.ChangeWeapon_Command, PacketExecutionClient.ChangeWeapon_Command);
        packetActions.Add(PacketIDs.GameStart_Command, PacketExecutionClient.GameStart_Command);
        packetActions.Add(PacketIDs.Damaged_Command, PacketExecutionClient.Damaged_Command);
        packetActions.Add(PacketIDs.GameEnded_Command, PacketExecutionClient.GameEnded_Command);
        packetActions.Add(PacketIDs.Restart_Command, PacketExecutionClient.Restart_Command);
        packetActions.Add(PacketIDs.DisconnectRestart_Command, PacketExecutionClient.DisconnectRestart_Command);

        for (short i = 1000; i < 1000 + (short)PacketIDs.Count; i++)
        {
            myClient.RegisterHandler(i, OnPacketReceived);
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

         restartButton.SetActive(true);
    }

    public void RestartButton()
    {
        new PacketBase(PacketIDs.Server_RestartButton).SendAsClient();
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