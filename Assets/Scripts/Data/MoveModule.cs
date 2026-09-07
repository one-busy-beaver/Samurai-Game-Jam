using UnityEngine;

// An abstract class for different movement patterns
public abstract class MoveModule : ScriptableObject
{
    public abstract void Step(ref BulletState state, float deltaTime);

}

[CreateAssetMenu(menuName = "Move/Linear")]
public class LinearMove : MoveModule
{
    [SerializeField] float acceleration = 0f;
    [SerializeField] float minSpeed = 0f;
    [SerializeField] float maxSpeed = 30f;

    public override void Step(ref BulletState state, float deltaTime)
    {
        if (acceleration != 0f)
            state.speed = Mathf.Clamp(state.speed + acceleration * deltaTime,
                                      minSpeed, maxSpeed);

        state.position += state.heading * state.speed * deltaTime;
    }
}

[CreateAssetMenu(menuName = "Move/Sine")]
public class SineMove : MoveModule
{
    [SerializeField] float amplitude = 0.5f;
    [SerializeField] float frequency = 6f;
    [SerializeField] bool alternateByIndex = true;

    public override void Step(ref BulletState state, float deltaTime)
    {
        float sign = (alternateByIndex && (state.index & 1) == 1) ? -1f : 1f;

        // A vector 90 degrees from the heading — "sideways" for this bullet.
        Vector2 perpendicular = new Vector2(-state.heading.y, state.heading.x);

        // Compute the wave offset now vs one frame ago, and apply the DIFFERENCE.
        // If we set position absolutely instead, the bullet would teleport.
        float before = Mathf.Sin((state.age - deltaTime) * frequency) * amplitude * sign;
        float after  = Mathf.Sin(state.age * frequency) * amplitude * sign;

        state.position += state.heading * state.speed * deltaTime
                        + perpendicular * (after - before);
    }
}