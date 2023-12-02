using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Derrota : MonoBehaviour
{
    public GameObject perso, derrota, vitoria;

    void Start()
    {
        derrota.SetActive(false);
    }

    public void Vitoria()
    {
        vitoria.SetActive(true);

        Cursor.visible = true;

        Time.timeScale = 0;
    }
}
