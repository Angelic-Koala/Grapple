using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ShipPart : MonoBehaviour
{
    public Collider2D mainCollider;

    public abstract void ShipPartDetected();
}
