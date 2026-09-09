using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Volley/Parallel")]
public class Parallel : BulletVolley
{
    [SerializeField] float bulletSpacing;
    [SerializeField] float distanceFromShooter;

    public override List<BulletSpawnInfo> Generate()
    {
        float min = - bulletSpacing * (count - 1) * 0.5f;
        var list = new List<BulletSpawnInfo>(count);
        for (int i = 0; i < count; i++)
        {
            float lateralDisplacement = min + bulletSpacing * i;
            Vector2 pos = new Vector2(distanceFromShooter, lateralDisplacement);

            BulletSpawnInfo info = new BulletSpawnInfo 
            { 
                relativePosition = pos, 
                angle = 0f, 
                motion = motion
            };
            list.Add(info);
        }
        return list;
    }

}