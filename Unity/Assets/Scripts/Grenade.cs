using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class Grenade : NetworkBehaviour
{


    public Explosion explosion;
    public int playerId;
    public int damage;

    private Vector3 _moveVector;
    private float _speed;
    private Action<int, int> Server_Dammaged;
    private int _life;

    void Start()
    {
        if (!isServer)
            enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += _moveVector * _speed * Time.deltaTime;
        if (_life <= 0)
        {
            Explode();
            NetworkServer.Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isServer)
        {
            var bomb = other.GetComponent<Bullet>();
            var player = other.GetComponent<Player>();
            var destroyableObject = other.GetComponent<DestroyableObject>();
            if (bomb != null)
            {
                Damaged(bomb.damage);
            }
            if (player != null)
            {
                if (player.connectionId != playerId)
                {
                    Explode();
                    NetworkServer.Destroy(gameObject);
                }
            }
            else if (destroyableObject != null)
            {
                if (destroyableObject.playerId != playerId)
                {
                    Explode();
                    NetworkServer.Destroy(gameObject);
                }
            }
            else
            {
                Explode();
                NetworkServer.Destroy(gameObject);
            }

        }
    }

    void Damaged(int damage)
    {
        _life -= damage;
    }
    void Explode()
    {
        Explosion e = Instantiate(explosion);
        e.transform.position = transform.position;
        e.Setup(damage, Server_Dammaged);
        NetworkServer.Spawn(e.gameObject);
    }

    public Grenade Setup(float speed, int life, int damage, Action<int, int> Server_Dammaged, Vector3 moveVector)
    {
        _speed = speed;
        _moveVector = moveVector;
        _life = life;
        this.damage = damage;
        this.Server_Dammaged = Server_Dammaged;
        return this;
    }
}
