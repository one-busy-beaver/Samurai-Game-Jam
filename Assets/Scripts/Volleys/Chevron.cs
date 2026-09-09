using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Volley/Chevron")]
public class Chevron : BulletVolley
{
    [SerializeField] float bulletSpacing;
    [SerializeField] float distanceFromShooter;
    [SerializeField] float chevronDepth; // Positive for '>', Negative for '<'
    [SerializeField] float spreadAngle;


    public override List<BulletSpawnInfo> Generate()
    {
        var list = new List<BulletSpawnInfo>(count);
        if (count == 1)
        {
            list.Add(new BulletSpawnInfo
            {
                relativePosition = new Vector2(distanceFromShooter, 0f),
                angle = 0f,
                motion = motion
            });
            return list;
        }
        
        int half = count / 2;
        for (int i = 0; i < count; i++)
        {
            int rank = i - half; // Distance from center: e.g. -2, -1, 0, 1, 2
            float y = rank * bulletSpacing;
            float x = distanceFromShooter - Mathf.Abs(rank) * chevronDepth; // Creates the V shape

            float angle = rank / (float)half * (spreadAngle * 0.5f);

            list.Add(new BulletSpawnInfo
            {
                relativePosition = new Vector2(x, y),
                angle = angle,
                motion = motion
            });
        }
        return list;
    }

}