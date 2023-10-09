using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatheusTest : MonoBehaviour
{
    public int maxHealth = 100;
    int currentHealth; 

    void Start()
    {
        currentHealth = maxHealth; 
    }

    void Update()
    {
        
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        //Hurt anim 

        if(currentHealth < 0) 
        {
            Die(); 
        }
        
    }

    void Die()
    {
        //Die anim

        Destroy(gameObject);
    }
}