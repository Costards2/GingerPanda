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

    /*private bool isWallSliding;
    private float wallSlidingSpeed = 2f;

    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(8f, 16f);*/

    public bool canDash = true;
    private bool isDashing;
    bool jumpInput = false;
    public bool dashInput = false;
    public float dashingPower = 14f;
    public float dashingTime = 0.2f;
    public float dashingCooldown = 1f;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private Animator animator;

    Vector2 moveDirection;

    enum State { Idle, Run, Jump, Glide, Dash }

    State state = State.Idle;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        jumpInput = Input.GetKey(KeyCode.Space);
        horizontalInput = Input.GetAxisRaw("Horizontal");
        dashInput = Input.GetKeyDown(KeyCode.LeftShift);
        
        Debug.Log(dashInput); 
            
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
        }

        moveDirection = new Vector2(horizontalInput, 0).normalized;
    }

    void IdleState()
    {
        animator.Play("Idle");

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

        if (IsGrounded())
        {
            if (jumpInput)
            {
                state = State.Jump;
            }
            else if (canDash && dashInput)
            {
                state = State.Dash;
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
        
        state = State.Glide;

        if (horizontalInput != 0f && IsGrounded())
        {
            state = State.Run;
        }
        else if (canDash && dashInput)
        {
            state = State.Dash;
        }
    }

    void GlideState()
    {
        if (jumpInput)
        {
            animator.Play("Jump");
        }
        else
        {
            animator.Play("Fall");
        }

        rb.velocity = rb.velocity.y * Vector2.up + speed * horizontalInput * Vector2.right;


        if (IsGrounded())
        {
            if (horizontalInput != 0f)
            {
                state = State.Run;
            }
            else
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
        }
        else if (horizontalInput != 0f)
        {
            state = State.Run;

        }

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
