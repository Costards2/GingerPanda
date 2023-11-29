using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ManaSystem;

public class ManaCanvas : MonoBehaviour
{
    [SerializeField] private ManaSystem manaSystem;
    public Image barImage;

    void Start()
    {
        //manaSystem.GetManaNormalized();
        barImage = transform.Find("Mana").GetComponentInChildren<Image>();
        manaSystem = FindAnyObjectByType<ManaSystem>();
    }

    void Update()
    {
        barImage.fillAmount = manaSystem.GetManaNormalized();
    }
}
