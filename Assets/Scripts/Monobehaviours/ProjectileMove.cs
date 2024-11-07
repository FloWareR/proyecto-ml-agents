using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMove : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float fireRate;
    

    void Update()
    {
        if (speed == 0) return;
        transform.position += transform.forward * (speed * Time.deltaTime);
    }
}
