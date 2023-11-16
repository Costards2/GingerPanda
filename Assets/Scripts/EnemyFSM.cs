using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class EnemyFSM : MonoBehaviour
{
    public int maxHealth = 100;
    int currentHealth;
    public float tempo = 1;
    private SpriteRenderer sprite;
    private Color normalColor;
    bool isKb;
    private readonly float kbForceX = 6f;
    private readonly float kbForceY = 0f;
    private Rigidbody2D rb; 

    public float walkSpeed = 5f;
    private float currentWalkSpeed; 
    public Transform pointRight;
    public Transform pointLeft;
    private float right;
    private float left;

    private enum EnemyState
    {
        WalkingRight,
        WalkingLeft,
        TakeDamage
    }

    private EnemyState currentState;
    private EnemyState beforeState;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        normalColor = sprite.color;
        currentWalkSpeed = walkSpeed;
        currentHealth = maxHealth;
        currentState = EnemyState.WalkingRight;
        right = pointRight.position.x;
        left = pointLeft.position.x;
        rb = GetComponent<Rigidbody2D>();
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

    IEnumerator DamageWait()
    {
        walkSpeed = 0;
        yield return new WaitForSeconds(0.3f);
        walkSpeed = currentWalkSpeed;
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

    public void TakeDamage(int damage)
    {
        beforeState = currentState; 
        currentHealth -= damage;
        StartCoroutine(DamageWait());
        StartCoroutine(Damage());

        //Hurt anim 

        isKb = true;

        if (currentHealth < 0)
        {
            Die();
        }
        if (isKb)
        {
            if (currentState == EnemyState.WalkingRight)
            {
                rb.velocity = new Vector2(-kbForceX, kbForceY);
                isKb = false;
            }
            else if (currentState == EnemyState.WalkingLeft)
            {
                rb.velocity = new Vector2(kbForceX, kbForceY);
                isKb = false;
            }
        }

        Invoke(nameof(StopKB), 0.15f);
    }

    void StopKB()
    {
        rb.velocity = new Vector2(0, 0);
        currentState = beforeState;
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
