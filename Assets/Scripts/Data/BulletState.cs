using UnityEngine;

// Used by a move module each frame to update bullet's position
[System.Serializable]

public struct BulletState
{
    public Vector2 position; // updated every frame
    public Vector2 heading; // normalized; for rotation
    public float speed;
    public float age; // seconds since launch

    public Vector2 spawnPosition;
    public Vector2 spawnHeading;

    public int index; // "I am bullet 3..."
    public int total; // "...of 12 in this volley"
}
