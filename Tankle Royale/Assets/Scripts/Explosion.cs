using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    private int damage;
    private int attackerId;
    private bool isMine;

    public Rigidbody rig;
    public float sizeInc;
    public float targetDiameter;

    public void Initialize(int damage, int attackerId, bool isMine)
    {
        this.damage = damage;
        this.attackerId = attackerId;
        this.isMine = isMine;

        Destroy(gameObject, 1.0f);
    }

    private void Update()
    {
        transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one * targetDiameter, sizeInc * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isMine)
        {
            PlayerController player = GameManager.instance.GetPlayer(other.gameObject);

            if (player.id != attackerId)
                player.photonView.RPC("TakeDamage", player.photonPlayer, attackerId, damage);
        }
    }
}
