using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [Header("Runtime Parameters (Set Automatically)")]
    private float damage;
    private Vector2 direction;
    private float range;
    private LayerMask hitMask;
    private GameObject owner;

    [Header("Hitbox Settings")]
    [Tooltip("How long the hitbox stays active in seconds.")]
    public float lifetime = 0.1f;

    [Tooltip("Optional visual effect prefab to spawn when hitting something.")]
    public GameObject hitEffect;

    [Header("Debug / Gizmos")]
    [Tooltip("Show gizmos for debugging in Scene view.")]
    public bool showGizmos = true;

    [Tooltip("Show the area of the hitbox.")]
    public bool showHitArea = true;

    [Tooltip("Show direction arrow.")]
    public bool showDirectionArrow = true;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Init(
        float damage,
        Vector2 direction,
        float range,
        LayerMask hitMask,
        GameObject owner
    )
    {
        this.damage = damage;
        this.direction = direction.normalized;
        this.range = range;
        this.hitMask = hitMask;
        this.owner = owner;

        // Move hitbox forward slightly based on range
        transform.position += (Vector3)(this.direction * range * 0.5f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == owner)
            return;

        if ((hitMask.value & (1 << other.gameObject.layer)) == 0)
            return;

        var health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
            Debug.Log($"⚔️ {owner.name} hit {other.name} for {damage} damage!");
        }

        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }
    }

    // -------------------------
    // Scene Gizmos
    // -------------------------
    private void OnDrawGizmos()
    {
        if (!showGizmos)
            return;

        Gizmos.color = Color.red;
        Vector3 origin = transform.position;

        if (showHitArea)
        {
            // Draw a faint sphere for hit area
            Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
            Gizmos.DrawSphere(origin, range * 0.5f);

            // Outline
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(origin, range * 0.5f);
        }

        if (showDirectionArrow && direction != Vector2.zero)
        {
            Gizmos.color = Color.yellow;
            Vector3 dir3D = (Vector3)direction.normalized;
            Vector3 end = origin + dir3D * range;
            Gizmos.DrawLine(origin, end);
            Gizmos.DrawSphere(end, 0.05f);
        }
    }
}
