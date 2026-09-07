using UnityEngine;

public class Emitter : MonoBehaviour
{
    [SerializeField] Bullet bulletPrefab;
    [SerializeField] PatternModule pattern;
    [SerializeField] float muzzleRadius;
    [SerializeField] float interval;
    [SerializeField] bool firing = true;

    float nextFireTime;

    // The direction this emitter points, in degrees. Rotate the GameObject to aim.
    // Pattern reads this to know where the volley's center is.
    public float AimAngleDegrees =>
        Mathf.Atan2(transform.up.y, transform.up.x) * Mathf.Rad2Deg;

    void Update()
    {
        if (!firing || pattern == null) return;
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + interval;
        pattern.Fire(this);          // hand control to the pattern asset
    }

    // Called BY the pattern, once per bullet it wants.
    public void SpawnBullet(BulletDef def, Vector2 direction, int index, int total)
    {
        // Offset along each bullet's OWN direction, not from a single muzzle point.
        // This is what makes a ring look like it leaves the enemy's edge.
        Vector2 spawnPosition = (Vector2)transform.position + direction * muzzleRadius;

        Bullet bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        bullet.Launch(def, direction, index, total);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, muzzleRadius);
        Gizmos.DrawRay(transform.position, transform.up * (muzzleRadius + 1f));
    }
}