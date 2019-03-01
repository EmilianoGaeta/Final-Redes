using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerLogic : MonoBehaviour
{
    public static ServerLogic instance;

    [Header("Player Variables")]
    public int life;
    public int speed;
    public int rifleAmountStart;
    public int grenadeAmountStart;
    public int boxAmountStart;
    public int largeBoxAmountStart;
    public float shootCoolDown;

    public int[] values;

    [Header("Bullet")]
    public float bulletSpeed;
    public int bulletDamage;
    [Header("Boxes")]
    public int lifeSmallBox;
    public int lifeLargeBox;
    [Header("Grenade")]
    public float grenadeSpeed;
    public int grenadeLife;
    public int grenadeDamage;

    private int _restartCount;

    private GameObject _mysqlLogic;

    private Dictionary<int, string> _users = new Dictionary<int, string>();

    public bool endGame = false;

    void Awake()
    {
        instance = this;
        values = new int[] { life, speed, rifleAmountStart, grenadeAmountStart, boxAmountStart, largeBoxAmountStart};
        _mysqlLogic = GameObject.Find("MysqlLogic");
    }
    void Start()
    {
        _restartCount = ServerManager.instance.maxPlayers;
    }

    //Check user
    public void Server_CheckUser(int connectionId,string user, string pass, bool register)
    {
        if (register)
        {
            if (_mysqlLogic.GetComponent<LoginAndRegister>().Register(user, pass))
            {
                new PacketBase(PacketIDs.Conected_Command).SendAsServer(connectionId);
                _mysqlLogic.GetComponent<UserCommandsFunc>().SetConnectionState(user, "C", connectionId);
                _users[connectionId] = user;

                var connectedFriends = _mysqlLogic.GetComponent<UserCommandsFunc>().GetUserFriends(user).Item1.Select(x => _mysqlLogic.GetComponent<UserCommandsFunc>().GetUserConnectionStateAndID(x))
                                                                                                              .Where(y => y.Item2.ToLower() == "c");
                foreach (var friend in connectedFriends)
                {
                    var friendList = _mysqlLogic.GetComponent<UserCommandsFunc>().GetUserFriends(friend.Item1);
                    new PacketBase(PacketIDs.FriendList_Command).Add(friendList.Item1).Add(friendList.Item2).Add(friendList.Item3).SendAsServer(friend.Item3);
                }
            }
            else
                ServerManager.instance.UserRejected(connectionId);
        }
        else
        {
            if (_mysqlLogic.GetComponent<LoginAndRegister>().LogIn(user, pass))
            {
                new PacketBase(PacketIDs.Conected_Command).SendAsServer(connectionId);
                _mysqlLogic.GetComponent<UserCommandsFunc>().SetConnectionState(user,"C",connectionId);
                _users[connectionId] = user;

                var connectedFriends = _mysqlLogic.GetComponent<UserCommandsFunc>().GetUserFriends(user).Item1.Select(x => _mysqlLogic.GetComponent<UserCommandsFunc>().GetUserConnectionStateAndID(x))
                                                                                                              .Where(y=> y.Item2.ToLower() == "c");
                foreach (var friend in connectedFriends)
                {
                    var friendList = _mysqlLogic.GetComponent<UserCommandsFunc>().GetUserFriends(friend.Item1);
                    new PacketBase(PacketIDs.FriendList_Command).Add(friendList.Item1).Add(friendList.Item2).Add(friendList.Item3).SendAsServer(friend.Item3);
                }


            }
            else
                ServerManager.instance.UserRejected(connectionId);
        }
    }

    public void OnUserDisconected(int id)
    {
        var userName = "";

        foreach (var user in _users)
        {
            if (user.Key.Equals(id))
            {
                userName = user.Value;
                break;
            }
        }
        if (_users.ContainsKey(id))
            _users.Remove(id);

        _mysqlLogic.GetComponent<UserCommandsFunc>().SetConnectionState(userName, "D", -1);

        var connectedFriends = _mysqlLogic.GetComponent<UserCommandsFunc>().GetUserFriends(userName).Item1.Select(x => _mysqlLogic.GetComponent<UserCommandsFunc>().GetUserConnectionStateAndID(x))
                                                                                                         .Where(y => y.Item2.ToLower() == "c");
        foreach (var friend in connectedFriends)
        {
            var friendList = _mysqlLogic.GetComponent<UserCommandsFunc>().GetUserFriends(friend.Item1);
            new PacketBase(PacketIDs.FriendList_Command).Add(friendList.Item1).Add(friendList.Item2).Add(friendList.Item3).SendAsServer(friend.Item3);
        }

    }

    public void Server_AcceptReject_Friendship(int connectionID, string user1, string user2, string AorR)
    {
        _mysqlLogic.GetComponent<UserCommandsFunc>().AcceptRejectFriendship(user1, user2, AorR.ToLower());
        var user1Aux = _mysqlLogic.GetComponent<UserCommandsFunc>().GetUserConnectionStateAndID(user1);
        var user2Aux = _mysqlLogic.GetComponent<UserCommandsFunc>().GetUserConnectionStateAndID(user2);
        if (user1Aux.Item2.ToUpper() == "C")
        {
            var friendList = _mysqlLogic.GetComponent<UserCommandsFunc>().GetUserFriends(user1);
            new PacketBase(PacketIDs.FriendList_Command).Add(friendList.Item1).Add(friendList.Item2).Add(friendList.Item3).SendAsServer(user1Aux.Item3);
        }
        if (user2Aux.Item2.ToUpper() == "C")
        {
            var friendList = _mysqlLogic.GetComponent<UserCommandsFunc>().GetUserFriends(user2);
            new PacketBase(PacketIDs.FriendList_Command).Add(friendList.Item1).Add(friendList.Item2).Add(friendList.Item3).SendAsServer(user2Aux.Item3);
        }
    }

    public void Server_Delete_Friend(int connectionID, string user1, string user2)
    {
        _mysqlLogic.GetComponent<UserCommandsFunc>().DeleteFriend(user1, user2);
        var user1Aux = _mysqlLogic.GetComponent<UserCommandsFunc>().GetUserConnectionStateAndID(user1);
        var user2Aux = _mysqlLogic.GetComponent<UserCommandsFunc>().GetUserConnectionStateAndID(user2);
        if (user1Aux.Item2.ToUpper() == "C")
        {
            var friendList = _mysqlLogic.GetComponent<UserCommandsFunc>().GetUserFriends(user1);
            new PacketBase(PacketIDs.FriendList_Command).Add(friendList.Item1).Add(friendList.Item2).Add(friendList.Item3).SendAsServer(user1Aux.Item3);
        }
        if (user2Aux.Item2.ToUpper() == "C")
        {
            var friendList = _mysqlLogic.GetComponent<UserCommandsFunc>().GetUserFriends(user2);
            new PacketBase(PacketIDs.FriendList_Command).Add(friendList.Item1).Add(friendList.Item2).Add(friendList.Item3).SendAsServer(user2Aux.Item3);
        }
    }

    public void Server_Add_Friend(int connectionID, string user1, string user2)
    {
        _mysqlLogic.GetComponent<UserCommandsFunc>().AddFriend(user1, user2);

        var user1Aux = _mysqlLogic.GetComponent<UserCommandsFunc>().GetUserConnectionStateAndID(user1);
        var user2Aux = _mysqlLogic.GetComponent<UserCommandsFunc>().GetUserConnectionStateAndID(user2);
        if (user1Aux.Item2.ToUpper() == "C")
        {
            var friendList = _mysqlLogic.GetComponent<UserCommandsFunc>().GetUserFriends(user1);
            new PacketBase(PacketIDs.FriendList_Command).Add(friendList.Item1).Add(friendList.Item2).Add(friendList.Item3).SendAsServer(user1Aux.Item3);
        }
        if (user2Aux.Item2.ToUpper() == "C")
        {
            var friendList = _mysqlLogic.GetComponent<UserCommandsFunc>().GetUserFriends(user2);
            new PacketBase(PacketIDs.FriendList_Command).Add(friendList.Item1).Add(friendList.Item2).Add(friendList.Item3).SendAsServer(user2Aux.Item3);
        }
    }


    //Set Player Values
    public void Server_StartPlayer(string name, int playerId)
    {
        ServerManager.instance.myPlayers[playerId].OnServerStart(name, playerId, values, shootCoolDown);
        new PacketBase(PacketIDs.StartPlayer_Command).Add(name).Add(playerId).Add(values).Add(shootCoolDown).SendAsServer();
    }


    //Shoot
    public void Server_ShootCommand(TypeOfGun.myType type, int playerId)
    {
        var player = ServerManager.instance.myPlayers[playerId];

        if (type == TypeOfGun.myType.pistol)
        {
            PlayerShoot(player, type);
            return;
        }

        if (player.ammoAmount[type] <= 0) return;

        if (type == TypeOfGun.myType.rifle) PlayerShoot(player, type);
        else if (type == TypeOfGun.myType.throwable) PlayerThrowGrenade(player, type);
        else if (type == TypeOfGun.myType.box) PlayerSpawnBox(player, type);
        else if (type == TypeOfGun.myType.largebox) PlayerSpawnLargeBox(player, type);

        player.ammoAmount[type]--;
        player.UpdateAmmo();

        new PacketBase(PacketIDs.UpdateAmmo_Command).Add(type).Add(playerId).Add(1).SendAsServer();
    }

    void PlayerShoot(Player player, TypeOfGun.myType type)
    {
        Bullet bullet = Instantiate(ServerManager.instance.objectsToSpawn[0]).GetComponent<Bullet>();
        bullet.transform.position = player.gun.transform.GetChild(0).transform.position;
        bullet.transform.right = player.gun.transform.GetChild(0).transform.right;
        bullet.playerId = player.connectionId;
        bullet.Setup(bulletSpeed, bulletDamage, Server_Dammaged);
        NetworkServer.Spawn(bullet.gameObject);
    }

    void PlayerThrowGrenade(Player player, TypeOfGun.myType type)
    {
        Grenade grenade = Instantiate(ServerManager.instance.objectsToSpawn[1]).GetComponent<Grenade>();
        grenade.playerId = player.connectionId;
        grenade.transform.position = player.gun.transform.position;
        grenade.transform.right = player.transform.right;
        grenade.Setup(grenadeSpeed, grenadeLife, grenadeDamage, Server_Dammaged, player.transform.right);
        NetworkServer.Spawn(grenade.gameObject);
    }

    void PlayerSpawnBox(Player player, TypeOfGun.myType type)
    {
        DestroyableObject smallBox = Instantiate(ServerManager.instance.objectsToSpawn[2]).GetComponent<DestroyableObject>();
        smallBox.transform.position = player.gun.transform.position;
        smallBox.transform.right = player.transform.right;
        smallBox.playerId = player.connectionId;
        smallBox.Setup(lifeSmallBox);
        NetworkServer.Spawn(smallBox.gameObject);
    }

    void PlayerSpawnLargeBox(Player player, TypeOfGun.myType type)
    {
        DestroyableObject largeBox = Instantiate(ServerManager.instance.objectsToSpawn[3]).GetComponent<DestroyableObject>();
        largeBox.transform.position = player.gun.transform.position;
        largeBox.transform.right = player.transform.right;
        largeBox.playerId = player.connectionId;
        largeBox.Setup(lifeLargeBox);
        NetworkServer.Spawn(largeBox.gameObject);
    }

    public void Server_ChangeWeapon(int playerid, int weapon)
    {
        ServerManager.instance.myPlayers[playerid].ChangeWeapon(weapon);
        new PacketBase(PacketIDs.ChangeWeapon_Command).Add(playerid).Add(weapon).SendAsServer();
    }

    //Move
    public void ServerMove(int playerId, float horizontal, float vertical, Vector3 viewDir)
    {
        var player = ServerManager.instance.myPlayers[playerId];
        player.View(viewDir);
        player.Move(horizontal, vertical);
    }

    //Damage
    public void Server_Dammaged(int playerId, int damage)
    {
        ServerManager.instance.myPlayers[playerId].life -= damage;

        if(ServerManager.instance.myPlayers[playerId].life <= 0 && !endGame)
        {
            ServerManager.instance.GameEnded_Command(playerId);
            new PacketBase(PacketIDs.GameEnded_Command).Add(playerId).SendAsServer();

            _mysqlLogic.GetComponent<UserCommandsFunc>().AddLostToUser(_users[playerId]);
            _mysqlLogic.GetComponent<UserCommandsFunc>().AddWinToUser(_users.Where(x=> x.Key != playerId).First().Value);

            endGame = true;
        }
    }

    //Restart
    public void Server_RestartButton()
    {
        _restartCount--;

        if (_restartCount <= 0)
        {
            _restartCount = ServerManager.instance.maxPlayers;

            var walls = GameObject.FindObjectsOfType<DestroyableObject>();
            foreach (var wall in walls)
                NetworkServer.Destroy(wall.gameObject);

            new PacketBase(PacketIDs.GameStart_Command).SendAsServer();

            //ReseT values
            foreach (var player in ServerManager.instance.myPlayers)
            {
                player.Value.OnServerStart(player.Value.myname, player.Value.connectionId, values, shootCoolDown);
                new PacketBase(PacketIDs.StartPlayer_Command).Add(player.Value.myname).Add(player.Value.connectionId).Add(values).Add(shootCoolDown).SendAsServer();
            }

            //Reset position
            ServerManager.instance.Restart_Command();
            new PacketBase(PacketIDs.Restart_Command).SendAsServer();

            endGame = false;
        }
    }


    public void Server_FriendList(int id, string user)
    {
        var friendList = _mysqlLogic.GetComponent<UserCommandsFunc>().GetUserFriends(user);
        new PacketBase(PacketIDs.FriendList_Command).Add(friendList.Item1).Add(friendList.Item2).Add(friendList.Item3).SendAsServer(id);
    }

    public void Server_GetUserHighScores(int id,string user)
    {
        if (user == "")
        {
            Server_GetHighScores_Command(id);
            return;
        }

        var scores = _mysqlLogic.GetComponent<UserCommandsFunc>().GetUserHighScores(user);
        new PacketBase(PacketIDs.WriteHighScore_Command).Add(scores)
        .SendAsServer(id);
    }

    public void Server_GetHighScores_Command(int id)
    {
        var scores = _mysqlLogic.GetComponent<UserCommandsFunc>().GetHighScores();
        new PacketBase(PacketIDs.WriteHighScore_Command).Add(scores)
        .SendAsServer(id);
    }
    public void Server_UserReadyToPlay_Command(int id, string user)
    {
        ServerManager.instance.PlayerReadyToGame(id, user);
    }
}
