using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mana : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform shootingPoint;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        shootingPoint.rotation = gameObject.transform.rotation;
        Instantiate(bulletPrefab, shootingPoint.position, shootingPoint.rotation);
    }
}
