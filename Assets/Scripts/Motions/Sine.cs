using UnityEngine;

[CreateAssetMenu(menuName = "Move/Sine")]
public class Sine : MotionMath
{
    public override Vector2 Evaluate(float t, Vector2 heading)
    {
        t *= speed;
        Vector2 forward = heading.normalized;
        Vector2 perpendicular = new Vector2(-forward.y, forward.x);

        return (forward * t) + (perpendicular * Mathf.Sin(t));
    }
}