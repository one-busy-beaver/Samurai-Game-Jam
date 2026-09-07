using UnityEngine;

[CreateAssetMenu(menuName = "Move/Spiral")]
public class Spiral : MotionMath
{
    public override Vector2 Evaluate(float t)
    {
        t *= speed;
        Vector2 forward = heading.normalized;
        Vector2 perpendicular = new Vector2(-forward.y, forward.x);

        return (forward * Mathf.Cos(t) * t) + (perpendicular * Mathf.Sin(t) * t);
    }
}