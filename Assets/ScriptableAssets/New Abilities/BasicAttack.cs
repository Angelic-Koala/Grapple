using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
[CreateAssetMenu(fileName = "Attacks", menuName = "ScriptableObjects/Attacks")]

public class BasicAttack : ScriptableObject
{
    //public Hitbox[] hitboxes = new Hitbox[2];
    public List<Hitbox> hitboxes;
}
