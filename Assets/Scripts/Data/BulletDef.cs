using UnityEngine;

// 
[CreateAssetMenu(menuName = "BulletDef")]
public class BulletDef : ScriptableObject
{
    [field: SerializeField] public int damage { get; private set; }
    [field: SerializeField] public int speed { get; private set; }
}