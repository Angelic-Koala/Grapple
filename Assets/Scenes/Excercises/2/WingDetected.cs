using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WingDetected : ShipPart
{
    public override void ShipPartDetected()
    {
        GameObject ship = GetComponentInParent<GameObject>();
        Debug.Log(ship.name);
    }
}
