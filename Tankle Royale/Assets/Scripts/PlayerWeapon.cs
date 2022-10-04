using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Stats")]
    public int damage;
    public int curAmmo;
    public int maxAmmo;
    public float bulletSpeed;
    public float shootRate;

    private float lastShootTime;

    public GameObject bulletPrefab;
    public Transform bulletSpawnPos;
    public GameObject explosionPrefab;

    private PlayerController player;

    void Awake ()
    {
        //get required components
        player = GetComponent<PlayerController>();
    }

    public void TryShoot ()
    {
        //can we shoot?
        if (curAmmo <= 0 || Time.time - lastShootTime < shootRate)
            return;

        curAmmo--;
        lastShootTime = Time.time;

        // update the ammo UI
        GameUI.instance.UpdateAmmoText();

        // spawn the bullet
        player.photonView.RPC("SpawnBullet", RpcTarget.All, bulletSpawnPos.position, Camera.main.transform.forward);
    }

    public void TryExplode (Vector3 pos)
    {
        player.photonView.RPC("SpawnExplosion", RpcTarget.All, pos);
    }

    [PunRPC]
    void SpawnBullet (Vector3 pos, Vector3 dir)
    {
        // spawn and orient it
        GameObject bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
        bulletObj.transform.forward = dir;

        //get bullet script
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();

        //initialize it and set velocity
        bulletScript.Initialize(damage, player.id, player.photonView.IsMine, this);
        bulletScript.rig.velocity = dir * bulletSpeed;
    }

    [PunRPC]
    public void GiveAmmo (int ammoToGive)
    {
        curAmmo = Mathf.Clamp(curAmmo + ammoToGive, 0, maxAmmo);

        //update ammo ui
        GameUI.instance.UpdateAmmoText();
    }

    [PunRPC]
    void SpawnExplosion(Vector3 pos)
    {
        // spawn and orient it
        GameObject explosionObj = Instantiate(explosionPrefab, pos, Quaternion.identity);

        //get bullet script
        Explosion explosionScript = explosionObj.GetComponent<Explosion>();

        //initialize it and set velocity
        explosionScript.Initialize(damage, player.id, player.photonView.IsMine);
    }
}
