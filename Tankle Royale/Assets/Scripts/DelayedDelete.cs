using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedDelete : MonoBehaviour
{
    public float timeUntilDelete;
    void Start()
    {
        Destroy(gameObject, timeUntilDelete);
    }
}
