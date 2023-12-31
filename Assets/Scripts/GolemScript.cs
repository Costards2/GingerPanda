using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    public Transform groundCheck;
    public LayerMask groundLayer;
    bool playerAbove = false;

    private float timer;
    public GameObject projectilePrefab;
    public Transform bulllePos; 
    public LayerMask playerLayer;
    public Transform shootCheck;
    bool canShoot = true;
    //bool attacked = false ;

    public int maxHealth = 150;
    private int health;

    private Animator animator;
    //private float damageCooldown = 1f;
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
    bool damegeProjectile = true;
    [SerializeField] float rayCastLength;
    bool canShootIdle;

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
        rayCastLength = 6.5f;
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
        Debug.Log(IsGrounded());

        if(IsGrounded())
        {
            chaseSpeed = normalChaseSpeed;
        }
        else
        {
            chaseSpeed = 0;
        }

        //distanceToPlayer = Vector2.Distance(transform.position, player.position);

        //if (player.position.y > transform.position.y && Mathf.Abs(player.position.x - transform.position.x) < 1f)
        //{
        //    playerAbove = true;
        //}
        //else
        //{
        //    playerAbove = false;
        //}

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

        //if (player.position.x > transform.position.x)
        //{
        //    transform.localScale = new Vector3(scale, scale, scale);
        //}
        //else
        //{
        //    transform.localScale = new Vector3(-scale, scale, scale);
        //}
        if (Physics2D.Raycast(transform.position, Vector2.right, rayCastLength, playerLayer) || Physics2D.Raycast(transform.position, Vector2.left, rayCastLength, playerLayer))
        {
            currentState = State.Chase;
        }
        else if ((Physics2D.Raycast(transform.position, Vector2.right, 8.5f, playerLayer) || Physics2D.Raycast(transform.position, Vector2.left, 8.5f, playerLayer)))
        {
            canShootIdle = true;
        }
        else
        {
            canShootIdle = false;
        }

        if (canShoot && canShootIdle)
        {
            timer += Time.deltaTime;

            if (player.position.x > transform.position.x)
            {
                transform.localScale = new Vector3(scale, scale, scale);
            }
            else
            {
                transform.localScale = new Vector3(-scale, scale, scale);
            }

            if(timer > 2f)
            {
                canShoot = false;
                timer = 0;
                currentState = State.Attack;
            }
        }

        //else if (playerAbove || !IsGrounded())
        //{
        //    if (timer > 2 && canShoot)
        //    {
        //        canShoot = false;
        //        timer = 0;
        //        currentState = State.Attack;
        //    }
        //}
    }

    void ChaseState()
    {
        animator.Play("Run");

        timer += Time.deltaTime;

        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, rayCastLength, playerLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, rayCastLength, playerLayer);

        if ((hitRight || hitLeft) && IsGrounded())
        {
            Vector2 directionX = new Vector2(player.position.x - transform.position.x, 0).normalized;
            rb.velocity = new Vector2(directionX.x * chaseSpeed, rb.velocity.y);

            if (playerAbove)
            {
                currentState = State.Idle;
            }

            float flipBuffer = 0.2f; 

            if (player.position.x > transform.position.x + flipBuffer)
            {
                transform.localScale = new Vector3(scale, scale, scale);
            }
            else if (player.position.x < transform.position.x - flipBuffer)
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

        Shoot();

        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, rayCastLength, playerLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, rayCastLength, playerLayer);

        if ((hitRight || hitLeft) && !playerAbove && IsGrounded())
        {
            currentState = State.Chase;
        }
        else if ((Physics2D.Raycast(transform.position, Vector2.right, 8.5f, playerLayer) || Physics2D.Raycast(transform.position, Vector2.left, 8.5f, playerLayer)))        
        {
            currentState = State.Idle;
        }
        else if (!IsGrounded())
        {
            currentState = State.Idle;
        }
    }

    void Shoot()
    {
        Instantiate(projectilePrefab, bulllePos.position, Quaternion.identity);
        canShoot = true;
        Debug.Log("Shot");
 
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

        if (isKb && !damegeProjectile)
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
        else
        {
            isKb = false;
            damegeProjectile = false;
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

    public bool ShootIdleCheck()
    {
        return Physics2D.OverlapBox(shootCheck.position, new Vector2(10, 10), 10, playerLayer);
    }

    public bool IsGrounded()
    {
        return Physics2D.OverlapBox(groundCheck.position, new Vector2(0.25f, 0.25f), 0.25f, groundLayer);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            damegeProjectile = true;
            TakeDamage(25);
        }
    }
}
