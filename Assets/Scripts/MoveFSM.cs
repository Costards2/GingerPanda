using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class MoveFSM : MonoBehaviour
{
    [Header("Base Move")]
    public float horizontalInput = 0f;
    public bool canMove = true;
    [SerializeField] private float speed = 8f;
    private Vector2 newVelocity;
    private float jumpingPower = 16f;
    public bool isFacingRight = true;
    Vector2 moveDirection;
    private bool canJump = true;
    public bool doNothing = false;

    [Header("Everything related to Wall")]
    private readonly bool isWallSliding;
    private readonly float wallSlidingSpeed = 1f;
    private bool isWallJumping;
    private float wallJumpingDirection;
    private Vector2 wallJumpingPower = new(16f, 14f);
    public float facing = 0;
    bool canWallJump = true;
    public float wallJumpTime = 0.3f;
    public float wallJumpCooldown = 0.5f;
    private float wallJumpingCounter;
    private readonly float wallJumpingDuration = 0.2f;
    private readonly float wallJumpingTime = 0.2f;

    [Header("Dash")]
    public bool canDash = true;
    private bool isDashing;
    bool jumpInput = false;
    public bool dashInput = false;
    public float dashingPower = 14f;
    public float dashingTime = 0.35f;
    public float dashingCooldown = 1f;
    bool finishedShooting;

    [Header("Shoot")]
    public float shootCost = 20f;
    public GameObject bulletPrefab;
    public Transform shootingPoint;
    readonly float shootingTime = 0.25f;
    public bool shootInput;
    public float ShootingRate = 0.5f;
    float nextShot = 0f;

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

    //Criar um Script separado para a UI seria o Ideal (eu acho) principalmente com Delegates e Events 

    [Header("Lives and Leafs")]
    public GameObject Life1;
    public GameObject Life2;
    public GameObject Life3;
    public GameObject Leaf1;
    public GameObject Leaf2;
    public GameObject Leaf3;
    public bool leafInput;
    [SerializeField] private int leafs = 0;
    [SerializeField] bool imortal = false;

    [Header("Knockback")]
    private readonly float kbForceX = 14f;
    private readonly float kbForceY = 6f;
    private bool isKb = false;

    [Header("Base Components")]
    public GameObject player;
    public GameObject playerEverything;
    [SerializeField] private Transform InteractableCheck;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform trans;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask Player;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private Animator animator;
    [SerializeField] private CapsuleCollider2D playerCollider;
    [SerializeField] private GameObject goThroughPlatform;
    [SerializeField] public ManaSystem manaSystem;
    [SerializeField] Vector2 damageRB;
    [SerializeField] Vector3 playerCheckPoint;
    [SerializeField] public int collectables = 0;
    [SerializeField] Vector2 vertical;
    [SerializeField] int playerDied;
    [SerializeField] PhysicsMaterial2D noFriction;
    [SerializeField] PhysicsMaterial2D friction;
    private Vector2 colliderSize;
    [SerializeField]
    private float slopeCheckDistance;
    [SerializeField]
    private float slopeCheckDistanceHorizontal;
    [SerializeField]
    private float maxSlopeAngle;
    private float slopeDownAngle;
    private float slopeSideAngleFront;
    private float slopeSideAngleBack;
    private float lastSlopeAngle;
    private Vector2 slopeNormalPerp;
    private bool isOnSlope;
    private float slopeDownAngleOld;
    private bool canWalkOnSlope;
    private Vector2 newForce;

    //bool shouldNotSlope = false; //necessary condition because the Gothrough plataform and Slope were conflicting //I have to chnge It 

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
        playerCheckPoint = transform.position;
        playerCollider = GetComponent<CapsuleCollider2D>();

        colliderSize = playerCollider.size;
    }

    private void Update()
    {
        //SlopeCheck();
        //Debug.Log(state);
        //Debug.Log(shouldNotSlope);
        //Debug.Log("On slope: " + isOnSlope);
        //Debug.Log("Walk on Slope: " + canWalkOnSlope);
        //Debug.Log(slopeDownAngle);
        //Debug.Log(slopeSideAngleFront);
        //Debug.Log(slopeSideAngleBack);
        //Debug.Log();

        //SlopeCheck(); //Colocar nos estados corretos 

        jumpInput = Input.GetKey(KeyCode.Space);
        horizontalInput = Input.GetAxisRaw("Horizontal");
        dashInput = Input.GetKey(KeyCode.LeftShift);
        shootInput = Input.GetKey(KeyCode.L);
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
        moveDirection = new Vector2(horizontalInput, 0).normalized;

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
    }

    #region States

    void IdleState()
    {

        animator.Play("Idle");
        SlopeCheck();

        newVelocity.Set(0, 0);
        rb.velocity = newVelocity;

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
            if (shootInput && manaSystem.currentMana > shootCost)
            {
                state = State.Shoot;
            }
        }
    }

    void RunState()
    {
        SlopeCheck();
        animator.Play("Run");

        //rb.velocity = new Vector2(horizontalInput * speed, rb.velocity.y);
        //newVelocity.Set(speed * horizontalInput, rb.velocity.y);
        //rb.velocity = newVelocity;
        SlopeCheck();
        Flip();

        if (IsGrounded() && !isOnSlope) 
        {
            newVelocity.Set(speed * horizontalInput, 0.0f);
            rb.velocity = newVelocity;
        }
        else if (IsGrounded() && isOnSlope && canWalkOnSlope) 
        {
            newVelocity.Set(speed * slopeNormalPerp.x * -horizontalInput, speed * slopeNormalPerp.y * -horizontalInput);
            rb.velocity = newVelocity;
        }

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
            if (shootInput && manaSystem.currentMana > shootCost && Time.time >= nextShot) // O "Time.time >= nextShot" está aqui se não o player muda de estado para ver se pode atirar
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

        //rb.velocity = Vector2.up * jumpingPower;
        //rb.velocity = rb.velocity.y * Vector2.up + speed * horizontalInput * Vector2.right;
        newVelocity.Set(0.0f, 0.0f);
        rb.velocity = newVelocity;
        newForce.Set(0.0f, jumpingPower);
        //rb.AddForce(newForce, ForceMode2D.Impulse);
        rb.velocity = newForce;

        if (canDash && dashInput && horizontalInput != 0)
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
        SlopeCheck();

        if (!isWallJumping && !IsGrounded() && !isOnSlope) // If necessatio para com que o player consiga completar o WallJump se não ele já entra em queda 
        {
            newVelocity.Set(speed * horizontalInput, rb.velocity.y);
            rb.velocity = newVelocity;
            Flip();
        }

        if (rb.velocity.y < -0.5f && !IsGrounded() && !isOnSlope)
        {
            animator.Play("Fall");
        }

        else if (horizontalInput == 0 && isOnSlope && canWalkOnSlope && IsGrounded())
        {
            animator.Play("Idle");
        }

        if (IsGrounded() && horizontalInput != 0)
        {
            state = State.Run;
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
            if (horizontalInput != 0f && (rb.velocity.y == 0 || isOnSlope && canWalkOnSlope))
            {
                state = State.Run;
            }
            else if (horizontalInput == 0f && rb.velocity.x == 0 && (rb.velocity.y == 0 || isOnSlope && canWalkOnSlope))
            {
                state = State.Idle;
            }

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

    void WallSlide()
    {
        animator.Play("WallSlide");
        rb.velocity = new Vector2(0, -0.7f); //Prevent the player from sliding after jumpin to another wall (Me deu vontade ai escrevi fiz em Inglês)

        rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));

        if (horizontalInput == facing)
        {
            state = State.Glide;
        }
        else if (jumpInput && canWallJump && canJump)
        {
            state = State.WallJump;
        }
        else if (IsGrounded())
        {
            state = State.Idle;
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
            if (isFacingRight)
            {
                sprite.flipX = false;
            }
            else
            {
                sprite.flipX = true;
            }

            playerEverything.transform.Rotate(0, 180, 0);
        }

        rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);

        wallJumpingCounter = 0f;

        Invoke(nameof(StopWallJump), wallJumpingDuration);

        StartCoroutine(WallJumpDelay());

        if (IsWalled() && !IsGrounded())
        {
            state = State.WallSlide;
        }
        else if (!IsGrounded())
        {
            state = State.Glide;
        }

        if (!isWallJumping)
        {
            if (canDash && dashInput && horizontalInput != 0)
            {
                state = State.Dash;
            }
        }
    }

    void StopWallJump()
    {
        isWallJumping = false;
    }

    void Shoot()
    {
        SlopeCheck();

        if (Time.time >= nextShot)
        {
            finishedShooting = false;

            manaSystem.UseAbility(shootCost);

            StartCoroutine(ShootDelay());

            nextShot = Time.time + 1f / ShootingRate;
        }

        else if (horizontalInput == 0f && rb.velocity.x == 0 && finishedShooting && IsGrounded()  && ((canWalkOnSlope && isOnSlope) || !isOnSlope) && rb.velocity.y == 0)
        {
            state = State.Idle;
        }
        else if (horizontalInput != 0f && finishedShooting && IsGrounded() && ((canWalkOnSlope && isOnSlope) || !isOnSlope) && rb.velocity.y == 0)
        {
            state = State.Run;
        }
        else if (atkInput && finishedShooting)
        {
            state = State.Atk;
        }
        else if (!IsGrounded())
        {
            state = State.Glide;
        }
    }

    void Atk()
    {
        SlopeCheck();

        if (Time.time >= nextAtkTime)
        {

            StartCoroutine(ATK());

            nextAtkTime = Time.time + 1f / atkRate;
        }

        else if (horizontalInput == 0f && rb.velocity.x == 0 && finishedShooting && IsGrounded() && ((canWalkOnSlope && isOnSlope) || !isOnSlope) && rb.velocity.y == 0)
        {
            state = State.Idle;
        }
        else if (horizontalInput != 0f && finishedShooting && IsGrounded() && ((canWalkOnSlope && isOnSlope) || !isOnSlope) && rb.velocity.y == 0)
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

    public void TakeDamage()
    {
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

        //A partir daqui é para dar Knockback no Player e ver se ele ainda tem vidas

        isKb = true;

        if (playerHealth <= 0)
        {
            Die();
        }
        if (isKb)
        {
            if (damageRB.x > 0)
            {
                rb.velocity = new Vector2(-kbForceX, kbForceY);
                isKb = false;
            }
            else if (damageRB.x < 0)
            {
                rb.velocity = new Vector2(kbForceX, kbForceY);
                isKb = false;
            }
        }

        StartCoroutine(Imortal());
        StartCoroutine(VisualDamage());

        Invoke(nameof(StopKB), 0.15f);
    }

    void DoNothing()
    {
        SlopeCheck();
        rb.velocity = Vector2.zero;
        animator.Play("DoNothing");

        if (!doNothing)
        {
            StartCoroutine(DoSomething());
        }
    }

    #endregion

    #region Methods

    void Flip()
    {
        if (isFacingRight && horizontalInput < 0f || !isFacingRight && horizontalInput > 0f)
        {
            isFacingRight = !isFacingRight;

            if (isFacingRight)
            {
                sprite.flipX = false;
            }
            else
            {
                sprite.flipX = true;
            }

            playerEverything.transform.Rotate(0, 180, 0);
        }
    }

    public void ReceiveDamage(int damage)
    {
        if (!imortal)
        {
            playerHealth = playerHealth - damage;
            state = State.TakeDamage;
        }
    }

    void StopKB()
    {
        state = State.Glide;
    }

    public void Die()
    {
        playerDied++;

        if (playerDied > 2)
        {
            GameOver();
        }
        else
        {
            animator.Play("Death");

            playerHealth = 3;

            Life1.SetActive(true);
            Life2.SetActive(true);
            Life3.SetActive(true);
            Life2.GetComponent<Image>().color = normalColorLives;
            Life3.GetComponent<Image>().color = normalColorLives;

            state = State.Idle;

            this.transform.position = playerCheckPoint;

            StartCoroutine(Imortal());
        }

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

    void GameOver()
    {
        Debug.Log("GAME OVER!");

        Destroy(gameObject);
    }

    #endregion

    #region Courotines

    IEnumerator WallJumpDelay()
    {
        yield return new WaitForSeconds(wallJumpCooldown);
        canWallJump = true;
    }

    IEnumerator DashTempo()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        //playerCollider.excludeLayers = 8;
        gameObject.layer = LayerMask.NameToLayer("PlayerDashThrough"); //Ailu atravessa inimigos
        rb.velocity = new Vector2(moveDirection.x * dashingPower, 0f);
        trail.emitting = true;

        yield return new WaitForSeconds(dashingTime);

        trail.emitting = false;
        rb.gravityScale = originalGravity;
        gameObject.layer = LayerMask.NameToLayer("Player"); // Ailu não atravessa inimigos 
        //playerCollider.excludeLayers = 0;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    IEnumerator ATK()
    {
        isAtking = true;

        animator.Play("Atk");

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(atkPoint.position, atkRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {

            if (enemy.CompareTag("EnemyFire"))
            {
                enemy.GetComponent<EnemyFSM>().TakeDamage(25);
                Debug.Log("EnemyHit");
            }
            else if (enemy.CompareTag("EnemyGolem"))
            {
                enemy.GetComponent<GolemScript>().TakeDamage(25);
            }
            else if (enemy.CompareTag("Boss"))
            {
                enemy.GetComponent<Boss>().TakeDamage(25);
            }
        }

        yield return new WaitForSeconds(0.3f);

        isAtking = false;

    }

    IEnumerator ShootDelay()
    {
        animator.Play("Shoot");

        shootingPoint.rotation = gameObject.transform.rotation;
        Instantiate(bulletPrefab, shootingPoint.position, shootingPoint.rotation);

        yield return new WaitForSeconds(shootingTime);

        finishedShooting = true;
    }

    IEnumerator DisableCollision()
    {
        BoxCollider2D platformCollider = goThroughPlatform.GetComponent<BoxCollider2D>();

        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
        yield return new WaitForSeconds(0.25f);
        Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
    }

    IEnumerator VisualDamage()
    {
        animator.Play("TakeDamage");

        for (int i = 0; i < 2; i++)
        {
            //sprite.color = new Color(0.86f, 0.4f, 0.4f, 0.90f); Esse é o vermelho

            sprite.color = new Color(1, 1, 1, 0.01f);

            //sprite.enabled = true;

            yield return new WaitForSeconds(0.15f);

            sprite.color = normalColor;

            //sprite.enabled = false;

            yield return new WaitForSeconds(0.15f);

            //sprite.enabled = true;
        }
    }

    IEnumerator Imortal()
    {
        imortal = true;
        gameObject.layer = LayerMask.NameToLayer("PlayerDashThrough"); //Ailu atravessa inimigos
        yield return new WaitForSeconds(0.75f);
        gameObject.layer = LayerMask.NameToLayer("Player"); // Ailu não atravessa inimigos
        imortal = false;
    }

    IEnumerator DoSomething()
    {
        yield return new WaitForSeconds(0.15f);
        state = State.Idle;
    }

    #endregion

    #region Checkers

    public bool IsGrounded()
    {
        return Physics2D.OverlapBox(groundCheck.position, new Vector2(0.25f, 0.25f), 0.25f, groundLayer);
    }

    public bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.1f, wallLayer);
    }

    private void SlopeCheck()
    {
        Vector2 checkPos = transform.position - (Vector3)(new Vector2(0.0f, colliderSize.y / 4));

        SlopeCheckHorizontal(checkPos);
        SlopeCheckVertical(checkPos);
    }

    private void SlopeCheckHorizontal(Vector2 checkPos)
    {
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, transform.right, slopeCheckDistanceHorizontal, groundLayer);
        RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -transform.right, slopeCheckDistanceHorizontal, groundLayer);

        if (slopeHitFront)
        {
            isOnSlope = true;

            slopeSideAngleFront = Vector2.Angle(slopeHitFront.normal, Vector2.up);

        }
        else if (slopeHitBack)
        {
            isOnSlope = true;

            slopeSideAngleBack = Vector2.Angle(slopeHitBack.normal, Vector2.up);
        }
        else
        {
            slopeSideAngleFront = 0.0f;
            slopeSideAngleBack = 0.0f;
            isOnSlope = false;
        }

        //RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, transform.right, slopeCheckDistance, groundLayer);
        //RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -transform.right, slopeCheckDistance, groundLayer);

        //if (slopeHitFront)
        //{
        //    float slopeFrontAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);

        //    if (slopeFrontAngle < maxSlopeAngle)
        //    {
        //        isOnSlope = true;

        //        slopeSideAngleFront = Vector2.Angle(slopeHitFront.normal, Vector2.up);
        //    }

        //}
        //else if (slopeHitBack)
        //{
        //    float slopeBackAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
        //    if (slopeBackAngle < maxSlopeAngle)
        //    {
        //        isOnSlope = true;

        //        slopeSideAngleBack = Vector2.Angle(slopeHitBack.normal, Vector2.up);
        //    }
        //}
        //else
        //{
        //    slopeSideAngleFront = 0.0f;
        //    slopeSideAngleBack = 0.0f;;
        //    isOnSlope = false;
        //}

    }

    private void SlopeCheckVertical(Vector2 checkPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, slopeCheckDistance, groundLayer);

        if (hit)
        {
            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;

            slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            if (slopeDownAngle != lastSlopeAngle)
            {
                isOnSlope = true;
            }

            lastSlopeAngle = slopeDownAngle;

            Debug.DrawRay(hit.point, hit.normal, Color.green);
            Debug.DrawRay(hit.point, slopeNormalPerp, Color.red);

            //if (slopeDownAngle > maxSlopeAngle || slopeSideAngle > maxSlopeAngle)
            //{
            //    canWalkOnSlope = false;
            //}
            //else
            //{
            //    canWalkOnSlope = true;
            //}

            if (slopeDownAngle < maxSlopeAngle && (slopeSideAngleFront < maxSlopeAngle || slopeSideAngleBack < maxSlopeAngle)) 
            {
                canWalkOnSlope = true;
            }
            else
            {
                canWalkOnSlope = false;
            }

            if (isOnSlope && canWalkOnSlope && horizontalInput == 0 && !jumpInput && (slopeDownAngle < maxSlopeAngle))
            {
                //Debug.Log("FRICTION");
                playerCollider.sharedMaterial = friction;
            }
            else
            {
                //Debug.Log("NO FRICTION");
                playerCollider.sharedMaterial = noFriction;
            }
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Better use layer next time
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("EnemyFire") || collision.gameObject.CompareTag("EnemyGolem") || collision.gameObject.CompareTag("Projectile") || collision.gameObject.CompareTag("BossVine"))
        {
            damageRB = player.transform.InverseTransformPoint(collision.transform.position);//You can use Transform.InverseTransformPoint to find the enemy's relative position from the perspective of the player.
                                                                                            //This Vector2 damageRB is a vector that describes the enemy's position offset from the player's position along the player's left/right, up/down, and forward/back axes.
            ReceiveDamage(1);

        }

        //if (collision.gameObject.CompareTag("GoThroughPlatform"))
        //{
        //    shouldNotSlope = true;
        //}
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        //if (collision.gameObject.CompareTag("GoThroughPlatform"))
        //{
        //    //shouldNotSlope = true;
        //}
        //else
        //{
        //    //shouldNotSlope = false;
        //}

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("CheckPoint"))
        {
            Debug.Log("SpawnChanged");
            //Vector3 checkpoint = new Vector3();
            //checkpoint = collision.transform.position;
            playerCheckPoint = new Vector3( collision.transform.position.x ,collision.transform.position.y, -5);
            collision.gameObject.tag = "AlredyChecked";
        }

        if (collision.gameObject.CompareTag("Collectable"))
        {
            Debug.Log("Coletado");
            collectables++;
            collision.gameObject.SetActive(false);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        //if (collision.gameObject.CompareTag("GoThroughPlatform"))
        //{
        //    goThroughPlatform = null;
        //    shouldNotSlope = false;
        //}
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

    #endregion
}

