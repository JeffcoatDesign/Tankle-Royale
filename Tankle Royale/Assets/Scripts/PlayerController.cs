using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PlayerController : MonoBehaviourPun
{
    [Header("Info")]
    public int id;
    private int curAttackerId;

    [Header("Stats")]
    public float moveSpeed;
    public float jumpForce;
    public float rotationSpeed;
    public int curHp;
    public int maxHp;
    public int kills;
    public bool dead;

    private bool flashingDamage;
    private Vector3 camForward;
    private Vector3 camRight;
    private float curCamEulerY;

    [Header("Components")]
    public Rigidbody rig;
    public Player photonPlayer;
    public PlayerWeapon weapon;
    public MeshRenderer mr;
    public TextMeshPro nameText;

    [Header("Raycast Components")]
    public Transform frontLeftCorner;
    public Transform frontRightCorner;
    public Transform backRightCorner;
    public Transform backLeftCorner;

    [PunRPC]
    public void Initialize (Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;

        GameManager.instance.players[id - 1] = this;
        nameText.text = player.NickName;

        if (!photonView.IsMine)
        {
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
            rig.isKinematic = true;
        }
        else
        {
            GameUI.instance.Initialize(this);
            camForward = Camera.main.transform.forward;
            camRight = Camera.main.transform.right;
            curCamEulerY = Camera.main.transform.eulerAngles.y;
            nameText.enabled = false;
        }
    }

    void Update ()
    {
        if (nameText != null && isActiveAndEnabled)
        {
            nameText.transform.LookAt(Camera.main.transform.position);
            nameText.transform.Rotate(0, 180, 0);
        }

        if (!photonView.IsMine || dead)
            return;
        
        Move();

        //if (Input.GetKeyDown(KeyCode.Space))
        //    TryJump();

        if (Input.GetMouseButtonDown(0))
            weapon.TryShoot();
    }

    void Move ()
    {
        bool isRotating = false;
        // get input axis
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 dir;

        if (x == 0 && z == 0)
        {
            camForward = Camera.main.transform.forward;
            camRight = Camera.main.transform.right;
            curCamEulerY = Camera.main.transform.eulerAngles.y;
        }

        isRotating = CheckRot(x, z);

        if (!isRotating)
            dir = (camForward * z + camRight * x) * moveSpeed;
        else
            dir = (camForward * z + camRight * x) * (moveSpeed / 2);

        dir.y = rig.velocity.y;
        rig.velocity = dir;

        //MatchTerrain();
    }

    //void TryJump ()
    //{
        // create a ray facing down
        //Ray ray = new Ray(transform.position, Vector3.down);

        //if (Physics.Raycast(ray, 1.5f))
            //rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    //}

    bool CheckRot(float x, float z)
    {
        float currentRotation = transform.eulerAngles.y;
        float angleDifference;
        float targetRotation = 0;

        if (x > 0 && z > 0)
            targetRotation = 45;
        else if (x > 0 && z < 0)
            targetRotation = 135;
        else if (x < 0 && z > 0)
            targetRotation = 315;
        else if (x < 0 && z < 0)
            targetRotation = 225;
        else if (z < 0)
            targetRotation = 180;
        else if (z > 0)
            targetRotation = 0;
        else if (x < 0)
            targetRotation = 270;
        else if (x > 0)
            targetRotation = 90;
        else
            return false;

        targetRotation = modulo(targetRotation + curCamEulerY, 360f);
        angleDifference = modulo(targetRotation - currentRotation, 360f);
        if (currentRotation > targetRotation + 3 || currentRotation < targetRotation - 3)
        {
            if (angleDifference > 180)
            {
                transform.Rotate(0, 360 - rotationSpeed * Time.deltaTime, 0);
            }
            else
            {
                transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
            }
             return true;
        }
        return false;
    }

    //void MatchTerrain ()
    //{
    //    RaycastHit frontRightR;
    //    RaycastHit frontLeftR;
    //    RaycastHit backRightR;
    //    RaycastHit backLeftR;

    //    Vector3 upDirection;

    //    Physics.Raycast(frontRightCorner.position + Vector3.up, Vector3.down, out frontRightR);
    //    Physics.Raycast(frontLeftCorner.position + Vector3.up, Vector3.down, out frontLeftR);
    //    Physics.Raycast(backRightCorner.position + Vector3.up, Vector3.down, out backRightR);
    //    Physics.Raycast(backLeftCorner.position + Vector3.up, Vector3.down, out backLeftR);

    //    Vector3 side1 = frontRightR.point - frontLeftR.point;
    //    Vector3 side2 = frontLeftR.point - backLeftR.point;
    //    Vector3 side3 = backLeftR.point - backRightR.point;
    //    Vector3 side4 = backRightR.point - frontRightR.point;

    //    upDirection = (Vector3.Cross(side1, side2) +
    //        Vector3.Cross(side2, side3) +
    //        Vector3.Cross(side3, side4) +
    //        Vector3.Cross(side4, side1)).normalized;

    //    transform.rotation = Quaternion.LookRotation(transform.forward, -upDirection);
    //}

    float modulo (float x, float m)
    {
        return (x % m + m) % m;
    }

    [PunRPC]
    public void TakeDamage (int attackerId, int damage)
    {
        if (dead)
            return;

        curHp -= damage;
        curAttackerId = attackerId;

        //flash the player red
        photonView.RPC("DamageFlash", RpcTarget.Others);

        //update the healthbar UI
        GameUI.instance.UpdateHealthBar();

        //die if no health left
        if (curHp <= 0)
            photonView.RPC("Die", RpcTarget.All);
    }

    [PunRPC]
    void DamageFlash ()
    {
        if (flashingDamage)
            return;

        StartCoroutine(DamageFlashCoRoutine());

        IEnumerator DamageFlashCoRoutine ()
        {
            flashingDamage = true;

            Color defaultColor = mr.material.color;
            mr.material.color = Color.red;

            yield return new WaitForSeconds(0.05f);

            mr.material.color = defaultColor;
            flashingDamage = false;
        }
    }

    [PunRPC]
    void Die ()
    {
        curHp = 0;
        dead = true;

        GameUI.instance.PlayerDied(nameText.text);

        GameManager.instance.alivePlayers--;

        //host will check win condition
        if (PhotonNetwork.IsMasterClient)
            GameManager.instance.CheckWinCondition();

        // is this our local player?
        if(photonView.IsMine)
        {
            if(curAttackerId != 0)
                GameManager.instance.GetPlayer(curAttackerId).photonView.RPC("AddKill", RpcTarget.All);

            //set the cam to spectator
            GetComponentInChildren<CameraController>().SetAsSpectator();

            //disable the physics and hide the player
            rig.isKinematic = true;
            transform.position = new Vector3(0, -50, 0);
        }
    }

    [PunRPC]
    public void AddKill ()
    {
        kills++;

        // update game UI
        GameUI.instance.UpdatePlayerInfoText();
    }

    [PunRPC]
    public void Heal (int amountToHeal)
    {
        curHp = Mathf.Clamp(curHp + amountToHeal, 0, maxHp);

        //update the health bar ui
        GameUI.instance.UpdateHealthBar();
    }
}
