using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Derrota : MonoBehaviour
{
    public GameObject perso, derrota;
    // Start is called before the first frame update
    void Start()
    {
        derrota.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (perso == null)
        {
            derrota.SetActive(true);
        }
    }
}
