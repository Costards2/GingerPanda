using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveFSM : MonoBehaviour
{
    private float horizontalInput = 0f;
    private float speed = 8f;
    private float jumpingPower = 16f;
    private bool isFacingRight = true;

    private bool isWallSliding;
    private float wallSlidingSpeed = 2f;

    private bool isWallJumping;
    private float wallJumpingDirection;
    private readonly float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private readonly float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(8f, 16f);
    private float rot;
    public float facing = 0;

    public bool canDash = true;
    private bool isDashing;
    bool jumpInput = false;
    public bool dashInput = false;
    public float dashingPower = 14f;
    public float dashingTime = 0.2f;
    public float dashingCooldown = 1f;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform trans;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private Animator animator;

    Vector2 moveDirection;

    enum State { Idle, Run, Jump, Glide, Dash, WallSlide, WallJump }

    State state = State.Idle;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        trans = GetComponent<Transform>();
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
    }

    void FixedUpdate()
    {
        jumpInput = Input.GetKey(KeyCode.Space);
        horizontalInput = Input.GetAxisRaw("Horizontal");
        dashInput = Input.GetKeyDown(KeyCode.LeftShift);

        //Debug.Log(dashInput); 
            
        if (isDashing)
        {
            return;
        }

        Flip();

        switch (state)
        {
            case State.Idle: IdleState(); break;
            case State.Run: RunState(); break;
            case State.Jump: JumpState(); break;
            case State.Glide: GlideState(); break;
            case State.Dash: Dash(); break;
            case State.WallSlide: WallSlide(); break;
            case State.WallJump: WallJump(); break;
        }

        moveDirection = new Vector2(horizontalInput, 0).normalized;
    }

    void IdleState()
    {
        animator.Play("Idle");

        if (canDash && dashInput && horizontalInput != 0) //N faz sentido mas mantenha (Confie em min)
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
        }
        else if (horizontalInput != 0f)
        {
            state = State.Run;

        }
    }

    void RunState()
    {
        animator.Play("Run");

        rb.velocity = new Vector2(horizontalInput * speed, rb.velocity.y);

        if (canDash && dashInput && horizontalInput != 0)
        {
            state = State.Dash;
        }
        else if (IsWalled() && !IsGrounded())
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
        }
    }

    void JumpState()
    {
        animator.Play("Jump");

        rb.velocity = speed * horizontalInput * Vector2.right + jumpingPower * Vector2.up;

        if (horizontalInput != 0f && IsGrounded())
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

    void GlideState()
    {
        animator.Play("Fall");

        rb.velocity = rb.velocity.y * Vector2.up + speed * horizontalInput * Vector2.right;

        if (canDash && dashInput && horizontalInput != 0)
        {
            state = State.Dash;
        }
        else if (IsWalled() && !IsGrounded())
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

    void Dash()
    {
        animator.Play("Dash");

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
        else if (horizontalInput != 0f && !IsGrounded())
        {
            state = State.Glide;
        }
        else if (IsWalled() && !IsGrounded())
        {
            state = State.WallSlide;
        }

    }

    void WallSlide()
    {
        animator.Play("WallSlide");

        isWallSliding = true;
        rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));

        if (isWallSliding)
        {
            state = State.WallJump;
        }

        if (IsGrounded())
        {
            if (horizontalInput == 0)
            {
                isWallSliding = false;
                state = State.Idle;
            }
            else if (jumpInput)
            {
                isWallSliding = false;
                state = State.Jump;
            }
            else if (horizontalInput != 0f)
            {
                isWallSliding = false;
                state = State.Run;
            }
        }
    }

    void WallJump()
    {
        animator.Play("WallJump");

        isWallJumping = false;
        wallJumpingDirection = facing;
        wallJumpingCounter = wallJumpingTime;

        CancelInvoke(nameof(StopWallJumping));

        wallJumpingCounter -= Time.deltaTime;
        

        if (jumpInput && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;

            Flip();

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }


        if (canDash && dashInput && horizontalInput != 0)
        {
            state = State.Dash;
        }
        else if (IsWalled() && !IsGrounded())
        {
            state = State.WallSlide;
        }
        else if (horizontalInput != 0f && !IsGrounded())
        {
            state = State.Glide;
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
            else if (horizontalInput != 0f)
            {
                state = State.Run;
            }
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    private void Flip()
    {
        if (isFacingRight && horizontalInput < 0f || !isFacingRight && horizontalInput > 0f)
        {
            isFacingRight = !isFacingRight;
            transform.Rotate(0, 180, 0);
        }
    }
    
    public IEnumerator DashTempo ()
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
}
