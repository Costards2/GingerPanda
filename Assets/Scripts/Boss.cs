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
    public int wichSpawPointOfTheVine;
    public float attackCooldown = 2f;
    private float nextAttackTime = 0f;

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
    }

    void Update()
    {
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

        if (Input.GetKeyDown(KeyCode.Y)) 
        {
            Debug.Log("VineAtk");
            currentState = State.Attack2;
        }
    }


    void IdleState()
    {
        animator.Play("Idle");
    
    }

    void AttackState1()
    {
        animator.Play("ATK1");

    }

    void AttackState2()
    {
        float time = Time.deltaTime;

        animator.Play("ATK2");
        wichSpawPointOfTheVine = Random.Range(0, vineSpawnPoint.Length);

        GameObject newVine = Instantiate(vinePrefab, vineSpawnPoint[wichSpawPointOfTheVine].position, Quaternion.identity);
        nextAttackTime = Time.time + attackCooldown;

        currentState = State.Idle;

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

