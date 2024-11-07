using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToTarget : MonoBehaviour
{
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private Transform lookAt;

    
    void Update()
    {
        Debug.DrawRay(attackOrigin.position, lookAt.forward * 100, Color.red);
        transform.LookAt(lookAt);
        
    }
}
