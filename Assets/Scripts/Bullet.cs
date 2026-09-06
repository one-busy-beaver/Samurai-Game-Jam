using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] int damage;

    void Update()
    {
        Move();
    }

    void Move()
    {
        // TODO
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerControl player = collision.GetComponent<PlayerControl>();
            if (player != null && !player.IsInvincible())
            {
                player.TakeDamage(transform.position, damage);
            }
        }
    }
}
