using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterClasses : MonoBehaviour
{
    public virtual void Move()
    {
        Debug.Log("Move");
    }

    private void Start()
    {
        Move();
    }
}
