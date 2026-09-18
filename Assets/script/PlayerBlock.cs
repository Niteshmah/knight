using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBlock : MonoBehaviour
{
    private GameInputs inputs;
    private Animator anim;
    private PlayerController movementScript;
    private PlayerAttack attackScript;
    private bool isBlocking;

    public bool IsBlocking => isBlocking;
    private void Awake()
    {
        inputs = new GameInputs();
        anim = GetComponent<Animator>();
        movementScript = GetComponent<PlayerController>();
        attackScript = GetComponent<PlayerAttack>();
    }

    private void Start()
    {
        movementScript = GetComponent<PlayerController>();
    }
    private void OnEnable()
    {
        inputs.Player.Enable();
        inputs.Player.Block.performed += OnBlockPerformed;
        inputs.Player.Block.canceled += OnBlockCanceled;
    }

    private void OnDisable()
    {
        inputs.Player.Block.performed -= OnBlockPerformed;
        inputs.Player.Block.canceled -= OnBlockCanceled;
        inputs.Player.Disable();
    }

    private void Update()
    {
        if (isBlocking && movementScript != null && !movementScript.IsGrounded())
        {
            CancelBlock();
        }
    }

    private void OnBlockPerformed(InputAction.CallbackContext context)
    {
        if (movementScript == null) return;

        if (movementScript.IsDashing || (attackScript != null && attackScript.IsCasting)) return;

        if (movementScript.IsGrounded())
        {
            isBlocking = true;
            anim.SetBool("IsBlocking", true);
        }
    }

    private void OnBlockCanceled(InputAction.CallbackContext context)
    {
        CancelBlock();
    }
    
    private void CancelBlock()
    {
        if (!isBlocking) return;
        isBlocking = false;
        anim.SetBool("IsBlocking", false);
    }
}
