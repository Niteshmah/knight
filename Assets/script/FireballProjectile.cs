using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FireballProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 3f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void setup(float direction)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        rb.linearVelocity = new Vector2(direction * speed, 0f);

        float absoluteX = Mathf.Abs(transform.localScale.x);
        float targetScaleX = direction < 0 ? -absoluteX : absoluteX;

        if (!Mathf.Approximately(transform.localScale.x, targetScaleX))
        {
            transform.localScale = new Vector3(targetScaleX, transform.localScale.y, transform.localScale.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        {
           
        }
    }
}
