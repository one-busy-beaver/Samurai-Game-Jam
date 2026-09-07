using UnityEngine;

[CreateAssetMenu(menuName = "Move/Line")]
public class Line : MotionMath
{
    public override Vector2 Evaluate(float t, Vector2 heading)
    {
        t *= speed;
        return heading.normalized * t;
    }
}