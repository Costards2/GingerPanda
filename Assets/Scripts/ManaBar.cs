using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class PlayerManaAndAtk : MonoBehaviour
{
    
    public Mana mana; 
    private Image barImage;

    private void Awake()
    {
        barImage = transform.Find("Mana").GetComponent<Image>();
        
        barImage.fillAmount = .3f;

        mana = new Mana();
    }

    private void Update()
    {
        mana.Update();
        barImage.fillAmount = mana.GetManaNormalized();

        if (Input.GetKeyDown(KeyCode.E))
        {
            mana.TrySpend(20);
        }

    }

    public class Mana
    {
        public const int MANA_MAX = 100;

        public float manaAmount;
        public float manaRegenAmount;

        public Mana() {

            manaAmount = 0;
            manaRegenAmount = 7.5f;
        }

        public void Update()
        {
            manaAmount += manaRegenAmount * Time.deltaTime;
            manaAmount = Mathf.Clamp(manaAmount, 0f, MANA_MAX); 
        }

        public void TrySpend(int amount)
        {
            if(manaAmount >= amount ) { 
                manaAmount -= amount;
            }
        }
        
        public float GetManaNormalized()
        {
            return manaAmount / MANA_MAX; 
        }
    }
}
