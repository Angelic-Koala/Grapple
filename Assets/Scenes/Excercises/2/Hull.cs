using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hull : ShipPart
{
    public override void ShipPartDetected()
    {
        Debug.Log("Hull Detected!");
    }
}
