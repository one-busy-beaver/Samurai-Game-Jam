using UnityEngine;

public abstract class MotionMath : ScriptableObject
{
    [SerializeField] protected int speed = 5;
    public abstract Vector2 Evaluate(float t, Vector2 heading);
}