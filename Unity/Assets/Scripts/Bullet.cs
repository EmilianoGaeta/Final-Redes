using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Bullet : NetworkBehaviour
{

    public int playerId;

    private float _speed;
    [HideInInspector]
    public int damage;

    // Use this for initialization
    void Start()
    {
        if (!isServer)
            enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.right * _speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isServer)
        {
            var player = other.GetComponent<Player>();
            var destroyableObject = other.GetComponent<DestroyableObject>();
            if (player != null)
            {
                if (player.connectionId != playerId)
                {
                    ServerLogic.instance.Server_Dammaged(player.connectionId, damage);
                    NetworkServer.Destroy(gameObject);
                }
            }
            else if (destroyableObject != null)
            {
                if (destroyableObject.playerId != playerId)
                    NetworkServer.Destroy(gameObject);
            }
            else
                NetworkServer.Destroy(gameObject);
        }
    }

    public Bullet Setup(float speed, int damage)
    {
        _speed = speed;
        this.damage = damage;
        return this;
    }
}
