using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ShipPart shipPart = collision.GetComponent<ShipPart>();
        if(shipPart != null)
        {
            if(collision == shipPart.mainCollider)
                shipPart.ShipPartDetected();
        }

    }
}
