using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFSM : MonoBehaviour
{
    public int maxHealth = 100;
    int currentHealth;
    public float tempo = 1;

    public float walkSpeed = 5f;
    private float currentWalkSpeed; 
    public Transform pointRight;
    public Transform pointLeft;
    private float right;
    private float left;

    private enum EnemyState
    {
        WalkingRight,
        WalkingLeft
    }

    private EnemyState currentState;

    void Start()
    {
        currentWalkSpeed = walkSpeed;
        currentHealth = maxHealth;
        currentState = EnemyState.WalkingRight;
        right = pointRight.position.x;
        left = pointLeft.position.x;
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.WalkingRight:
                WalkRight();
                break;
            case EnemyState.WalkingLeft:
                WalkLeft();
                break;
            default:
                break;
        }
    }

    void WalkRight()
    {
        transform.Translate(Time.deltaTime * walkSpeed * Vector2.right);

        if (transform.position.x >= right)  
        {
            StartCoroutine(Wait());
            currentState = EnemyState.WalkingLeft;
        }
    }

    void WalkLeft()
    {
        transform.Translate(Time.deltaTime * walkSpeed * Vector2.left);

        if (transform.position.x <= left)
        {
            StartCoroutine(Wait());
            currentState = EnemyState.WalkingRight;
        }
    }

    IEnumerator Wait()
    {
        walkSpeed = 0;
        yield return new WaitForSeconds(tempo);
        walkSpeed = currentWalkSpeed; 
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        //Hurt anim 

        if (currentHealth < 0)
        {
            Die();
        }

    }

    void Die()
    {
        //Die anim

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
