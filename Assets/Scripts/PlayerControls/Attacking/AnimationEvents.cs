using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    [SerializeField] private Abilities abilities;

    public void AttackEvent()
    {
        Debug.Log("attacked");
    }
}
