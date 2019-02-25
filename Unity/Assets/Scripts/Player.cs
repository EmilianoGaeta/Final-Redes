using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player : NetworkBehaviour {

    
    [SyncVar]
    public int connectionId;
    [SyncVar]
    public string myname;

    public TypeOfGun gun;

    [HideInInspector]
    public int life;

    private float _speed;
    private float _shootCoolDown;
    private float _shoottimer;


    private Rigidbody _rb;
    public float _initialLife;

    //My HUD
    private Image _lifeBar;
    private Text _rifleAmountText;
    private Text _grenadeAmountText;
    private Text _boxAmountText;
    private Text _largeBoxAmountText;

    public Dictionary<TypeOfGun.myType, int> ammoAmount = new Dictionary<TypeOfGun.myType, int>();

    private bool _canPlay = false;

    // Use this for initialization
    void Start()
    {
        _rb = GetComponent<Rigidbody>();

        if (!hasAuthority)
        {
            this.enabled = false;
            return;
        }
        gun = transform.Find("Pistol").GetComponent<TypeOfGun>();
        gun.gameObject.SetActive(true);

        var netWorkUI = GameObject.Find("NetWork UI");
        var n = netWorkUI.transform.Find("Name").GetComponent<InputField>().text;
        myname = n != "" ? n : "Player_" + connectionId;

        netWorkUI.SetActive(false);

        _shoottimer = 0;

        new PacketBase(MultiplayerManager.PacketIDs.Server_StartPlayer).Add(myname).Add(connectionId).SendAsClient();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_canPlay) return;

        var dir = (GetDestination() - transform.position).normalized;
        new PacketBase(MultiplayerManager.PacketIDs.Server_Move)
            .Add(Input.GetAxis("Horizontal")).Add(Input.GetAxis("Vertical")).Add(dir)
            .SendAsClient(false);
       
        if (Input.GetMouseButtonDown(0))
        {
            new PacketBase(MultiplayerManager.PacketIDs.Server_ShootCommand)
           .Add(gun.type).Add(connectionId).SendAsClient();
        }

        if (gun.type == TypeOfGun.myType.rifle)
        {
            if (Input.GetMouseButton(0))
            {
                _shoottimer += Time.deltaTime;
                if (_shoottimer >= _shootCoolDown)
                {
                    new PacketBase(MultiplayerManager.PacketIDs.Server_ShootCommand)
                   .Add(gun.type).Add(connectionId).SendAsClient();

                    _shoottimer = 0;
                }
            }
        }

        #region Change Weapons
        if (Input.GetKeyDown(KeyCode.Alpha1))
            new PacketBase(MultiplayerManager.PacketIDs.Server_ChangeWeapon).Add(connectionId).Add(0).SendAsClient();
        if (Input.GetKeyDown(KeyCode.Alpha2))
            new PacketBase(MultiplayerManager.PacketIDs.Server_ChangeWeapon).Add(connectionId).Add(1).SendAsClient();
        if (Input.GetKeyDown(KeyCode.Alpha3))
            new PacketBase(MultiplayerManager.PacketIDs.Server_ChangeWeapon).Add(connectionId).Add(2).SendAsClient();
        if (Input.GetKeyDown(KeyCode.Alpha4))
            new PacketBase(MultiplayerManager.PacketIDs.Server_ChangeWeapon).Add(connectionId).Add(3).SendAsClient();
        if (Input.GetKeyDown(KeyCode.Alpha5))
            new PacketBase(MultiplayerManager.PacketIDs.Server_ChangeWeapon).Add(connectionId).Add(4).SendAsClient();
        #endregion
    }

    public void Move(float horizontal, float vertical)
    {
        _rb.velocity = (Vector3.right * horizontal
                       + Vector3.up * vertical) * _speed;
    }

    public void View(Vector3 dir)
    {   
        transform.right = dir;
    }

    public void ChangeWeapon(int index)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (i == index)
            {
                gun = transform.GetChild(i).GetComponent<TypeOfGun>();
                gun.gameObject.SetActive(true);
            }
            else
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    public void OnServerStart(string myname,int id, int[] values, float shootCoolDown)
    {
        life = values[0];
        _speed = values[1];

        ammoAmount[TypeOfGun.myType.rifle] = values[2];
        ammoAmount[TypeOfGun.myType.throwable] = values[3];
        ammoAmount[TypeOfGun.myType.box] = values[4];
        ammoAmount[TypeOfGun.myType.largebox] = values[5];

        _shootCoolDown = shootCoolDown;

        this.myname = myname;
        SetPlayer(id);
        SetUI(id, myname);
    }

    void SetPlayer(int id)
    {
        if (id == 1)
        {
            GetComponent<SpriteRenderer>().color = Color.red;
            for (int i = 1; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        else if (id == 2)
        {
            GetComponent<SpriteRenderer>().color = Color.green;
            for (int i = 1; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        gun = transform.Find("Pistol").GetComponent<TypeOfGun>();
        gun.gameObject.SetActive(true);
        _initialLife = life;
    }

    void SetUI(int id , string myname)
    {
        var HUD = FindObjectsOfType<MyHUD>();
        for (int i = 0; i < HUD.Length; i++)
        {
            if (HUD[i].playerId == id)
            {
                HUD[i].uiName.text = myname;
                _lifeBar = HUD[i].lifeBar;
                _lifeBar.fillAmount = life / _initialLife;
                _rifleAmountText = HUD[i].rifleAmount;
                _rifleAmountText.text = "x" + ammoAmount[TypeOfGun.myType.rifle];
                _grenadeAmountText = HUD[i].grenadeAmount;
                _grenadeAmountText.text = "x" + ammoAmount[TypeOfGun.myType.throwable];
                _boxAmountText = HUD[i].boxAmount;
                _boxAmountText.text = "x" + ammoAmount[TypeOfGun.myType.box];
                _largeBoxAmountText = HUD[i].largeBoxAmount;
                _largeBoxAmountText.text = "x" + ammoAmount[TypeOfGun.myType.largebox];
            }
        }
        this.myname = myname;
        this.connectionId = id;
    }

    public void Damaged()
    {
        _lifeBar.fillAmount = life / _initialLife;
    }

    private Vector3 GetDestination()
    {
        RaycastHit hit;
        var mask = 1 << LayerMask.NameToLayer("Background");
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, mask)) 
        {
            return new Vector3(hit.point.x, hit.point.y, 0);
        }
        return transform.position;
    }

    public void UpdateAmmo()
    {
        _rifleAmountText.text = "x" + ammoAmount[TypeOfGun.myType.rifle];
        _grenadeAmountText.text = "x" + ammoAmount[TypeOfGun.myType.throwable];
        _boxAmountText.text = "x" + ammoAmount[TypeOfGun.myType.box];
        _largeBoxAmountText.text = "x" + ammoAmount[TypeOfGun.myType.largebox];
    }

    public void CanPlay(bool play)
    {
        _canPlay = play;
    }
}
