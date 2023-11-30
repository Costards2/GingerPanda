using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements;

public class Boss : MonoBehaviour
{
    public int maxHealth = 100;
    int currentHealth;
    private SpriteRenderer sprite;
    private Color normalColor;
    private Animator animator;
    public GameObject vinePrefab;
    public Transform[] vineSpawnPoint = new Transform[6];
    public int wichSpawPointOfTheVine1;
    public int wichSpawPointOfTheVine2;
    public int wichSpawPointOfTheVine3;

    float idleTime;

    private enum State
    {
        Idle,
        Attack1,
        Attack2
    }

    private State currentState;

    void Start()
    {
        currentHealth = maxHealth;
        sprite = GetComponent<SpriteRenderer>();
        normalColor = sprite.color;
        animator = GetComponent<Animator>();
        currentState = State.Idle;
    }

    void Update()
    {
        Debug.Log(currentState);

        switch (currentState)
        {
            case State.Idle:
                IdleState();
                break;
            case State.Attack1:
                AttackState1();
                break;
            case State.Attack2:
                AttackState2();
                break;
        }
    }


    void IdleState()
    {
        if (gameObject.layer != LayerMask.NameToLayer("Enemy"))
        {
           //gameObject.layer = LayerMask.NameToLayer("Enemy");
        }

        idleTime = 0;

        animator.Play("Idle");

        if (animator.GetBool("finishedIdle") == true)
        {
            animator.SetBool("finishedIdle", false);
            currentState = State.Attack1;
        }
    }

    void AttackState1()
    {
        if (gameObject.layer != LayerMask.NameToLayer("BossInvunarable"))
        {
            //gameObject.layer = LayerMask.NameToLayer("BossInvunarable");
        }

        animator.Play("ATK1");

        ChangeToATCK2();

        if (animator.GetBool("finishedATK1") == true)
        {
            animator.SetBool("finishedATK1", false);
            currentState = State.Attack2;
        }

    }

    void AttackState2()
    {
        if (gameObject.layer != LayerMask.NameToLayer("BossInvunarable"))
        {
           //gameObject.layer = LayerMask.NameToLayer("BossInvunarable");
        }

        animator.Play("ATK2");

        if(animator.GetBool("finishedATK2") == true)
        {
            animator.SetBool("finishedATK2", false);
            currentState = State.Idle;
        }
    }

    public void ChangeToATCK1()
    {
        animator.SetBool("finishedIdle", true);
    }

    public void ChangeToATCK2()
    {
        animator.SetBool("finishedATK1", true);
    }

    public void ChangeToIdle()
    {
        animator.SetBool("finishedATK2", true);
    }

    public void TakeDamage(int damage)
    {

        currentHealth -= damage;
        StartCoroutine(VisualDamage());



        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public IEnumerator VisualDamage()
    {
        for (int i = 0; i < 1; i++)
        {
            sprite.color = new Color(0.68f, 0.17f, 0.17f, 0.90f);

            yield return new WaitForSeconds(0.15f);

            sprite.color = normalColor;

            yield return new WaitForSeconds(0.15f);
        }
    }

    public void VineInstantiate()
    {
        wichSpawPointOfTheVine1 = UnityEngine.Random.Range(0, vineSpawnPoint.Length);

        GameObject newVine1 = Instantiate(vinePrefab, vineSpawnPoint[wichSpawPointOfTheVine1].position, Quaternion.identity);
    }

    //public IEnumerator ATK1()
    //{
    //    animator.Play("ATK1");

    //    yield return new WaitForSeconds(5f);

    //    currentState = State.Attack2;
    //}

    //public IEnumerator ATK2()
    //{
    //    animator.Play("ATK2");

    //    do
    //    {
    //        wichSpawPointOfTheVine1 = UnityEngine.Random.Range(0, vineSpawnPoint.Length);
    //        wichSpawPointOfTheVine2 = UnityEngine.Random.Range(0, vineSpawnPoint.Length);
    //        wichSpawPointOfTheVine3 = UnityEngine.Random.Range(0, vineSpawnPoint.Length);

    //    } while
    //        (wichSpawPointOfTheVine1 == wichSpawPointOfTheVine2 || wichSpawPointOfTheVine1 == wichSpawPointOfTheVine3 || wichSpawPointOfTheVine2 == wichSpawPointOfTheVine3);

    //    if((wichSpawPointOfTheVine1 != wichSpawPointOfTheVine2 || wichSpawPointOfTheVine1 != wichSpawPointOfTheVine3 || wichSpawPointOfTheVine2 != wichSpawPointOfTheVine3))
    //    {
    //        GameObject newVine1 = Instantiate(vinePrefab, vineSpawnPoint[wichSpawPointOfTheVine1].position, Quaternion.identity);

    //        yield return new WaitForSeconds(2f);

    //        GameObject newVine2 = Instantiate(vinePrefab, vineSpawnPoint[wichSpawPointOfTheVine2].position, Quaternion.identity);

    //        yield return new WaitForSeconds(2f);

    //        GameObject newVine3 = Instantiate(vinePrefab, vineSpawnPoint[wichSpawPointOfTheVine3].position, Quaternion.identity);

    //        yield return new WaitForSeconds(0.5f);

    //        currentState = State.Idle;
    //    }
    //}

    void Die()
    {
        
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Projectile"))
        {
            TakeDamage(20);
        }
    }
}

