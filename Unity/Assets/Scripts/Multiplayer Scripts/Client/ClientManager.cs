using System;
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

    [Header("Objects To Spawn")]
    public GameObject prefabPlayer;
    public List<GameObject> objectsToSpawn;

    private InputField _ip;
    private InputField _password;
    private InputField _user;

    private GameObject _restartButton;
    private GameObject _waitingForOtherPlayer;
    private Text _countdownText;
    private Text _winnerName;

    Dictionary<PacketIDs, Action<PacketBase>> packetActions = new Dictionary<PacketIDs, Action<PacketBase>>();

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
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
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

    }

    public void SetupClient()
    {
        packetActions = new Dictionary<PacketIDs, Action<PacketBase>>();
        myClient = new NetworkClient();
        AddPacketActions();
        myClient.Connect(_ip.text, 8080);
    }

    private void OnConnectPlayer(NetworkMessage netMsg)
    {
        new PacketBase(PacketIDs.Server_CheckUser).ConnectionId(myClient.connection.connectionId).Add(_user.text).Add(_password.text).SendAsClient();
    }
    private void OnDisconnectPlayer(NetworkMessage netMsg)
    {
        myPlayers = new Dictionary<int, Player>();
        SceneManager.LoadScene("Login");
    }

    private void AddPacketActions()
    {
        myClient.RegisterHandler(MsgType.Connect, OnConnectPlayer);
        myClient.RegisterHandler(MsgType.Disconnect, OnDisconnectPlayer);

        //Packets Client
        packetActions.Add(PacketIDs.StartPlayer_Command, PacketExecutionClient.StartAPlayer_Command);
        packetActions.Add(PacketIDs.RefreshPlayers_Command, PacketExecutionClient.RefreshPlayers_Command);
        packetActions.Add(PacketIDs.UpdateAmmo_Command, PacketExecutionClient.UpdateAmmo_Comand);
        packetActions.Add(PacketIDs.ChangeWeapon_Command, PacketExecutionClient.ChangeWeapon_Command);
        packetActions.Add(PacketIDs.GameStart_Command, PacketExecutionClient.GameStart_Command);
        packetActions.Add(PacketIDs.GameEnded_Command, PacketExecutionClient.GameEnded_Command);
        packetActions.Add(PacketIDs.Restart_Command, PacketExecutionClient.Restart_Command);
        packetActions.Add(PacketIDs.FriendList_Command, PacketExecutionClient.Friend_List_Command);
        packetActions.Add(PacketIDs.WriteHighScore_Command, PacketExecutionClient.WriteHighScore_Command);
        packetActions.Add(PacketIDs.Conected_Command, PacketExecutionClient.Conected_Command);

        for (short i = 1000; i < 1000 + (short)PacketIDs.Count; i++)
        {
            myClient.RegisterHandler(i, OnPacketReceived);
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
        myPlayers=new Dictionary<int, Player>();

        var allPlayersOnScene = GameObject.FindObjectsOfType<Player>();

        foreach (var player in allPlayersOnScene)
        {
            if (!myPlayers.ContainsKey(player.connectionId))
            {
                myPlayers.Add(player.connectionId, player);
                player.OnServerStart(player.myname, player.connectionId, values, shootCoolDown);
                player.CanPlay(false);
                PlayerPos(player);
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

    public void GameEnded_Command(int playerId)
    {
        string winner = "";
        foreach (var player in myPlayers)
        {
            player.Value.CanPlay(false);

            if(player.Value.connectionId != playerId)
                winner = player.Value.myname;
        }

        _winnerName.enabled = true;
        _winnerName.text = "El ganador es: " + winner;

         _restartButton.SetActive(true);
    }

    public void RestartButton()
    {
        new PacketBase(PacketIDs.Server_RestartButton).SendAsClient();
        _restartButton.SetActive(false);
        _waitingForOtherPlayer.SetActive(true);
    }

    public void Restart_Command()
    {
        _waitingForOtherPlayer.SetActive(false);
        GameObject.Find("Winner").transform.Find("Name").GetComponent<Text>().enabled = false;

        foreach (var player in myPlayers)
        {
            PlayerPos(player.Value);
        }
    }

    //Data Base
    public void FriendList_Command(string[] friendList)
    {
        var profile = GameObject.FindObjectOfType<ProfileManager>();
        if (profile != null)
        {
            profile.WriteFriendList(friendList);
        }
    }
    public void HighScore_Command(string[] highScores)
    {
        var profile = GameObject.FindObjectOfType<ProfileManager>();
        if (profile != null)
        {
            profile.WriteHighScore(highScores);
        }
    }
    public void Conected_Command()
    {
        SceneManager.LoadScene("Menu");
    }


    public ClientManager SetupMenu()
    {
        new PacketBase(PacketIDs.Server_GetUserHighScore).ConnectionId(myClient.connection.connectionId).Add("").SendAsClient();
        new PacketBase(PacketIDs.Server_FriendList).ConnectionId(myClient.connection.connectionId).Add(_user.text).SendAsClient();
        return this;
    }

    public ClientManager SetupLogin(InputField user, InputField password, InputField ip)
    {
        _user = user;
        _password = password;
        _ip = ip;
        return this;
    }

    public ClientManager StupGame(GameObject waiting, GameObject restartButton, Text coundDown, Text winnerName)
    {
        _waitingForOtherPlayer = waiting;
        _restartButton = restartButton;
        _countdownText = coundDown;
        _winnerName = winnerName;

        new PacketBase(PacketIDs.Server_UserReadyToPlay).ConnectionId(myClient.connection.connectionId).Add(_user.text).SendAsClient();
        return this;
    }

    IEnumerator CountDown(int countStart)
    {
        _countdownText.enabled = true;
        _waitingForOtherPlayer.SetActive(false);

        var wait = new WaitForSeconds(1);
        var count = countStart;
        while (count > 0)
        {
            _countdownText.text = count.ToString();
            yield return wait;
            count--;
        }

        _countdownText.text = "Fight";
        yield return wait;
        _countdownText.enabled = false;
        foreach (var player in myPlayers)
        {
            player.Value.CanPlay(true);
        }
    }

}
