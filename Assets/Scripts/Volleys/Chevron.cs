using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Volley/Chevron")]
public class Chevron : BulletVolley
{

    public override List<BulletSpawnInfo> Generate()
    {
        var list = new List<BulletSpawnInfo>(count);
        for (int i = 0; i < count; i++)
        {
            // TODO
        }
        return list;
    }

}