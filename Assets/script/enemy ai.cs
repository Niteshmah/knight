using Unity.VisualScripting;
using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
public class enemyai : MonoBehaviour
{
    [Header("Movement setting")]
    [SerializeField] private float patrolSpeed = 2.5f;
    [SerializeField] private float ChaseSpeed = 4.5f;
    [SerializeField] private float flipcooldown = 2f;

    [Header("Detection setting")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask groundLayer;

    [Header("Ground/edge checking")]
    [SerializeField] private Transform groundcheckpoint;
    [SerializeField] private Transform wallcheckpoint;
    [SerializeField] private float checkRadius = 0.2f;

    private Rigidbody2D rb;
    private Transform playerTransform;
    private bool isChasing;
    private bool movingRight = true;
    private float nextFlipTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        checkForplayer();

        if (isChasing && playerTransform != null)
        {
            ChasePlayer();
        }
        else
        {
            PatrolRoutine();
        }
    }

    private void PatrolRoutine()
    {
        bool isGroundedAhead = Physics2D.OverlapCircle(groundcheckpoint.position, checkRadius, groundLayer);
        bool isWallBlocking = Physics2D.OverlapCircle(wallcheckpoint.position, checkRadius, groundLayer);

        if ((!isGroundedAhead || isWallBlocking) && Time.time >= nextFlipTime)
        {
            Flip();
        }

        float direction = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * patrolSpeed, rb.linearVelocity.y);
    }

    private void ChasePlayer()
    {
        float directionToPlayer = playerTransform.position.x - transform.position.x;
        
        if (directionToPlayer > 0 && !movingRight) Flip();
        else if (directionToPlayer < 0 && movingRight) Flip();

        float moveDirection = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(moveDirection * ChaseSpeed, rb.linearVelocity.y);
    }

    private void checkForplayer()
    {
        Vector2 rayDirection = movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, detectionRange, playerLayer);

        if (hit.collider != null)
        {
            isChasing = true;

        }
        else if (isChasing && playerTransform != null)
        {
            float distanceToPlayer = Mathf.Abs(transform.position.x - playerTransform.position.x);
            if (distanceToPlayer <= detectionRange * 1.3f)
            {
                isChasing = false;
            }
        }
    }

    private void Flip()
    {
        movingRight = !movingRight;
        transform.Rotate(0f, 180f, 0f);
        nextFlipTime = Time.time + flipcooldown;
    }

   
}
