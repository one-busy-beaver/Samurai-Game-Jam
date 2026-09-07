using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Volley/Spread")]
public class Spread : BulletVolley
{
    [SerializeField] float spreadAngle;
    [SerializeField] float radius;

    public override List<BulletSpawnInfo> Generate()
    {
        var list = new List<BulletSpawnInfo>(count);
        if (count == 1)
        {
            BulletSpawnInfo info = new BulletSpawnInfo 
            { 
                relativePosition = Vector2.zero, 
                angle = 0f, 
                motion = motion
            };
            list.Add(info);
            return list;
        }

        spreadAngle = Math.Min(spreadAngle, 360 - spacing);

        float inc = spreadAngle / (count - 1);
        float min = - spreadAngle / 2;
        for (int i = 0; i < count; i++)
        {
            float angle = min + inc * i;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 pos = new Vector2(Mathf.Sin(rad) * radius, Mathf.Cos(rad) * radius);

            BulletSpawnInfo info = new BulletSpawnInfo 
            { 
                relativePosition = pos, 
                angle = angle, 
                motion = motion
            };
            list.Add(info);
        }
        return list;
    }

}