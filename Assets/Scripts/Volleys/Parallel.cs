using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Volley/Parallel")]
public class Parallel : BulletVolley
{
    [SerializeField] float displacement;

    public override List<BulletSpawnInfo> Generate()
    {
        float min = - spacing * (count - 1) * 0.5f;
        var list = new List<BulletSpawnInfo>(count);
        for (int i = 0; i < count; i++)
        {
            float lateralDisplacement = min + spacing * i;
            Vector2 pos = new Vector2(displacement, lateralDisplacement);

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