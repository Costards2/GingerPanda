using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static Mana;

public class MoveFSM : MonoBehaviour
{
    private float horizontalInput = 0f;
    private float speed = 8f;
    private float jumpingPower = 16f;
    private bool isFacingRight = true;

    private bool isWallSliding;
    private float wallSlidingSpeed = 2f;
    private float wallSlideDelay = 1f;

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

    public bool canDash = true;
    private bool isDashing;
    bool jumpInput = false;
    public bool dashInput = false;
    public float dashingPower = 14f;
    public float dashingTime = 0.2f;
    public float dashingCooldown = 1f;

    public ManaPlay mana;
    public GameObject bulletPrefab;
    public Transform shootingPoint;
    private float manaMana;
    //bool canShoot = true;
    bool isShooting = false;
    float shootingTime = 0.75f;
    float shootingCooldown = 1f;
    public bool canShoot = true;
    public bool shootInput;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform trans;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private Animator animator;

    Vector2 moveDirection;

    enum State { Idle, Run, Jump, Glide, Dash, WallSlide, WallJump, Atk, Shoot }

    State state = State.Idle;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        trans = GetComponent<Transform>();
        mana = new ManaPlay();
    }

    private void Update()
    {
        mana.Update();

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
    }

    void FixedUpdate()
    {
        jumpInput = Input.GetKey(KeyCode.Space);
        horizontalInput = Input.GetAxisRaw("Horizontal");
        dashInput = Input.GetKeyDown(KeyCode.LeftShift);
        shootInput = Input.GetKey(KeyCode.E);

        Debug.Log(isWallJumping); 
            
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
            if ( shootInput && /* mana.manaAmount > 20 && */ canShoot)
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
            else if (shootInput && /* mana.manaAmount > 20 && */ canShoot)
            {
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

        if(isWallSliding && jumpInput && canWallJump)
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
        rb.velocity = new Vector2( wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
        isWallJumping = true;
        wallJumpingCounter = 0f;

        Invoke(nameof(StopWallJump), wallJumpingDuration);

        StartCoroutine(WallJumpDelay());

        //canWallJump = true;

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
            if (Input.GetKeyDown(KeyCode.E) && mana.manaAmount > 20)
            {
                state = State.Shoot;
            }
        }
    }

    public IEnumerator ShootDelay ()
    {
        canShoot = false;
        isShooting = true;
        float originalSpeed = speed; 
        speed = 0f;
        //mana.TrySpend(20);
        shootingPoint.rotation = gameObject.transform.rotation;
        Instantiate(bulletPrefab, shootingPoint.position, shootingPoint.rotation);
        yield return new WaitForSeconds(shootingTime);
        isShooting = false;
        speed = originalSpeed;
        yield return new WaitForSeconds(shootingCooldown);
        canShoot = true;
    }

    public class ManaPlay
    {
        public const int MANA_MAX = 100;

        public float manaAmount;
        public float manaRegenAmount;

        public ManaPlay()
        {

            manaAmount = 0;
            manaRegenAmount = 7.5f;
        }

        public void Update()
        {
            manaAmount += manaRegenAmount * Time.deltaTime;
            manaAmount = Mathf.Clamp(manaAmount, 0f, MANA_MAX);
        }

        public void TrySpend(int amount)
        {
            if (manaAmount >= amount)
            {
                manaAmount -= amount;
            }
        }

        public float GetManaNormalized()
        {
            return manaAmount / MANA_MAX;
        }
    }

    private void Atk()
    {

    }
}
