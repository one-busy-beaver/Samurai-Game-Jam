using UnityEngine;

[CreateAssetMenu(menuName = "Move/Line")]
public class Line : MotionMath
{
    public override Vector2 Evaluate(float t)
    {
        t *= speed;
        return heading.normalized * t;
    }
}