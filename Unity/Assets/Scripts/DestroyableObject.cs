using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DestroyableObject : NetworkBehaviour
{
    public int playerId;

    private int _life;

    void Start()
    {
        if (!isServer)
            enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_life <= 0)
        {
            NetworkServer.Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isServer)
        {
            var bullet = other.GetComponent<Bullet>();
            var explosion = other.GetComponent<Explosion>();
            if (bullet != null)
            {
                if (bullet.playerId != playerId)
                {
                    Damage(bullet.damage);
                }
            }
            if (explosion != null)
            {
                Damage(explosion.damage);
            }
        }
    }

    void Damage(int damage)
    {
        _life -= damage;
    }

    public DestroyableObject Setup(int life)
    {
        _life = life;
        return this;
    }
}
