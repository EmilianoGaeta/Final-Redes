using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Grenade : NetworkBehaviour
{
    

    public Explosion explosion;

    [SyncVar]
    [HideInInspector]
    public int life;
    public int playerId;

    public Vector3 moveVector;

    private float _speed;
    private int _damage;

    void Start()
    {
        if (!isServer)
            enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += moveVector * _speed * Time.deltaTime;
        if (life <= 0)
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
        life -= damage;
    }
    void Explode()
    {
        Explosion e = Instantiate(explosion);
        e.transform.position = transform.position;
        e.Setup(_damage);
        NetworkServer.Spawn(e.gameObject);
    }

    public Grenade Setup(float speed,int life, int damage)
    {
        _speed = speed;
        this.life = life;
        _damage = damage;
        return this;
    }
}
