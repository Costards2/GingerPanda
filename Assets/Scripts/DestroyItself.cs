using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyItself : MonoBehaviour
{
    //public bool parentAlive = true;

    private void Start()
    {
        Destroy(gameObject, 3f);
    }

    //private void Update()
    //{
    //    if (!parentAlive)
    //    {
    //        Destroy(gameObject);
    //    }
    //}
}
