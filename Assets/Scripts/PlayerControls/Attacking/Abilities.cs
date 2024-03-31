using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "AbilityStats", menuName = "ScriptableObjects/AbilityStats")]

public class Abilities : ScriptableObject
{
    [field: SerializeField][field: Range(0, 10)] public float CoolDown { get; private set; }
    [field: SerializeField][field: Range(0, 10)] public float Damage { get; private set; }
    [field: SerializeField] public Vector2 KnockbackDirection { get; private set; }
    [field: SerializeField][field: Range(0, 10)] public float KnockbackValue { get; private set; }
    [field: SerializeField] public Vector2Int[] HitboxOffsets { get; private set; }
    [field: SerializeField][field: Range(0, 10)] public float HitboxScale { get; private set; }
    [field: SerializeField] public string Animation { get; private set; }
}
