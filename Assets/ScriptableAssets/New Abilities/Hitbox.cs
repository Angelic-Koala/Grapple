using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Hitboxes", menuName = "ScriptableObjects/Hitboxes")]

public class Hitbox : ScriptableObject
{
    public float sizeX;
    public float sizeY;
    public Vector2Int offset;
    public bool parented;
    public float duration;
    public Vector2 velocity;
    public bool hasGravity;
    public Vector2 knockback;
    public float damage;
}
