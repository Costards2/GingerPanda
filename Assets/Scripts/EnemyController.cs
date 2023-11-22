using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements.Experimental;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 5f;
    
    public float playerDetectionRadius = 10f;
    public float laserAttackRange = 5f;
    public float laserDamage = 10f;
    public float maxHealth = 100f;
    public float idleDuration = 0.35f;
    bool isKb;
    private readonly float kbForceX = 6f;
    private readonly float kbForceY = 0f;
    private Rigidbody2D rb;
    float facing;
    private SpriteRenderer sprite;
    private Color normalColor;
    float beforeMoveSpeed;
    private bool isFacingLeft = true;
    [SerializeField] private Animator animator;

    private float currentHealth;
    private Transform player;
    private CircleCollider2D detectionCollider;

    private enum EnemyState
    {
        Idle,
        Chase,
        Attack
    }

    private EnemyState currentState = EnemyState.Idle;
    private EnemyState beforeState;

    private void Start()
    {
        detectionCollider = gameObject.AddComponent<CircleCollider2D>();
        detectionCollider.radius = playerDetectionRadius;
        detectionCollider.isTrigger = true;
        sprite = GetComponent<SpriteRenderer>();
        normalColor = sprite.color;
        beforeMoveSpeed = moveSpeed;
        rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                Idle();
                break;

            case EnemyState.Chase:
                MoveTowardsPlayer();

                if (IsPlayerInAttackRange())
                {
                    currentState = EnemyState.Attack;
                }
                break;

            case EnemyState.Attack:
                AttackPlayer();

                if (!IsPlayerInAttackRange())
                {
                    currentState = EnemyState.Chase;
                }
                break;
        }
    }

    private void Idle()
    {
        animator.Play("Idle");

        moveSpeed = 0;
        StartCoroutine(WaitBeforeChase());
    }

    IEnumerator WaitBeforeChase()
    {
        yield return new WaitForSeconds(idleDuration);

        if (player.position.x != transform.position.x)
        {
            moveSpeed = beforeMoveSpeed;
            currentState = EnemyState.Chase;
        }
    }

    private void MoveTowardsPlayer()
    {
        Flip();

        if (player != null)
        {
            float distanceToPlayer = Mathf.Abs(player.position.x - transform.position.x);

            if (distanceToPlayer < playerDetectionRadius)
            {
                if (distanceToPlayer > 0.1f)
                {
                    Vector2 direction = new Vector2(player.position.x - transform.position.x, 0f).normalized;
                    rb.velocity = direction * moveSpeed;
                    animator.Play("Chase");
                }
                else
                {
                    rb.velocity = Vector2.zero;
                    currentState = EnemyState.Idle;
                    animator.Play("Idle");
                }
            }
            else
            {
                rb.velocity = Vector2.zero;
                currentState = EnemyState.Idle;
                animator.Play("Idle");
            }
        }
    }

    private bool IsPlayerInAttackRange()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, laserAttackRange, LayerMask.GetMask("Player"));
        return playerCollider != null && playerCollider.CompareTag("Player");
    }

    private void AttackPlayer()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, player.position - transform.position, laserAttackRange, LayerMask.GetMask("Player"));

        Debug.DrawRay(transform.position, (player.position - transform.position).normalized * laserAttackRange, Color.red);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            Debug.Log("Hit player with laser!");

            hit.collider.gameObject.GetComponent<MoveFSM>().ReceiveDamage(1);
        }
    }

    public void TakeDamage(float damage)
    {
        beforeState = currentState;
        currentHealth -= damage;
        StartCoroutine(Damage());

        if (currentHealth <= 0)
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

    IEnumerator DamageWait()
    {
        moveSpeed = 0;
        yield return new WaitForSeconds(0.3f);
        moveSpeed = beforeMoveSpeed;
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

    void StopKB()
    {
        rb.velocity = new Vector2(0, 0);
        currentState = beforeState;
    }

    private void Die()
    {
        Debug.Log("Enemy died!");
        Destroy(gameObject);
    }

    void Flip()
    {
        if (isFacingLeft && rb.velocity.x > 0f || !isFacingLeft && rb.velocity.x < 0f)
        {
            isFacingLeft = !isFacingLeft;
            transform.Rotate(0, 180, 0);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            currentState = EnemyState.Chase;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            currentState = EnemyState.Idle;
        }
    }
}
