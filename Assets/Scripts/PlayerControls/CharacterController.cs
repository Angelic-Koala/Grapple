using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.Animations;

public class CharacterController : MonoBehaviour
{
    //Animation Bools
    public bool isWalking = false;
    public Animator playerAnimator;

    //Movement inputs
    [SerializeField] private Collider2D m_PlatformCheck;
    public bool collided = false;
    [SerializeField]private bool isWallJumping = false;
    [SerializeField] public int direction = 1;

    public float runSpeed = 40f;

    public float horizontalMove = 0f;
    bool jump = false;
    public bool crouch = false;
    bool canDash = true;
    public bool grappling = false;
    public bool canGrapple = false;


    //public Transform player;
    private PlatformDash dash;

    [SerializeField] private float m_JumpForce = 400f;                          // Amount of force added when the player jumps.
    [SerializeField] private float m_dashForce = 15f;
    //[Range(0, 40)][SerializeField] private float m_wallJumpAngle = 20f;                                    // wall sliding -----------------------------------
    [Range(0, 5)][SerializeField] private float m_fastFallTime = 0.5f;
    [Range(0, 1)][SerializeField] private float m_CrouchSpeed = .36f;           // Amount of maxSpeed applied to crouching movement. 1 = 100%
    [Range(0, .3f)][SerializeField] private float m_GroundedMovementSmoothing = .05f;   // How much to smooth out the movement
    [Range(0, .3f)][SerializeField] private float m_ArielMovementSmoothing = 0.15f;
    [SerializeField] private bool m_AirControl = false;                         // Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
    //[SerializeField] private LayerMask m_WhatIsWall;                                    // wall sliding -----------------------------------
    [SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
    [SerializeField] private Transform m_CeilingCheck;                          // A position marking where to check for ceilings
    [SerializeField] private Transform m_WallCheck;                                     // wall sliding -----------------------------------
    [SerializeField] private Collider2D m_CrouchDisableCollider;                // A collider that will be disabled when crouching

    [SerializeField] private float airJumps = 2f;
    [SerializeField] private float maxJumps = 2f;
    [SerializeField] private float gravity;
    public float dashCooldown = 2f;
    public float dashTimer = 2f;
    public float wallCooldown = 0.4f;
    public float wallTimer = 0.4f;
    private bool apex;
    private bool m_dashing = false;
    public bool m_wallSliding;                                     // wall sliding -----------------------------------
    private bool flipped;

    const float k_GroundedRadius = .1f; // Radius of the overlap circle to determine if grounded
    const float k_WallRadius = .1f;
    [SerializeField] private bool m_Grounded;            // Whether or not the player is grounded.
    [SerializeField] private bool m_fastFall;
    const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up
    public Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true;  // For determining which way the player is currently facing.
    private Vector2 m_Velocity = Vector2.zero;

    /*[Header("Events")]
    [Space]

    public UnityEvent OnLandEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public BoolEvent OnCrouchEvent;*/
    private bool m_wasCrouching = false;

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        dash = GetComponentInChildren<PlatformDash>();

        /*if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();*/
    }

    private void FixedUpdate()
    {
        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        if(m_Rigidbody2D.velocity.y <= 0 && !grappling)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject && !colliders[i].gameObject.GetComponent<Collider2D>().isTrigger)
                {
                    if (!canGrapple)
                    {
                        Debug.Log("Grapple Reset");
                    }
                    isWallJumping = false;
                    m_Grounded = true;
                    airJumps = maxJumps;
                    canGrapple = true;
                    /*if (!wasGrounded)
                        OnLandEvent.Invoke();*/
                }
            }
            Collider2D[] wallColliders = Physics2D.OverlapCircleAll(m_WallCheck.position, k_WallRadius);
            for (int e = 0; e < wallColliders.Length; e++)
            {
                if (wallColliders[e].gameObject != gameObject && wallColliders[e].gameObject.layer == 3)
                {
                    m_wallSliding = true;
                    isWallJumping = false;
                    m_Rigidbody2D.gravityScale = 1;
                    if(airJumps < 1)
                    airJumps = 1;
                    dashTimer = 2;                                    // wall sliding -----------------------------------
                }
                else if (wallColliders[e].gameObject.layer != 3)
                {
                    m_wallSliding = false;
                }
            }
        }

        //movement
        Move(horizontalMove * Time.fixedDeltaTime, crouch, jump, m_wallSliding);
        jump = false;
    }
                        // handles gravity and cooldowns----------------------------------
    private void Update()
    {
        //Animations
        if(Mathf.Abs(m_Rigidbody2D.velocity.x) >= 0.1)
        {
            isWalking = true;
            playerAnimator.SetBool("isWalking", true);
        }
        else
        {
            playerAnimator.SetBool("isWalking", false);
        }

        //Inputs
        /*if (isWallJumping & !flipped)
        {
            horizontalMove *= -1;
            flipped = true;
        }
        else if (!isWallJumping)
        {
            flipped = false;*/
            horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
        //}

        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
        }

        if (Input.GetButton("Crouch"))
        {
            crouch = true;
        }
        else
        {
            crouch = false;
        }

        if (Input.GetButtonDown("Dash"))
        {
            if (collided || canDash)
            {
                StartCoroutine(NewDash(horizontalMove));
            }
        }
        //End of inputs

        if (m_dashing && dashTimer > 0.3f)
            m_dashing = false;

        if (isWallJumping && wallTimer >= wallCooldown)
            isWallJumping = false;
        
        if(!m_wallSliding)
        {
                                                         // fastfalling
            if (Input.GetButtonDown("Crouch"))
            {
                m_fastFall = true;
            }
            else if (Input.GetButtonUp("Crouch"))
            {
                m_fastFall = false;
                m_Rigidbody2D.gravityScale = 2f;
            }

            if (m_fastFall && m_Rigidbody2D.velocity.y <= m_fastFallTime)
                m_Rigidbody2D.gravityScale = 7;
            else if (Mathf.Abs(m_Rigidbody2D.velocity.y) <= 1)
            {
                if (!apex)
                    m_Rigidbody2D.gravityScale = 1.7f;
                apex = true;

            }
            else //if(m_Rigidbody2D.velocity.y <=0)
            {
                apex = false;
                m_Rigidbody2D.gravityScale = 2.5f;
            }
        }

        if (Input.GetButtonUp("Jump") && m_Rigidbody2D.velocity.y > 0f)
        {
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, m_Rigidbody2D.velocity.y * 0.5f);
        }
        


        // cooldowns
        dashTimer += Time.deltaTime;
        wallTimer += Time.deltaTime;
        gravity = m_Rigidbody2D.gravityScale;
    }


    public void Move(float move, bool crouch, bool jump, bool wallSliding)
    {
        // If crouching, check to see if the character can stand up
        /*if (!crouch)
        {
            // If the character has a ceiling preventing them from standing up, keep them crouching
            if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
            {
                crouch = true;
            }
        }*/

        //only control the player if grounded or airControl is turned on
        if (m_Grounded || m_AirControl)
        {
            // If crouching
            if (crouch && m_Grounded)
            {
                if (!m_wasCrouching)
                {
                    m_wasCrouching = true;
                    //OnCrouchEvent.Invoke(true);
                }

                // Reduce the speed by the crouchSpeed multiplier
                move *= m_CrouchSpeed;

                // Disable one of the colliders when crouching
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = false;
            }
            else
            {
                // Enable the collider when not crouching
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = true;

                if (m_wasCrouching)
                {
                    m_wasCrouching = false;
                    //OnCrouchEvent.Invoke(false);
                }
            }

            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * 15f, m_Rigidbody2D.velocity.y);
            if (isWallJumping)
            {
                targetVelocity.x = Mathf.Clamp(targetVelocity.x, 2, runSpeed) * direction;
            }
            if (m_wallSliding)
            {
                targetVelocity.y = Mathf.Clamp(targetVelocity.y, 0, 100);
            }
            else if (m_fastFall)
            {
                targetVelocity.y = Mathf.Clamp(targetVelocity.y, -10, 100);
            }
            else
            {
                targetVelocity.y = Mathf.Clamp(targetVelocity.y, -5, 100);
            }
                


            // And then smoothing it out and applying it to the character
            if (m_Grounded)
            {
                m_Rigidbody2D.velocity = Vector2.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_GroundedMovementSmoothing);
            }
            else
            {
                m_Rigidbody2D.velocity = Vector2.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_ArielMovementSmoothing);
            } 

            // If the input is moving the player right and the player is facing left...
            if (move > 0 && !m_FacingRight)
            {
                // ... flip the player.
                if(m_Grounded)
                    Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0 && m_FacingRight)
            {
                // ... flip the player.
                
                if(m_Grounded)
                    Flip();
            }
        }
        // If the player should jump...
        if(wallSliding && jump)
        {
            Flip();
            isWallJumping = true;
            wallTimer = 0;
            m_Rigidbody2D.velocity = new Vector2(move * -15, m_JumpForce - 5);
        }
        else if(airJumps > 0 && jump)
        {
            isWallJumping = false;
            // Add a vertical force to the player.
            m_fastFall = false;
            //m_Rigidbody2D.gravityScale = 2.5f;
            airJumps--; Debug.Log("Jumped");
            m_Grounded = false;
            m_CrouchDisableCollider.enabled = true;
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, m_JumpForce); //(new Vector2(0f, m_JumpForce));
        }
    }

    public void Dash(float direction)
    {
        dashTimer = 0;
        m_dashing = true;
        if (dash.colliding)
        {
            m_Rigidbody2D.position = new Vector2(m_Rigidbody2D.position.x, dash.y + 0.6f);
            Debug.Log("Platform Dash");
        }
        m_Rigidbody2D.velocity = new Vector2(direction * m_dashForce, 0);
    }

    IEnumerator NewDash(float direction)
    {
        canDash = false;
        float startTime = Time.time;

        while(Time.time < startTime + 0.12)
        {
            m_Rigidbody2D.velocity = new Vector2(direction * m_dashForce, 0);
            yield return null;
        }

        yield return new WaitForSeconds(2);
        canDash = true;
    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;
        direction *= -1;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}