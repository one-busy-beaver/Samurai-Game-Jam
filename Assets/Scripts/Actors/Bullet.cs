using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] bool selfDestroying;
    [SerializeField] float destroyTime = 1f;
    
    public MotionMath Motion { get; set; }
    public Vector2 Heading { get; set; }
    Vector2 startPosition;
    float time;

    void Start()
    {
        startPosition = transform.position;
        if (selfDestroying)
            Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        Move();
    }

    void Move()
    {
        if (Motion == null) return;
        time += Time.deltaTime;
        Vector2 offset = Motion.Evaluate(time, Heading);
        transform.position = startPosition + offset;
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerControl player = collision.GetComponent<PlayerControl>();
            if (player != null && !player.IsInvincible)
            {
                // Get the closest point on the player's collider to the hitbox's center
                Vector2 hitPoint = collision.ClosestPoint(transform.position);
                player.TakeDamage(hitPoint, damage);
            }
        }
    }

    // Destroy bullet when it flies off screen
    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}