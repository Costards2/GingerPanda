using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mana : MonoBehaviour
{
    public ManaPlay mana;
    public GameObject bulletPrefab;
    public Transform shootingPoint;
    public float manaMana;

    private void Awake()
    {
        mana = new ManaPlay();
    }

    void Update()
    {
        mana.Update();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (mana.manaAmount > 20)
            {
                mana.TrySpend(20);
                Shoot();
            }
        }
    }

    void Shoot()
    {
        shootingPoint.rotation = gameObject.transform.rotation;
        Instantiate(bulletPrefab, shootingPoint.position, shootingPoint.rotation);
    }

    public class ManaPlay
    {
        public const int MANA_MAX = 100;

        public float manaAmount;
        public float manaRegenAmount;

        public ManaPlay()
        {

            manaAmount = 0;
            manaRegenAmount = 15f;
        }

        public void Update()
        {
            manaAmount += manaRegenAmount * Time.deltaTime;
            manaAmount = Mathf.Clamp(manaAmount, 0f, MANA_MAX);
        }

        public void TrySpend(int amount)
        {
            if (manaAmount >= amount)
            {
                manaAmount -= amount;
            }
        }

        public float GetManaNormalized()
        {
            return manaAmount / MANA_MAX;
        }
    }
}
