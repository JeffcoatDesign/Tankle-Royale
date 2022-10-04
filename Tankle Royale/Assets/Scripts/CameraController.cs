using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Look Sensitivity")]
    public float sensX;
    public float sensY;

    [Header("Clamping")]
    public float minY;
    public float maxY;

    [Header("Spectator")]
    public float spectatorMoveSpeed;

    [Header("Barrel")]
    public GameObject barrelObject;
    public float minBarX;
    public float maxBarX;
    public float rotBarOffset;

    private float barRotCoeff;

    private float rotX;
    private float rotY;
    private Quaternion parentQuaternion;

    private bool isSpectator = false;

    void Start ()
    {
        //lock cursor to middle of screen.
        Cursor.lockState = CursorLockMode.Locked;

        //Math for turret control
        barRotCoeff = (maxBarX - minBarX) / (maxY - minY);
    }

    void LateUpdate()
    {
        rotX += Input.GetAxis("Mouse X") * sensX;
        rotY += Input.GetAxis("Mouse Y") * sensY;

        rotY = Mathf.Clamp(rotY, minY, maxY);

        if(isSpectator)
        {
            //rotate camera vertically
            transform.rotation = Quaternion.Euler(-rotY, rotX, 0);

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            float y = 0;

            if (Input.GetKey(KeyCode.E))
                y = 1;
            else if (Input.GetKey(KeyCode.Q))
                y = -1;

            Vector3 dir = transform.right * x + transform.up * y + transform.forward * z;
            transform.position += dir * spectatorMoveSpeed * Time.deltaTime;
        }
        else
        {
            parentQuaternion = transform.parent.parent.rotation;

            //rotate the cam vertically
            transform.localRotation = Quaternion.Euler(-rotY, 0, 0);

            //rotate the player horizontally
            transform.parent.rotation = parentQuaternion * Quaternion.Euler(0, rotX, 0);

            UpdateBarrel();
        }
    }

    void UpdateBarrel ()
    {
        float rotBarValue = Mathf.Clamp(-rotY * barRotCoeff + rotBarOffset, minBarX, maxBarX);
        //rotate the barrel
        barrelObject.transform.localRotation = Quaternion.Euler(rotBarValue, 0, 0);
    }

    public void SetAsSpectator ()
    {
        isSpectator = true;
        transform.parent = null;
    }
}
