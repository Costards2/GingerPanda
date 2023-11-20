using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class MoveFSM : MonoBehaviour
{
    [Header("Base Move")]
    public float horizontalInput = 0f;
    public bool canMove = true;
    [SerializeField] private float speed = 8f;
    private float jumpingPower = 16f;
    private bool isFacingRight = true;
    Vector2 moveDirection;
    private bool canJump = true;
    public bool doNothing = false;
  
    [Header("Everything related to Wall")]
    private readonly bool isWallSliding;
    private readonly float wallSlidingSpeed = 2f;
    private bool isWallJumping;
    private float wallJumpingDirection;
    //WallJumpPowerX era 9,5f
    private Vector2 wallJumpingPower = new (50f, 14f);
    public float facing = 0;
    bool canWallJump = true;
    public float wallJumpTime = 0.3f;
    public float wallJumpCooldown = 1f;
    private float wallJumpingCounter;
    private readonly float wallJumpingDuration = 0.2f;
    private readonly float wallJumpingTime = 0.2f;

    [Header("Dash")]
    public bool canDash = true;
    private bool isDashing;
    bool jumpInput = false;
    public bool dashInput = false;
    public float dashingPower = 14f;
    public float dashingTime = 0.2f;
    public float dashingCooldown = 1f;

    [Header("Shoot")]
    public float shootCost = 20f; 
    public GameObject bulletPrefab;
    public Transform shootingPoint;
    readonly float shootingTime = 0.75f;
    readonly float shootingCooldown = 0.5f;
    public bool shootInput;
    public bool canShoot = true;
    public bool shootCooldown;

   [Header("Melee ATK")]
    bool atkInput;
    private readonly float atkRange = 0.5f;
    public LayerMask enemyLayer;
    public Transform atkPoint;
    public float atkRate = 2f;
    float nextAtkTime = 0f;
    public bool isAtking;

    [Header("Player Health")]
    public int playerHealth = 3;
    private SpriteRenderer sprite;
    private Color normalColor;
    private Color normalColorLives;

    //Criar um Script separado para a UI seria o Ideal (eu acho)

    [Header("Lives and Leafs")]
    public GameObject Life1;
    public GameObject Life2;
    public GameObject Life3;
    public GameObject Leaf1;
    public GameObject Leaf2;
    public GameObject Leaf3;
    public bool leafInput;
    private int leafs = 0; 

    [Header("Knockback")]
    private readonly float kbForceX= 14f;
    private readonly float kbForceY = 6f;
    private bool isKb = false;

    [Header("Base Components")]
    public GameObject player; 
    [SerializeField] private Transform InteractableCheck;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform trans;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private Animator animator;
    [SerializeField] private CapsuleCollider2D playerCollider;
    private GameObject goThroughPlatform;
    public ManaSystem manaSystem;

    Vector2 vertical;

    enum State { Idle, Run, Jump, Glide, Dash, WallSlide, WallJump, Atk, Shoot, TakeDamage, DoNothing }

    State state = State.Idle;

    private void Awake()
    {
        manaSystem = GetComponent<ManaSystem>();
        animator = GetComponent<Animator>();
        trans = GetComponent<Transform>();
        sprite = GetComponent<SpriteRenderer>();
        normalColor = sprite.color;
        normalColorLives = Life1.GetComponent<Image>().color;
    }

    private void Update()
    {
        jumpInput = Input.GetKey(KeyCode.Space);
        horizontalInput = Input.GetAxisRaw("Horizontal");
        dashInput = Input.GetKey(KeyCode.LeftShift);
        shootInput = Input.GetKeyDown(KeyCode.L);
        atkInput = Input.GetKey(KeyCode.K);
        leafInput = Input.GetKeyDown(KeyCode.Q);

        if (canMove)
        {
            speed = 8;
        }
        else
        {
            speed = 0;
        }

        if (isFacingRight)
        {
            facing = -1;
        }
        else
        {
            facing = 1;
        }

        //Fazer com que segurar o JumoInput não faça o player pular infinitamente
        if (Input.GetKeyUp(KeyCode.Space))
        {
            canJump = true;
        }

        //Seria interessante crira mais um estado para o curar
        if (leafInput && leafs > 0 && playerHealth < 3)
        {
            Heal();
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }

        switch (state)
        {
            case State.Idle: IdleState(); break;
            case State.Run: RunState(); break;
            case State.Jump: JumpState(); break;
            case State.Glide: GlideState(); break;
            case State.Dash: Dash(); break;
            case State.WallSlide: WallSlide(); break;
            case State.WallJump: WallJump(); break;
            case State.Shoot: Shoot(); break;
            case State.Atk: Atk(); break;
            case State.TakeDamage: TakeDamage(); break;
            case State.DoNothing: DoNothing(); break;
        }

        moveDirection = new Vector2(horizontalInput, 0).normalized;

    }

    void IdleState()
    {
        animator.Play("Idle");

        if (canDash && dashInput && horizontalInput != 0)
        {
            state = State.Dash;
        }
        else if (!IsGrounded())
        {
            state = State.Glide;
        }

        if (IsGrounded())
        {
            if (jumpInput && canJump)
            {
                state = State.Jump;
            }
            else if (horizontalInput != 0f && rb.velocity.y == 0)
            {
                state = State.Run;
            }
            else if (atkInput)
            {
                state = State.Atk;
            }
            else if (doNothing == true)
            {
                state = State.DoNothing;
            }
            if (shootInput && manaSystem.currentMana > shootCost && canShoot && !shootCooldown)
            {
                state = State.Shoot;
            }
        }
    }

    void RunState()
    {
        animator.Play("Run");

        Flip();

        rb.velocity = new Vector2(horizontalInput * speed, rb.velocity.y);

        if (canDash && dashInput && horizontalInput != 0)
        {
            state = State.Dash;
        }
        else if (IsWalled() && !IsGrounded())
        {
            state = State.WallSlide;
        }
        else if (!IsGrounded())
        {
            state = State.Glide;
        }

        if (IsGrounded())
        {
            if (jumpInput && canJump)
            {
                state = State.Jump;
            }
            else if (horizontalInput == 0f && rb.velocity.y == 0 && rb.velocity.x == 0)
            {
                state = State.Idle;
            }
            else if (atkInput)
            {
                rb.velocity = Vector2.zero;
                state = State.Atk;
            }
            else if (doNothing == true)
            {
                state = State.DoNothing;
            }
            if (shootInput && manaSystem.currentMana > shootCost && canShoot && !shootCooldown)
            {
                rb.velocity = Vector2.zero;
                state = State.Shoot;
            }
        }
    }

    void JumpState()
    {
        animator.Play("Jump");

        canJump = false;

        Flip();

        rb.velocity = Vector2.up * jumpingPower;

        rb.velocity = rb.velocity.y * Vector2.up + speed * horizontalInput * Vector2.right;

        if (horizontalInput != 0f && IsGrounded() && rb.velocity.y == 0)
        {
            state = State.Run;
        }
        else if (canDash && dashInput && horizontalInput != 0)
        {
            state = State.Dash;
        }
        else if (IsWalled() && !IsGrounded())
        {
            state = State.WallSlide;
        }

        state = State.Glide;
    }

    private IEnumerator DisableCollision()
    {
        BoxCollider2D platformCollider = goThroughPlatform.GetComponent<BoxCollider2D>();

        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
        yield return new WaitForSeconds(0.25f);
        Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
    }

    void GlideState()
    {
        Flip();

        rb.velocity = rb.velocity.y * Vector2.up + speed * horizontalInput * Vector2.right;

        if (rb.velocity.y < -0.2f)
        {
            animator.Play("Fall");
        }

        if (canDash && dashInput && horizontalInput != 0)
        {
            state = State.Dash;
        }
        else if (IsWalled() && !IsGrounded())
        {
            state = State.WallSlide;

        }
        else if (atkInput)
        {
            state = State.Atk;
        }

        if (IsGrounded())
        {
            if (horizontalInput != 0f && rb.velocity.y == 0)
            {
                state = State.Run;
            }
            else if (horizontalInput == 0f && rb.velocity.y == 0 && rb.velocity.x == 0)
            {
                state = State.Idle;
            }

        }
    }

    private void Flip()
    {
        if (isFacingRight && horizontalInput < 0f || !isFacingRight && horizontalInput > 0f)
        {
            isFacingRight = !isFacingRight;
            transform.Rotate(0, 180, 0);
        }
    }

    void Dash()
    {
        animator.Play("Dash");

        Flip();

        StartCoroutine(DashTempo());

        if (IsGrounded())
        {
            if (jumpInput && canJump)
            {
                state = State.Jump;
            }
            else if (horizontalInput != 0f && rb.velocity.y == 0)
            {
                state = State.Run;
            }
            else if (horizontalInput != 0f)
            {
                state = State.Run;
            }
            else if (doNothing == true)
            {
                state = State.DoNothing;
            }
        }
        else if (!IsGrounded())
        {
            state = State.Glide;
        }
    }

    public IEnumerator DashTempo()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(moveDirection.x * dashingPower, 0f);
        trail.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        trail.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    void WallSlide()
    {
        animator.Play("WallSlide");
   
        rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));

        if (horizontalInput == facing)
        {
            state = State.Glide;
        }
        else if (jumpInput && canWallJump)
        {
            state = State.WallJump;
        }
        else if (!IsGrounded() && !IsWalled())
        {
            state = State.Glide;
        }
    }

    void WallJump()
    {
        animator.Play("WallJump");

        canWallJump = false;
        isWallJumping = true;
        wallJumpingDirection = facing;
        wallJumpingCounter = wallJumpingTime;
        wallJumpingCounter -= Time.deltaTime;

        if (transform.localScale.x != wallJumpingDirection)
        {
            isFacingRight = !isFacingRight;
            transform.Rotate(0, 180, 0);
        }

        rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);

        Debug.Log(rb.velocity);

        wallJumpingCounter = 0f;

        Invoke(nameof(StopWallJump), wallJumpingDuration);

        StartCoroutine(WallJumpDelay());

        if (!IsGrounded())
        {
            state = State.Glide;
        }

        if (!isWallJumping)
        {
            if (canDash && dashInput && horizontalInput != 0)
            {
                state = State.Dash;
            }
            else if (IsWalled() && !IsGrounded())
            {
                state = State.WallSlide;
            }
            else if (!IsGrounded())
            {
                state = State.Glide;
            }
        }
    }

    IEnumerator WallJumpDelay()
    {
        yield return new WaitForSeconds(wallJumpCooldown);
        canWallJump = true;
    }

    void StopWallJump()
    {
        isWallJumping = false;
    }

    public bool IsGrounded()
    {
        return Physics2D.OverlapBox(groundCheck.position, new Vector2(0.1f, 0.1f) ,0.01f, groundLayer);
    }

    public bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.1f, wallLayer);
    }

    void Shoot()
    {
        animator.Play("Shoot");

        manaSystem.UseAbility(shootCost);

        StartCoroutine(ShootDelay());

        if (jumpInput && canJump)
        {
            state = State.Jump;
        }
        else if (horizontalInput != 0f && rb.velocity.y == 0)
        {
            state = State.Run;
        }
        else if (horizontalInput == 0f && rb.velocity.y == 0 && rb.velocity.x == 0)
        {
            state = State.Idle;
        }
       else if (shootInput && manaSystem.currentMana > shootCost && canShoot)
       {
            state = State.Shoot;
       }
       else if (!IsGrounded())
       {
            state = State.Glide;
       }
    }

    public IEnumerator ShootDelay()
    {
        shootCooldown = true;
        canMove = false;
        shootingPoint.rotation = gameObject.transform.rotation;
        Instantiate(bulletPrefab, shootingPoint.position, shootingPoint.rotation);
        yield return new WaitForSeconds(shootingTime);
        shootCooldown = false;
        canMove = true;
    }

    private void Atk()
    {  
        if (Time.time >= nextAtkTime)
        {

            StartCoroutine(ATK());

            nextAtkTime = Time.time + 1f / atkRate;
        }

        else if (horizontalInput == 0f && isAtking == false && rb.velocity.y == 0 && rb.velocity.x == 0)
        {
            state = State.Idle;
        }
        else if (horizontalInput != 0f && isAtking == false && rb.velocity.y == 0)
        {
            state = State.Run;
        }
        else if (atkInput)
        {
            state = State.Atk;
        }
        else if (!IsGrounded() && !isAtking)
        {
            state = State.Glide;
        }
    }

    IEnumerator ATK()
    {
        isAtking = true;

        animator.Play("Atk");

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(atkPoint.position, atkRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {   
            if(enemy.CompareTag("Enemy"))
            {
                enemy.GetComponent<EnemyFSM>().TakeDamage(20);
            }
            else if (enemy.CompareTag("Boss"))
            {
                enemy.GetComponent<Boss>().TakeDamage(20);
            }
        }

        yield return new WaitForSeconds(0.3f);

        isAtking = false;

    }

    void OnDrawGizmosSelected()
    {
        if (atkPoint == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(atkPoint.position, atkRange);

        Gizmos.DrawWireSphere(groundCheck.position, atkRange);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            playerHealth--;

            if (playerHealth == 2)
            {
                Life1.SetActive(false);
                Life2.GetComponent<Image>().color = Color.yellow;
                Life3.GetComponent<Image>().color = Color.yellow;
            }
            else if (playerHealth == 1)
            {
                Life2.SetActive(false);
                Life3.GetComponent<Image>().color = Color.red;
            }
            else if (playerHealth == 0)
            {
                Life3.SetActive(false);
            }

            state = State.TakeDamage;   
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("GoThroughPlatform"))
        {
            goThroughPlatform = null;
        }
    }

    public void TakeDamage()
    {
       // playerHealth--;

        StartCoroutine(Damage());

        isKb = true;
        
        //A partir daqui é para dar Knockback no Player e ver se ele ainda tem vidas

        if (playerHealth <= 0)
        {
            Die();
        }
        if (isKb)
        {
            if(isFacingRight) 
            {
                rb.velocity = new Vector2(-kbForceX, kbForceY);
                isKb = false;
            }
            else if (!isFacingRight)
            {
                rb.velocity = new Vector2(kbForceX, kbForceY);
                isKb = false;
            }
        }

        Invoke(nameof(StopKB), 0.15f);
    }

    public IEnumerator Damage()
    {
        animator.Play("TakeDamage");

        for ( int i = 0; i < 2; i++ ) 
        {
            sprite.color = new Color(0.86f, 0.4f, 0.4f, 0.90f);

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
        state = State.Glide;   
    }

    void Die()
    {
        animator.Play("Death");
        Destroy(gameObject);
    }

    public void AddLeaf()
    {
        leafs++;

        if (leafs == 1)
        {
            Leaf1.SetActive(true);
        }
        else if (leafs == 2)
        {
            Leaf2.SetActive(true);
        }
        else if (leafs == 3)
        {
            Leaf3.SetActive(true);
        }

        if (leafs > 3)
        {
            leafs--;
        }
    }

    void Heal()
    {

        if (leafs == 3)
        {
            Leaf3.SetActive(false);
        }
        else if (leafs == 2)
        {
            Leaf2.SetActive(false);
        }
        else if (leafs == 1)
        {
            Leaf1.SetActive(false);
        }

        leafs--;

        playerHealth++;

        if (playerHealth == 3)
        {
            Life1.SetActive(true);
            Life2.GetComponent<Image>().color = normalColorLives;
            Life3.GetComponent<Image>().color = normalColorLives;
        }
        else if (playerHealth == 2)
        {
            Life2.SetActive(true);
            Life2.GetComponent<Image>().color = Color.yellow;
            Life3.GetComponent<Image>().color = Color.yellow;
        }
    }

    void DoNothing()
    {
        rb.velocity = Vector2.zero;
        animator.Play("DoNothing");

        if (!doNothing)
        {
            StartCoroutine(DoSomething());
        }
    }

    IEnumerator DoSomething()
    {
        yield return new WaitForSeconds(0.15f);
        state = State.Idle;
    }
}

