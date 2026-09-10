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

    // Switched to OnTriggerEnter2D so impact triggers once immediately upon contact
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerControl player = collision.GetComponent<PlayerControl>();
            if (player != null)
            {
                Vector2 hitPoint = collision.ClosestPoint(transform.position);
                // TakeDamage internally checks 'if (IsInvincible) return;' so damage is safely ignored during dashes
                player.TakeDamage(hitPoint, damage);
            }

            // Always destroy the bullet on contact so it doesn't linger and hit you after the dash ends
            Destroy(gameObject);
        }
    }

    // Destroy bullet when it flies off screen
    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}