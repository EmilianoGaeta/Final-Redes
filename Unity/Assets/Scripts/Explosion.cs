using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class Explosion : NetworkBehaviour
{

    public int damage;

    private float _timer;
    private Action<int, int> Server_Dammaged;

    // Use this for initialization
    void Start()
    {
        if (!isServer)
            enabled = false;

        _timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= 0.5f)
        {
            NetworkServer.Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isServer)
        {
            var player = other.GetComponent<Player>();
            if (player != null)
            {
                Server_Dammaged(player.connectionId, damage);
            }
        }
    }

    public Explosion Setup(int damage, Action<int, int> Server_Dammaged)
    {
        this.damage = damage;
        this.Server_Dammaged = Server_Dammaged;
        return this;
    }
}
