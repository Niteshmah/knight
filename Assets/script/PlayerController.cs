using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Dash settings")]
    [SerializeField] private float dashPower = 24f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Movement tweaks")]
    [SerializeField] private float groundDeceleration = 50f;
    [SerializeField] private float airDeceleration = 20f;
     
    [Header("Collider Shrink Settings")]
    [SerializeField] private float dashHeightMultiplier = 0.5f;

    private GameInputs inputs;
    private float horizontalInput;
    private Rigidbody2D rb;
    private Animator anim;
    private PlayerBlock blockScript;
    private PlayerAttack attackScript;

    private BoxCollider2D playerCollider;

    private bool isDashing;
    private bool canDash = true;
    private float originalGravityScale;

    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;

    public bool IsDashing => isDashing;

    private void Awake()
    {
        inputs = new GameInputs();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        blockScript = GetComponent<PlayerBlock>();
        attackScript = GetComponent<PlayerAttack>();

                playerCollider = GetComponent<BoxCollider2D>();
        if (playerCollider != null)
        {
            originalColliderSize = playerCollider.size;
            originalColliderOffset = playerCollider.offset;
        }

        originalGravityScale = rb.gravityScale;
    }

    private void OnEnable()
    {
        inputs.Player.Enable();
        inputs.Player.Move.performed += OnMovePerformed;
        inputs.Player.Move.canceled += OnMovePerformed;
        inputs.Player.Jump.performed += OnJumpPerformed;
        inputs.Player.Dash.performed += OnDashPerformed;
    }

    private void OnDisable()
    {
        inputs.Player.Move.performed -= OnMovePerformed;
        inputs.Player.Move.canceled -= OnMovePerformed;
        inputs.Player.Jump.performed -= OnJumpPerformed;
        inputs.Player.Dash.performed -= OnDashPerformed;
        inputs.Player.Disable();
    }

    private void Update()
    {
        bool grounded = IsGrounded();
        // terenary operator it check if i am on ground or not 
        float animationSpeed = Mathf.Abs(horizontalInput);

        anim.SetFloat("Speed", animationSpeed);
        anim.SetBool("IsGrounded", grounded);
        anim.SetFloat("VerticalSpeed", rb.linearVelocity.y);

        if (isDashing) return;
        if ((blockScript != null && blockScript.IsBlocking) || (attackScript != null && attackScript.IsCasting)) return;

        if (horizontalInput > 0)
        {
            transform.eulerAngles = Vector3.zero;
        }
        else if (horizontalInput < 0)
        {
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            float dashDirection = transform.eulerAngles.y == 180f ? -1f : 1f;
            rb.linearVelocity = new Vector2(dashDirection * dashPower, 0f);
            return;
        }

        if (blockScript != null && blockScript.IsBlocking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (attackScript != null && attackScript.IsCasting)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float targetXVelocity = horizontalInput * moveSpeed;
        float decelarationRate = IsGrounded() ? groundDeceleration : airDeceleration;
        float newXVelocity = Mathf.MoveTowards(rb.linearVelocity.x, targetXVelocity, decelarationRate * Time.fixedDeltaTime);
        //it is a early return statement that checks if the player is dashing, blocking, or attacking. If any of these conditions are true, the method returns early and does not execute the rest of the code. This prevents the player from moving while performing these actions.
        rb.linearVelocity = new Vector2(newXVelocity, rb.linearVelocity.y);
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        horizontalInput = context.ReadValue<float>();
    }
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if ((blockScript != null && blockScript.IsBlocking) || (attackScript != null && attackScript.IsCasting)) return;

        if (isDashing)
        {
            if (!IsGrounded()) return;

            float dashDirection = transform.eulerAngles.y == 180f ? -1f : 1f;
            // this just give gravity it original value
            rb.gravityScale = originalGravityScale;
            rb.linearVelocity = new Vector2(dashDirection * dashPower * 1.2f, jumpForce * 1.1f);

            anim.SetTrigger("JumpTrigger");
            

            ResetCollider();
            isDashing = false;
            return;
        }

        if (IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("JumpTrigger");
        }
    }

    private IEnumerator DashcooldownOnlyRoutine()
    {
        canDash = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void OnDashPerformed(InputAction.CallbackContext context)
    {
        if (!canDash || isDashing) return;
        if (attackScript != null && attackScript.IsCasting) return;
        if (blockScript != null && blockScript.IsBlocking) return;
        if (!IsGrounded()) return;

        StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;

        rb.gravityScale = 0f;
        float dashDirection = transform.eulerAngles.y == 180f ? -1f : 1f;
        rb.linearVelocity = new Vector2(dashDirection * dashPower, 0f);

        anim.SetTrigger("DashTrigger");

        ShrinkCollider();

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravityScale;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        ResetCollider();
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void ShrinkCollider()
    {
        if (playerCollider == null) return;
         
        float targetHeight = originalColliderSize.y * dashHeightMultiplier;
        float heightDifference = originalColliderSize.y - targetHeight;

                playerCollider.size = new Vector2(originalColliderSize.x, targetHeight);

                playerCollider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - (heightDifference / 2f));
    }

    private void ResetCollider()
    {
        if (playerCollider == null) return;

        
        playerCollider.size = originalColliderSize;
        playerCollider.offset = originalColliderOffset;
    }

    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }
}
