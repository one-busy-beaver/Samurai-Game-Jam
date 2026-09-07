using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] int damage;
    
    public MotionMath Motion { get; set; }
    public Vector2 Heading { get; set; }
    Vector2 startPosition;
    float time;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        Move();
    }

    void Move()
    {
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
                player.TakeDamage(transform.position, damage);
            }
        }
    }

    // Destroy bullet when it flies off screen
    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
