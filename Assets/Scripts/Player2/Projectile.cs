using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody2D rb;

    public float speed = 10f;
    private Vector2 direction;

    // Initialize the projectile with a direction
    public void Init(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Awake()
    {
        // Check if Rigidbody2D exists
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic; // safer than isKinematic

        // Check if Collider exists
        CircleCollider2D c = GetComponent<CircleCollider2D>();
        if (c == null)
        {
            c = gameObject.AddComponent<CircleCollider2D>();
        }
        c.isTrigger = true;
    }

    void Update()
    {
        if (rb != null)
            rb.linearVelocity = direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var dmg = other.GetComponent<IDamageable>();
        if (dmg != null)
        {
            dmg.TakeDamage(1f, gameObject);
            Destroy(gameObject);
        }
    }
}
