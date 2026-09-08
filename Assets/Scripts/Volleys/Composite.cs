using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Volley/Composite")]
public class CompositeVolley : BulletVolley
{
    [SerializeField] BulletVolley parentVolley;
    [SerializeField] BulletVolley childVolley;
    [SerializeField] float displacement;

    public override List<BulletSpawnInfo> Generate()
    {
        var list = new List<BulletSpawnInfo>();

        List<BulletSpawnInfo> parentSpawns = parentVolley.Generate();
        List<BulletSpawnInfo> childSpawns = childVolley.Generate();

        for (int p = 0; p < parentSpawns.Count; p++)
        {
            BulletSpawnInfo parentBullet = parentSpawns[p];
            Quaternion parentRotation = Quaternion.Euler(0f, 0f, parentBullet.angle);

            for (int c = 0; c < childSpawns.Count; c++)
            {
                BulletSpawnInfo childBullet = childSpawns[c];

                Vector2 offset = childBullet.relativePosition + new Vector2(displacement, 0f);
                Vector2 rotatedOffset = parentRotation * offset;

                list.Add(new BulletSpawnInfo
                {
                    relativePosition = parentBullet.relativePosition + rotatedOffset,
                    angle = parentBullet.angle + childBullet.angle,
                    motion = childBullet.motion != null ? childBullet.motion : parentBullet.motion
                });
            }
        }

        return list;
    }
}