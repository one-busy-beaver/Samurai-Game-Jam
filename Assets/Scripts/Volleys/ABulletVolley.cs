using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BulletSpawnInfo
{
    public Vector2 relativePosition; // Offset from spawn point
    public float angle; // Degrees (0 = right, 90 = up, etc.)
    public MotionMath motion; // Movement pattern for this bullet
}

public abstract class BulletVolley : ScriptableObject
{   
    [SerializeField, Min(1)] protected int count;
    [SerializeField] protected float spacing;
    [SerializeField] protected MotionMath motion;
    public abstract List<BulletSpawnInfo> Generate();
}