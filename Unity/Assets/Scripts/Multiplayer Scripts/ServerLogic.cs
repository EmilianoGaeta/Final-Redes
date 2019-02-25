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

    void Awake()
    {
        instance = this;
        values = new int[] { life, speed, rifleAmountStart, grenadeAmountStart, boxAmountStart, largeBoxAmountStart};
        _mysqlLogic = GameObject.Find("MysqlLogic");
    }
    void Start()
    {
        _restartCount = MultiplayerManager.instance.maxPlayers;
    }

    //Check user
    public void Server_CheckUser(string user, string pass)
    {
        if (_mysqlLogic.GetComponent<LoginAndRegister>().LogIn(user,pass))
        {
            MultiplayerManager.instance.OnUserChecked(true);
        }
        else
        {
            MultiplayerManager.instance.OnUserChecked(false);
        }
    }


    //Set Player Values
    public void Server_StartPlayer(string name, int playerId)
    {
        MultiplayerManager.instance.myPlayers[playerId].OnServerStart(name, playerId, values, shootCoolDown);
        MultiplayerManager.instance.myPlayers[playerId].CanPlay(false);
        new PacketBase(MultiplayerManager.PacketIDs.StartPlayer_Command).Add(name).Add(playerId).Add(values).Add(shootCoolDown).SendAsServer();
    }


    //Shoot
    public void Server_ShootCommand(TypeOfGun.myType type, int playerId)
    {
        var player = MultiplayerManager.instance.myPlayers[playerId];

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

        new PacketBase(MultiplayerManager.PacketIDs.UpdateAmmo_Command).Add(type).Add(playerId).Add(1).SendAsServer();
    }

    void PlayerShoot(Player player, TypeOfGun.myType type)
    {
        Bullet bullet = Instantiate(MultiplayerManager.instance.objectsToSpawn[0]).GetComponent<Bullet>();
        bullet.transform.position = player.gun.transform.GetChild(0).transform.position;
        bullet.transform.right = player.gun.transform.GetChild(0).transform.right;
        bullet.playerId = player.connectionId;
        bullet.Setup(bulletSpeed, bulletDamage);
        NetworkServer.Spawn(bullet.gameObject);
    }

    void PlayerThrowGrenade(Player player, TypeOfGun.myType type)
    {
        Grenade grenade = Instantiate(MultiplayerManager.instance.objectsToSpawn[1]).GetComponent<Grenade>();
        grenade.playerId = player.connectionId;
        grenade.transform.position = player.gun.transform.position;
        grenade.transform.right = player.transform.right;
        grenade.moveVector = player.transform.right;
        grenade.Setup(grenadeSpeed, grenadeLife, grenadeDamage);
        NetworkServer.Spawn(grenade.gameObject);
    }

    void PlayerSpawnBox(Player player, TypeOfGun.myType type)
    {
        DestroyableObject smallBox = Instantiate(MultiplayerManager.instance.objectsToSpawn[2]).GetComponent<DestroyableObject>();
        smallBox.transform.position = player.gun.transform.position;
        smallBox.transform.right = player.transform.right;
        smallBox.playerId = player.connectionId;
        smallBox.Setup(lifeSmallBox);
        NetworkServer.Spawn(smallBox.gameObject);
    }

    void PlayerSpawnLargeBox(Player player, TypeOfGun.myType type)
    {
        DestroyableObject largeBox = Instantiate(MultiplayerManager.instance.objectsToSpawn[3]).GetComponent<DestroyableObject>();
        largeBox.transform.position = player.gun.transform.position;
        largeBox.transform.right = player.transform.right;
        largeBox.playerId = player.connectionId;
        largeBox.Setup(lifeLargeBox);
        NetworkServer.Spawn(largeBox.gameObject);
    }

    public void Server_ChangeWeapon(int playerid, int weapon)
    {
        MultiplayerManager.instance.myPlayers[playerid].ChangeWeapon(weapon);
        new PacketBase(MultiplayerManager.PacketIDs.ChangeWeapon_Command).Add(playerid).Add(weapon).SendAsServer();
    }

    //Move
    public void ServerMove(int playerId, float horizontal, float vertical, Vector3 viewDir)
    {
        var player = MultiplayerManager.instance.myPlayers[playerId];
        player.View(viewDir);
        player.Move(horizontal, vertical);
    }

    //Damage
    public void Server_Dammaged(int playerId, int damage)
    {
        MultiplayerManager.instance.myPlayers[playerId].life -= damage;
        MultiplayerManager.instance.myPlayers[playerId].Damaged();

        var l = MultiplayerManager.instance.myPlayers[playerId].life;

        new PacketBase(MultiplayerManager.PacketIDs.Damaged_Command).Add(playerId).Add(l).SendAsServer();

        if(l <= 0)
        {
            MultiplayerManager.instance.GameEnded_Command(playerId);
            new PacketBase(MultiplayerManager.PacketIDs.GameEnded_Command).Add(playerId).SendAsServer();
        }
    }

    //Restart
    public void Server_RestartButton()
    {
        _restartCount--;

        if (_restartCount <= 0)
        {
            _restartCount = MultiplayerManager.instance.maxPlayers;

            var walls = GameObject.FindObjectsOfType<DestroyableObject>();
            foreach (var wall in walls)
                NetworkServer.Destroy(wall.gameObject);

            new PacketBase(MultiplayerManager.PacketIDs.GameStart_Command).SendAsServer();
            MultiplayerManager.instance.GameStart_Command();

            //Reser values
            foreach (var player in MultiplayerManager.instance.myPlayers)
            {
                player.Value.OnServerStart(player.Value.myname, player.Value.connectionId, values, shootCoolDown);
                new PacketBase(MultiplayerManager.PacketIDs.StartPlayer_Command).Add(player.Value.myname).Add(player.Value.connectionId).Add(values).Add(shootCoolDown).SendAsServer();
            }

            //Reset position
            MultiplayerManager.instance.Restart_Command();
            new PacketBase(MultiplayerManager.PacketIDs.Restart_Command).SendAsServer();
        }
    }
}
