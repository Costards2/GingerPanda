using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class MoveFSM : MonoBehaviour
{
    [Header("Base Move")]
    private float horizontalInput = 0f;
    public bool canMove = true;
    [SerializeField] private float speed = 8f;
    private float jumpingPower = 16f;
    private bool isFacingRight = true;
    Vector2 moveDirection;

    [Header("Everything related to Wall")]
    private bool isWallSliding;
    private float wallSlidingSpeed = 2f;
    private bool isWallJumping;
    private float wallJumpingDirection;
    private Vector2 wallJumpingPower = new Vector2(8.5f, 16f);
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
    float shootingTime = 0.75f;
    float shootingCooldown = 0.5f;
    public bool shootInput;

    [Header("Melee ATK")]
    bool atkInput;
    private float atkRange = 0.5f;
    public LayerMask enemyLayer;
    public Transform atkPoint;
    public float atkRate = 2f;
    float nextAtkTime = 0f;
    public bool isAtking;

    [Header("Player Health")]
    public int playerHealth = 3;
    private SpriteRenderer sprite;
    private Color normalColor;

    [Header("Lives and Leafs")]
    public GameObject Life1;
    public GameObject Life2;
    public GameObject Life3;
    public GameObject Leaf1;
    public GameObject Leaf2;
    public GameObject Leaf3;
    public bool leafInput;
    private int leafs = 1; 

    [Header("Knockback")]
    private float kbForceX= 14f;
    private float kbForceY = 6f;
    private bool isKb = false;

    [Header("Base Components")]
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

    public ManaSystem manaSystem;

    public GameObject placeHolderCajado; 

    enum State { Idle, Run, Jump, Glide, Dash, WallSlide, WallJump, Atk, Shoot, TakeDamage }

    State state = State.Idle;

    private void Awake()
    {
        manaSystem = GetComponent<ManaSystem>();
        animator = GetComponent<Animator>();
        trans = GetComponent<Transform>();
        sprite = GetComponent<SpriteRenderer>();
        normalColor = sprite.color;
    }

    private void Update()
    {
        if (isFacingRight)
        {
            facing = -1;
        }
        if (!isFacingRight)
        {
            facing = 1;
        }

        if (horizontalInput != 0 && IsWalled() && !IsGrounded())
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }

        if (canMove)
        {
            speed = 8; 
        }
        else
        {
            speed = 0;
        }

        Debug.Log(playerHealth);
    }

    void FixedUpdate()
    {
        jumpInput = Input.GetKey(KeyCode.Space);
        horizontalInput = Input.GetAxisRaw("Horizontal");
        dashInput = Input.GetKeyDown(KeyCode.LeftShift);
        shootInput = Input.GetKey(KeyCode.E);
        atkInput = Input.GetKey(KeyCode.Mouse0);
        leafInput = Input.GetKey(KeyCode.Q);

        if (leafInput && leafs > 0 && playerHealth < 3)
        {
            Heal();
        }

        //Debug.Log(CanInteract());

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

        if (IsGrounded())
        {
            if (jumpInput)
            {
                state = State.Jump;
            }
            else if (horizontalInput != 0f)
            {
                state = State.Run;
            }
            else if (atkInput)
            {
                state = State.Atk;
            }
            if (Input.GetKeyDown(KeyCode.E) && manaSystem.currentMana > shootCost)
            {
                manaSystem.UseAbility(shootCost);
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
        else if (isWallSliding)
        {
            state = State.WallSlide;
        }

        if (IsGrounded())
        {
            if (jumpInput)
            {
                state = State.Jump;
            }
            else if (horizontalInput == 0f)
            {
                state = State.Idle;
            }
            else if (atkInput)
            {
                state = State.Atk;
            }
            if (Input.GetKeyDown(KeyCode.E) && manaSystem.currentMana > shootCost)
            {
                manaSystem.UseAbility(shootCost);
                state = State.Shoot;
            }
        }
    }

    void JumpState()
    {
        animator.Play("Jump");

        Flip();

        rb.velocity = speed * horizontalInput * Vector2.right + jumpingPower * Vector2.up;

        if (horizontalInput != 0f && IsGrounded())
        {
            state = State.Run;
        }
        else if (canDash && dashInput && horizontalInput != 0)
        {
            state = State.Dash;
        }
        else if (isWallSliding)
        {
            state = State.WallSlide;
        }

        state = State.Glide;
    }

    void GlideState()
    {
        animator.Play("Fall");

        Flip();

        rb.velocity = rb.velocity.y * Vector2.up + speed * horizontalInput * Vector2.right;

        if (canDash && dashInput && horizontalInput != 0)
        {
            state = State.Dash;
        }
        else if (isWallSliding)
        {
            state = State.WallSlide;
        }

        if (IsGrounded())
        {
            if (horizontalInput != 0f)
            {
                state = State.Run;
            }
            else if (horizontalInput == 0f)
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
            if (jumpInput)
            {
                state = State.Jump;
            }
            else if (horizontalInput != 0f)
            {
                state = State.Run;
            }
            else if (horizontalInput != 0f)
            {
                state = State.Run;
            }
        }
        else if (/* horizontalInput != 0f && */ !IsGrounded())
        {
            state = State.Glide;
        }
        else if (isWallSliding)
        {
            state = State.WallSlide;
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

        if (isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }

        if (isWallSliding && jumpInput && canWallJump)
        {
            state = State.WallJump;
        }
        else if (!IsGrounded() && !isWallSliding && !isWallJumping)
        {
            state = State.Glide;
        }

        if (IsGrounded())
        {
            if (horizontalInput == 0)
            {
                state = State.Idle;
            }
            else if (jumpInput)
            {
                state = State.Jump;
            }
            else if (horizontalInput != 0f)
            {
                state = State.Run;
            }
        }
    }

    void WallJump()
    {
        animator.Play("WallJump");

        wallJumpingDirection = facing;

        isWallJumping = false;
        wallJumpingDirection = facing;
        wallJumpingCounter = wallJumpingTime;

        CancelInvoke(nameof(StopWallJump));

        wallJumpingCounter -= Time.deltaTime;

        canWallJump = false;
        rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
        isWallJumping = true;
        wallJumpingCounter = 0f;

        Invoke(nameof(StopWallJump), wallJumpingDuration);

        StartCoroutine(WallJumpDelay());

        if (canDash && dashInput && horizontalInput != 0)
        {
            state = State.Dash;
        }
        else if (isWallSliding)
        {
            state = State.WallSlide;
        }
        else if (!IsGrounded())
        {
            state = State.Glide;
        }
        if (IsGrounded())
        {
            if (horizontalInput == 0f)
            {
                state = State.Idle;
            }
            else if (horizontalInput != 0f)
            {
                state = State.Run;
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

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }

    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    private void Shoot()
    {
        animator.Play("Shoot");

        StartCoroutine(ShootDelay());

        if (IsGrounded())
        {
            if (jumpInput)
            {
                state = State.Jump;
            }
            else if (horizontalInput != 0f)
            {
                state = State.Run;
            }
            else if (horizontalInput == 0f)
            {
                state = State.Idle;
            }
            if (Input.GetKeyDown(KeyCode.E) &&  manaSystem.currentMana > shootCost)
            {
                manaSystem.UseAbility(shootCost);
                state = State.Shoot;
            }
        }
    }

    public IEnumerator ShootDelay()
    {
        canMove = false;  
        shootingPoint.rotation = gameObject.transform.rotation;
        Instantiate(bulletPrefab, shootingPoint.position, shootingPoint.rotation);
        yield return new WaitForSeconds(shootingTime);
        canMove = true;
    }

    private void Atk()
    {
        
        if (Time.time >= nextAtkTime)
        {
            animator.Play("Atk");
            isAtking = true;

            placeHolderCajado.SetActive(true);

            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(atkPoint.position, atkRange, enemyLayer);

            foreach (Collider2D enemy in hitEnemies)
            {
                enemy.GetComponent<EnemyFSM>().TakeDamage(20);
            }
            isAtking = false;

            nextAtkTime = Time.time + 1f / atkRate;

             Invoke("CajadoDesinvocado", 0.5f);
        }

        if (IsGrounded())
        {
            if (jumpInput)
            {
                state = State.Jump;
            }
            else if (horizontalInput != 0f)
            {
                state = State.Run;
            }
            else if (horizontalInput == 0f)
            {
                state = State.Idle;
            }
            else if (atkInput)
            {
                state = State.Atk;
            }
            if (Input.GetKeyDown(KeyCode.E) && manaSystem.CanAffordAbility(shootCost))
            {
                manaSystem.UseAbility(shootCost);
                state = State.Shoot;
            }

        }
    }

    void OnDrawGizmosSelected()
    {
        if (atkPoint == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(atkPoint.position, atkRange);

        //Gizmos.DrawWireSphere(InteractableCheck.position, 2f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Enemy"))
        {
            playerHealth--;

            if (playerHealth == 2)
            {
                Life1.SetActive(false);
            }
            else if (playerHealth == 1)
            {
                Life2.SetActive(false);
            }
            else if (playerHealth == 0)
            {
                Life3.SetActive(false);
            }
            state = State.TakeDamage;
        }
    }

    private void OnTrigger2DEnter(Collider2D other)
    {
        if (other.gameObject.CompareTag("Leaf") && leafs < 3)
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
        }
    }

    void TakeDamage()
    {
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

        for( int i = 0; i < 2; i++ ) 
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

    /*private bool CanInteract()
    {
        return Physics2D.OverlapCircle(InteractableCheck.position, 2f, interactableLayer);
    }*/

    void CajadoDesinvocado()
    {
        placeHolderCajado.SetActive(false);
    }

    void Heal()
    {

        if (playerHealth == 2)
        {
            Life1.SetActive(true);
        }
        else if (leafs == 1)
        {
            Life2.SetActive(true);
        }

        playerHealth++;

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

    }
}

