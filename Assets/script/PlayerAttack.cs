using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("prefab Reference")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform firePoint;
    [Header("Timing Settings")]
    [SerializeField] private float castHoldDuration = 0.3f;
    [SerializeField] private float castCooldown = 1f;

    private GameInputs inputs;
    private Animator anim;
    private PlayerController movementScript;
    private PlayerBlock blockScript;
    private Rigidbody2D rb;
    private bool isCasting;
    private bool isCooldown;

    private float originalGravityScale;

    public bool IsCasting => isCasting;

    private void Awake()
    {
        inputs = new GameInputs();
        anim = GetComponent<Animator>();
        movementScript = GetComponent<PlayerController>();
        blockScript = GetComponent<PlayerBlock>();
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            originalGravityScale = rb.gravityScale;
        }
    }

    private void OnEnable()
    {
        inputs.Player.Enable();
        inputs.Player.Attack.performed += OnAttackPerformed;
        inputs.Player.Cast.performed += OnCastPerformed;
    }

    private void OnDisable()
    {
        inputs.Player.Attack.performed -= OnAttackPerformed;
        inputs.Player.Cast.performed -= OnCastPerformed;
        inputs.Player.Disable();
        ResetGravityonInterruption();
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (movementScript == null || movementScript.IsDashing || isCasting) return;
        if (blockScript != null && blockScript.IsBlocking) return;

        if (movementScript.IsGrounded())
        {
            anim.SetTrigger("AttackTrigger");
        }
        else
        {
            anim.SetBool("IsAttacking", true);
            anim.SetTrigger("AirAttackTrigger");
        }
    }

    private void OnCastPerformed(InputAction.CallbackContext context)
    {
        if (movementScript == null || movementScript.IsDashing || isCasting || isCooldown) return;
        if (blockScript != null && blockScript.IsBlocking) return;

        StartCoroutine(CastFireballRoutine());
    }

    private IEnumerator CastFireballRoutine()
    {
        isCasting = true;

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
        }

        anim.SetTrigger("CastTrigger");

        yield return new WaitForSeconds(castHoldDuration);

        if (fireballPrefab != null && firePoint != null)
        {
            GameObject fireball = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
            float direction = Mathf.Approximately(transform.eulerAngles.y, 180f) ? -1f : 1f;

            if (fireball.TryGetComponent(out FireballProjectile projectile))
            {
                projectile.setup(direction);
            }
        }

        if (rb != null) rb.gravityScale = originalGravityScale;
        isCasting = false;

        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        isCooldown = true;
        yield return new WaitForSeconds(castCooldown);
        isCooldown = false;
    }

    private void ResetGravityonInterruption()
    {
        if (isCasting && rb != null)
        {
            rb.gravityScale = originalGravityScale;
            isCasting = false;
        }
    }
}
