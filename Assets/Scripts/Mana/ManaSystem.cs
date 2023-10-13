using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaSystem : MonoBehaviour
{
    public float maxMana = 100f;
    public float startingMana = 100f;
    public float manaRegenRate = 5f;

    public float currentMana;

    //[SerializeField] private Image barImage;

    void Start()
    {
        currentMana = startingMana;

        //Não se esqueça que vc tem que declarar esse componente no player caso esse script esteja no player
        //barImage = transform.Find("Mana").GetComponentInChildren<Image>();
    }

    void Update()
    {
        RegenerateMana();
    }

    private void RegenerateMana()
    {
        if (currentMana < maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;
            currentMana = Mathf.Clamp(currentMana, 0f, maxMana);
            //UpdateManaUI();
        }
    }

    public void UpdateManaUI()
    {
        //manaCanvas.barImage.fillAmount = GetManaNormalized();
    }

    public float GetManaNormalized()
    {
        return currentMana / maxMana;
    }

    public bool CanAffordAbility(float abilityCost)
    {
        return currentMana >= abilityCost;
    }

    public void UseAbility(float abilityCost)
    {
        currentMana -= abilityCost;
        currentMana = Mathf.Clamp(currentMana, 0f,maxMana);
        //UpdateManaUI();
    }

}
