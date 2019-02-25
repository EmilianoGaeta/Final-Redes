using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Explosion : NetworkBehaviour
{
    [HideInInspector]
    public int damage;

    private float _timer;

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
                ServerLogic.instance.Server_Dammaged(player.connectionId, damage);
            }
        }
    }

    public Explosion Setup(int damage)
    {
        this.damage = damage;
        return this;
    }
}
