using UnityEngine;

public abstract class PatternModule : ScriptableObject
{
    public abstract void Fire(Emitter emitter);
}

[CreateAssetMenu(menuName = "Pattern/Spread")]
public class SpreadPattern : PatternModule
{
    [SerializeField] BulletDef bullet;
    [SerializeField, Min(1)] int count = 1;
    [SerializeField] float arcDegrees = 0f;    // 0 = column, 45 = fan, 360 = ring
    [SerializeField] float angleOffset = 0f;

    public override void Fire(Emitter emitter)
    {
        float baseAngle = emitter.AimAngleDegrees + angleOffset;
        bool fullCircle = Mathf.Approximately(arcDegrees, 360f);

        // A full ring divides by count so the first and last don't overlap.
        // An arc divides by count-1 so the outermost bullets land on the edges.
        float step = fullCircle
            ? arcDegrees / count
            : (count > 1 ? arcDegrees / (count - 1) : 0f);

        float start = fullCircle ? baseAngle : baseAngle - arcDegrees * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float radians = (start + step * i) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

            emitter.SpawnBullet(bullet, direction, i, count);
        }
    }
}