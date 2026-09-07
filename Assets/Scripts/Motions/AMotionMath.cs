using UnityEngine;

public abstract class MotionMath : ScriptableObject
{
    [SerializeField] protected int speed = 5;
    [HideInInspector] public Vector2 heading;
    public abstract Vector2 Evaluate(float t);
}