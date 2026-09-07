using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] BulletDef def;
    [SerializeField] MoveModule move;
    BulletState state;


    void Update()
    {
        move.Step(ref state, Time.deltaTime);
        transform.position = state.position;
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerControl player = collision.GetComponent<PlayerControl>();
            if (player != null && !player.IsInvincible)
            {
                player.TakeDamage(transform.position, def.damage);
            }
        }
    }

    // Destroy bullet when it flies off screen
    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

    // Called by Emitter immediately after Instantiate.
    public void Launch(BulletDef def, Vector2 direction, int index, int total)
    {
        this.def = def;
        Vector2 heading = direction.normalized;

        state = new BulletState
        {
            position      = transform.position,
            heading       = heading,
            speed         = def.speed,
            age           = 0f,
            spawnPosition = transform.position,
            spawnHeading  = heading,
            index         = index,
            total         = total
        };
        transform.position = state.position;
    }

}
