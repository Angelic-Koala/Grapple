using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class CharacterMovement : MonoBehaviour
{
    #region Field ///////////////////////////////////////////////////////////////////////////////////////////////////////

    //Script References ------------------------------------------------------------------------------------------------------------------------------------
    [SerializeField] private InputManager controls;

    //Components ------------------------------------------------------------------------------------------------------------------------------------
    private Rigidbody2D m_Rigidbody; //rigidbody of player
    [SerializeField] private Collider2D m_CrouchDisableCollider; //upper hitbox to disable while crouching

    [SerializeField] private LayerMask WhatIsGround;
    [SerializeField] private LayerMask WhatisGrappleable;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private SliderJoint2D rope;

    [SerializeField] private Animator animator;

    //booleans ------------------------------------------------------------------------------------------------------------------------------------
    [SerializeField] private bool m_isGrounded;
    [SerializeField] private bool m_isWallsliding;
    [SerializeField] private bool m_isWallJumping;
    [SerializeField] private bool m_fastFall;
    [SerializeField] private bool m_FacingRight = true;
    [SerializeField] private bool m_isCrouching = false;
    [SerializeField] private bool m_isGrappling = false;
    [SerializeField] private bool m_isDashing = false;
    [SerializeField] private bool m_canMove = true;
    [SerializeField] private bool m_canAttack = true;
    [SerializeField] private bool m_canTurn = true;
    [SerializeField] private bool m_canGrapple = true;
    [SerializeField] private bool m_canDash = false;

    private bool apex;

    //Floats and Ints ------------------------------------------------------------------------------------------------------------------------------------
    private float m_targetDirection = 0f;
    [Range(0, 1)][SerializeField] private float m_CrouchSpeed = .36f; //percent speed while crouching
    [SerializeField] public int direction = 1; // 1 when facing right, -1 when facing left
    [Range(0, .3f)][SerializeField] private float m_GroundedMovementSmoothing = .05f;
    [Range(0, .3f)][SerializeField] private float m_ArielMovementSmoothing = 0.15f;
    [SerializeField] private float m_JumpForce = 400f;
    [SerializeField] private float m_dashForce = 15f;
    [SerializeField] private float runSpeed = 40f;
    public float horizontalMove = 0f;
    [SerializeField] private float airJumps = 2f;
    [SerializeField] private float maxJumps = 2f;
    [SerializeField] private float maxFallSpeed = -5f;
    const float GroundedRadius = .1f;
    const float WallRadius = .1f;
    [SerializeField] private float gravity;

    //Timers -------------------------------------
    [SerializeField] private float wallTimer = 0.4f;
    [SerializeField] private float dashTimer = 2f;
    [Range(0, 0.7f)][SerializeField] private float m_fastFallTime = 0.5f;
    [SerializeField] private float wallCooldown = 0.4f;
    [SerializeField] private float dashDuration = 0.12f;

    //Vectors ------------------------------------------------------------------------------------------------------------------------------------
    private Vector2 m_Velocity = Vector2.zero;
    [SerializeField] private Transform m_GroundCheck;
    [SerializeField] private Transform m_WallCheck;
    


    #endregion Field ///////////////////////////////////////////////////////////////////////////////////////////////////////



    #region Game Loop And Setup ///////////////////////////////////////////////////////////////////////////////////////////////////////

    public virtual void Awake()
    {
        controls = new InputManager();
        m_Rigidbody = GetComponent<Rigidbody2D>();
        StateSetup();

        //Grapple setup
        rope = GetComponent<SliderJoint2D>();
        rope.enabled = false;
        rope.autoConfigureConnectedAnchor = false;

        //controls setup
        controls.Player.Jump.performed += context => Jump();
        controls.Player.Jump.canceled += context => CancelJump();
        controls.Player.Walk.performed += context => Walk(context.ReadValue<float>());
        controls.Player.Walk.canceled += context => Walk(0);
        controls.Player.Crouch.performed += context => CrouchOrFall();
        controls.Player.Crouch.canceled += context => CancelCrouch();
        controls.Player.Grapple.started += context => StartGrapple();
        controls.Player.Grapple.canceled += context => CancelGrapple("walking");
        controls.Player.Dash.started += context => Dash(horizontalMove);
    }

    private void FixedUpdate()
    {
        if (m_Rigidbody.velocity.y <= 0 && !m_isGrappling)
        {
            CheckCollisionOnLayer(m_GroundCheck.position, GroundedRadius, WhatIsGround, OnLandMethod);
            CheckCollision(m_WallCheck.position, WallRadius, OnWallslideMethod);
        }
        states[m_currentState]();
    }

    #endregion Game Loop And Setup ///////////////////////////////////////////////////////////////////////////////////////////////////////



    #region StateManager ///////////////////////////////////////////////////////////////////////////////////////////////////////



    #region Field ///////////////////////////////////////////////////////////////////////////////////////////////////////

    private string m_currentState = "idle";

    private Dictionary<string, Action> states = new Dictionary<string, Action>();

    #endregion Field ///////////////////////////////////////////////////////////////////////////////////////////////////////



    public virtual void StateSetup()
    {
        states.Add("idle", Idle);
        states.Add("walking", Walking);
        states.Add("crouching", Crouching);
        states.Add("attacking", Attacking);
        states.Add("stunned", Stunned);
        states.Add("grappling", Grappling);
        states.Add("wallsliding", WallSliding);
        states.Add("walljumping", WallJumping);
    }

    private void Update()
    {
        dashTimer += Time.deltaTime;
        wallTimer += Time.deltaTime;

        if (m_isDashing && dashTimer > 0.3f)
            m_isDashing = false;

        if (m_isWallJumping && wallTimer >= wallCooldown)
        {
            m_isWallJumping = false;
            ChangeState("walking");
        }
            
        //Gravity Handler
        if(m_isWallJumping || m_isWallsliding)
        {
            m_Rigidbody.gravityScale = 1f;
        }
        else if(m_fastFall && m_Rigidbody.velocity.y <= m_fastFallTime)
        {
            m_Rigidbody.gravityScale = 7f;
        }
        else if (Mathf.Abs(m_Rigidbody.velocity.y) <= 1)
        {
            m_Rigidbody.gravityScale = 1.7f;
            apex = true;
        }
        else
        {
            apex = false;
            m_Rigidbody.gravityScale = 2.5f;
        }

        gravity = m_Rigidbody.gravityScale;
    }

    public virtual void Idle()
    {
        Move(new Vector2(0, Mathf.Clamp(m_Rigidbody.velocity.y, maxFallSpeed, 100)));
    }

    public virtual void Walking()
    {
        Move(new Vector2(horizontalMove * Time.fixedDeltaTime * 15, Mathf.Clamp(m_Rigidbody.velocity.y, maxFallSpeed, 100)));
        if(Mathf.Abs(m_Rigidbody.velocity.x) <= 0.1)
        {
            ChangeState("idle");
        }
    }

    public virtual void Crouching()
    {
        Move(new Vector2(0, m_Rigidbody.velocity.y));
        if (m_CrouchDisableCollider != null)
            m_CrouchDisableCollider.enabled = false;
    }

    public virtual void Attacking()
    {
        if (m_canMove)
            Move(new Vector2(horizontalMove * Time.fixedDeltaTime * 15, Mathf.Clamp(m_Rigidbody.velocity.y, maxFallSpeed, 100)));
    }

    public virtual void Stunned()
    {
        Move(Vector2.zero);
    }

    public virtual void Grappling()
    {
        lineRenderer.SetPosition(1, transform.position);
    }

    public virtual void WallSliding()
    {
        Move(new Vector2(horizontalMove * Time.fixedDeltaTime * 15, Mathf.Clamp(m_Rigidbody.velocity.y, 0, 100)));
    }

    public virtual void WallJumping()
    {
        Move(new Vector2(Mathf.Clamp(horizontalMove * Time.fixedDeltaTime * 15, 2, runSpeed) * direction , Mathf.Clamp(m_Rigidbody.velocity.y, maxFallSpeed, 100)));
    }

    public void ChangeState(string newState)
    {
        m_currentState = newState;
        Debug.Log(m_currentState);
        PlayAnimation(newState);
    }

    #endregion StateManager ///////////////////////////////////////////////////////////////////////////////////////////////////////



    #region Collision Checking Methods ///////////////////////////////////////////////////////////////////////////////////////////////////////
    private void CheckCollisionOnLayer(Vector2 point, float radius, LayerMask layer, Action<Collider2D[], int> OnCollisionMethod)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(point, radius, layer);
        for (int i = 0; i < colliders.Length; i++)
        {
            OnCollisionMethod(colliders, i);
        }
    }

    private void CheckCollision(Vector2 point, float radius, Action<Collider2D[], int> OnCollisionMethod)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(point, radius);
        for (int i = 0; i < colliders.Length; i++)
        {
            OnCollisionMethod(colliders, i);
        }
    }

    public virtual void OnLandMethod(Collider2D[] colliders, int i)
    {
        if (colliders[i].gameObject != gameObject && !colliders[i].gameObject.GetComponent<Collider2D>().isTrigger)
        {
            if (!m_canGrapple)
            {
                Debug.Log("Grapple Reset");
            }
            m_isWallJumping = false;
            m_isGrounded = true;
            animator.SetBool("grounded", true);
            airJumps = maxJumps;
            m_canGrapple = true;
        }
    }

    public virtual void OnWallslideMethod(Collider2D[] colliders, int i)
    {
        if (colliders[i].gameObject != gameObject && colliders[i].gameObject.layer == 3)
        {
            m_isWallsliding = true;
            m_isWallJumping = false;
            m_Rigidbody.gravityScale = 1;
            if (airJumps < 1)
                airJumps = 1;
            dashTimer = 2;
            ChangeState("wallsliding");
        }
        else if (colliders[i].gameObject.layer != 3)
        {
            m_isWallsliding = false;
            if(m_currentState == "wallsliding")
                ChangeState("walking");
        }
    }

    #endregion Collision Checking Methods ///////////////////////////////////////////////////////////////////////////////////////////////////////



    #region Movement Functions ///////////////////////////////////////////////////////////////////////////////////////////////////////

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }



    public virtual void Walk(float direction)
    {
        m_targetDirection = direction;
        horizontalMove = direction * runSpeed;
        if (m_currentState == "idle")
            ChangeState("walking");
    }

    public virtual IEnumerator Dash(float direction)
    {
        if (!m_canDash)
        {
            yield break;
        }
        m_canDash = false;
        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            m_isDashing = true;
            m_Rigidbody.velocity = new Vector2(direction * m_dashForce, 0);
            yield return null;
        }
        m_isDashing = false;

        yield return new WaitForSeconds(2);
        m_canDash = true;
    }

    public virtual void CrouchOrFall()
    {
        if (!m_isGrounded)
        {
            m_fastFall = true;
            if (m_fastFall && m_Rigidbody.velocity.y <= m_fastFallTime)
                m_Rigidbody.gravityScale = 7;
        }
        else
        {
            ChangeState("crouching");
        }
    }

    public virtual void CancelCrouch()
    {
        ChangeState("walking");
        m_fastFall = false;
        m_Rigidbody.gravityScale = 2f;
    }

    public virtual void Jump()
    {
        if (m_isWallsliding)
        {
            Flip();
            m_isWallJumping = true;
            ChangeState("walljumping");
            wallTimer = 0;
            m_Rigidbody.velocity = new Vector2(horizontalMove * Time.fixedDeltaTime * -15, m_JumpForce - 5);
        }
        else if (airJumps > 0)
        {
            m_isWallJumping = false;
            ChangeState("walking");
            m_fastFall = false;
            airJumps--;
            m_isGrounded = false;
            animator.SetBool("grounded", false);
            m_CrouchDisableCollider.enabled = true;
            m_Rigidbody.velocity = new Vector2(m_Rigidbody.velocity.x, m_JumpForce);
        }
    }

    public virtual void CancelJump()
    {
        Debug.Log("jump canceled");
        if (m_Rigidbody.velocity.y > 0f)
            m_Rigidbody.velocity = new Vector2(m_Rigidbody.velocity.x, m_Rigidbody.velocity.y * 0.5f);
    }

    public virtual void StartGrapple()
    {
        if (m_canGrapple)
        {
            Vector2 mousePos = (Vector2)mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 startPos = (Vector2)transform.position;
            Vector2 grappleDir = mousePos - (Vector2)transform.position;
            RaycastHit2D hit = Physics2D.Raycast(origin: transform.position, grappleDir, 100f, WhatisGrappleable);
            if (hit)
            {
                lineRenderer.SetPosition(0, hit.point);
                rope.connectedAnchor = hit.point;
                rope.angle = Vector2.SignedAngle(Vector2.right, startPos - rope.connectedAnchor);
                Debug.DrawLine(startPos, rope.connectedAnchor, Color.green, 3f);
                m_isGrappling = true;
                rope.enabled = true;
                m_canGrapple = false;
                lineRenderer.enabled = true;
                ChangeState("grappling");
            }
        }
    }

    public virtual void CancelGrapple(string newState)
    {
        rope.enabled = false;
        lineRenderer.enabled = false;
        m_isGrappling = false;
        ChangeState(newState);
    }

    public virtual void Move(Vector2 targetVelocity)
    {
        if (m_isGrounded)
        {
            m_Rigidbody.velocity = Vector2.SmoothDamp(m_Rigidbody.velocity, targetVelocity, ref m_Velocity, m_GroundedMovementSmoothing);
            if (m_canTurn && m_targetDirection * direction < 0)
                Flip();
        }
        else
        {
            m_Rigidbody.velocity = Vector2.SmoothDamp(m_Rigidbody.velocity, targetVelocity, ref m_Velocity, m_ArielMovementSmoothing);
        }
    }

    private void Flip()
    {
        // Switch the way the player is labeled as facing.
        m_FacingRight = !m_FacingRight;
        direction *= -1;

        // Multiply the player's x local scale by -1.
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void PlayAnimation(string animation)
    {
        foreach (KeyValuePair<string, Action> func in states)
        {
            animator.SetBool(func.Key, false);
        }
        animator.SetBool(animation, true);
    }

    #endregion Movement Functions


}
