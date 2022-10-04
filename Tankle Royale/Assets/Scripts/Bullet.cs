using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Bullet : MonoBehaviour
{
    private int damage;
    private int attackerId;
    private bool isMine;
    private PlayerWeapon playerWeapon;

    public Rigidbody rig;
    public float bulletDrop;

    public void Initialize(int damage, int attackerId, bool isMine, PlayerWeapon playerWeapon)
    {
        this.damage = damage;
        this.attackerId = attackerId;
        this.isMine = isMine;
        this.playerWeapon = playerWeapon;

        Destroy(gameObject, 10.0f);
    }

    private void Update()
    {
        Vector3 dir = rig.velocity;
        dir.y -= bulletDrop * Time.deltaTime;
        rig.velocity = dir;
    }

    void OnTriggerEnter(Collider other)
    {


        if (other.CompareTag("Player") && isMine)
        {
            PlayerController player = GameManager.instance.GetPlayer(other.gameObject);

            if (player.id != attackerId)
                player.photonView.RPC("TakeDamage", player.photonPlayer, attackerId, damage);
        }
        if (other.CompareTag("Explosion"))
            return;
        else
        {
            playerWeapon.TryExplode(transform.position);
            Destroy(gameObject);
        }
    }
}
