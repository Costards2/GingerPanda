using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class Boss : MonoBehaviour
{
    public int maxHealth = 100;
    int currentHealth;
    private SpriteRenderer sprite;
    private Color normalColor;

    void Start()
    {
        currentHealth = maxHealth;
        sprite = GetComponent<SpriteRenderer>();
        normalColor = sprite.color;
    }

    void Update()
    {

    }

    public void TakeDamage(int damage)
    {

        currentHealth -= damage;
        StartCoroutine(Damage());


        //Hurt anim 

        if (currentHealth < 0)
        {
            Die();
        }
    }

    public IEnumerator Damage()
    {
        for (int i = 0; i < 1; i++)
        {
            sprite.color = new Color(0.68f, 0.17f, 0.17f, 0.90f);

            //sprite.enabled = true;

            yield return new WaitForSeconds(0.15f);

            sprite.color = normalColor;

            //sprite.enabled = false; 

            yield return new WaitForSeconds(0.15f);

            //sprite.enabled = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Projectile"))
        {
            TakeDamage(20);
        }
    }

    void Die()
    {
        //Die anim

        Destroy(gameObject);
    }
}
