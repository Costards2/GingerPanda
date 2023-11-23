using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements.Experimental;

public class GolemScript : MonoBehaviour
{
    public float chaseSpeed = 2.5f;
    public float idleTime = 2f;
    public float projectileSpeed = 7f;
    public float knockbackForce = 5f;
    public GameObject projectilePrefab;
    public float chaseDistance = 5f; 

    private Transform player;
    public int maxHealth = 100;
    private int health;
    private Animator animator;
    private float damageCooldown = 1f;
    private float lastDamageTime;
    private Vector2 knockbackDirection;
    public Rigidbody2D rb;

    private float attackCooldown = 2f;
    private float lastAttackTime;
    bool canShoot = true;
    float scale;
    private SpriteRenderer sprite;
    private Color normalColor;
    bool isKb;
    private readonly float kbForceX = 6f;
    private readonly float kbForceY = 0f;
    float facing;

    private enum State
    {
        Idle,
        Chase,
        Attack
    }

    private State currentState;
    private State beforeState;

    void Start()
    {
        currentState = State.Idle;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        scale = transform.localScale.x;
        sprite = GetComponent<SpriteRenderer>();
        normalColor = sprite.color;
        health = maxHealth;
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Idle:
                IdleState();
                break;
            case State.Chase:
                ChaseState();
                break;
            case State.Attack:
                AttackState();
                break;
        }
    }

    void IdleState()
    {
        animator.Play("Idle");

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < chaseDistance)
        {
            currentState = State.Chase;
        }
    }

    void ChaseState()
    {
        animator.Play("Run");

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        Vector2 directionX = new Vector2(player.position.x - transform.position.x, 0).normalized;
        rb.velocity = new Vector2(directionX.x * chaseSpeed, rb.velocity.y);

        if (player.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(scale, scale, scale);
        }
        else
        {
            transform.localScale = new Vector3(-scale, scale, scale);
        }

        if (canShoot)
        {
            StartCoroutine(ShootCooldown());
        }
    }

    IEnumerator ShootCooldown()
    {
        canShoot = false;
        currentState = State.Attack;
        yield return new WaitForSeconds(3);
        canShoot = true;
        currentState = State.Chase; 
    }

    void AttackState()
    {
        animator.Play("ATK");

        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        Vector2 direction = (player.position - transform.position).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        projectile.GetComponent<Rigidbody2D>().velocity = direction * projectileSpeed;

        lastAttackTime = Time.time;

        currentState = State.Chase;
    }

    public void TakeDamage(int damage)
    {
        health = health - damage;

        if (health <= 0)
        {
            Die();
        }

        facing = FindObjectOfType<MoveFSM>().facing;

        isKb = true;

        if (isKb)
        {
            if (facing == 1)
            {
                rb.velocity = new Vector2(-kbForceX, kbForceY);
                isKb = false;
            }
            else if (facing == -1)
            {
                rb.velocity = new Vector2(kbForceX, kbForceY);
                isKb = false;
            }
        }
        Invoke(nameof(StopKB), 0.15f);
        //StartCoroutine(DamageWait());
    }

    //IEnumerator DamageWait()
    //{
    //    //walkSpeed = 0;
    //    yield return new WaitForSeconds(0.3f);
    //    //walkSpeed = currentWalkSpeed;
    //}

    void StopKB()
    {
        rb.velocity = new Vector2(0, 0);
        currentState = beforeState;
    }

    public IEnumerator Damage()
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            TakeDamage(20);
        }
    }
}
