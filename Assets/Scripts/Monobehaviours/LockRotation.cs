using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockRotation : MonoBehaviour
{
    
    void Update()
    {
        transform.localPosition = new Vector3(0f, 0f, 0f);
        transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
    }
}
