using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements.Experimental;

public class GolemScript : MonoBehaviour
{
    public float chaseSpeed;
    private float normalChaseSpeed = 2.5f;
    public float idleTime = 2f;
    public float knockbackForce = 5f;
    float distanceToPlayer;
    public float chaseDistance = 12f;
    private Transform player;

    private float timer;
    public GameObject projectilePrefab;
    public Transform bulllePos; 
    public LayerMask playerLayer;
    public Transform shootCheck;
    bool canShoot = true;
    bool attacked;

    public int maxHealth = 100;
    private int health;

    private Animator animator;
    private float damageCooldown = 1f;
    private float lastDamageTime;
    private Vector2 knockbackDirection;
    public Rigidbody2D rb;

    float scale;
    private SpriteRenderer sprite;
    private Color normalColor;
    bool isKb;
    private readonly float kbForceX = 10f;
    private readonly float kbForceY = 2.25f;
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
        chaseSpeed = normalChaseSpeed;
    }

    void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);

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

        if (distanceToPlayer < chaseDistance)
        {
            currentState = State.Chase;
        }
    }

    void ChaseState()
    {
        timer += Time.deltaTime;

        animator.Play("Run");

        if (distanceToPlayer < chaseDistance)
        {
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

            if (timer > 2 && canShoot)
            {
                canShoot = false;
                timer = 0;
                currentState = State.Attack;
            }
        }
        else
        {
            currentState = State.Idle;
        }
    }

    void AttackState()
    {
        animator.Play("ATK");

        attacked = false;

        Shoot();

        if (distanceToPlayer < chaseDistance && attacked)
        {
            currentState = State.Chase;
        }
        else if (distanceToPlayer > chaseDistance && attacked)
        {
            currentState = State.Idle;
        }
    }

    void Shoot()
    {
        Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        canShoot = true;
        attacked = true;
    }

    public void TakeDamage(int damage)
    {
        health = health - damage;

        StartCoroutine(VisualDamage());

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
        StartCoroutine(DamageWait());
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

    IEnumerator DamageWait()
    {
        chaseSpeed = 0;
        yield return new WaitForSeconds(0.3f);
        chaseSpeed = normalChaseSpeed;
    }

    void StopKB()
    {
        rb.velocity = new Vector2(0, 0);
        currentState = beforeState;
    }

    void Die()
    {
        Destroy(gameObject);
    }

    public bool ShootCheck()
    {
        return Physics2D.OverlapBox(shootCheck.position, new Vector2(5, 5), 5, playerLayer);
    }

    //public bool ChaseCheck()
    //{
    //    return Physics2D.OverlapBox(shootCheck.position, new Vector2(3, 3), 3, playerLayer); //I decides this was not necessary anymore, after all I already made a player distance
    //}

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(shootCheck.position, 5);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            TakeDamage(20);
        }
    }
}
