using UnityEngine;

[CreateAssetMenu(menuName = "Move/Noise")]
public class Noise : MotionMath
{
    public override Vector2 Evaluate(float t, Vector2 heading)
    {
        t *= speed;
        Vector2 forward = heading.normalized;
        Vector2 perpendicular = new Vector2(-forward.y, forward.x);

        float noiseOffset = Mathf.PerlinNoise(t, 0f) * 2f - 1f;
        return (forward * t) + (perpendicular * noiseOffset);
    }
}