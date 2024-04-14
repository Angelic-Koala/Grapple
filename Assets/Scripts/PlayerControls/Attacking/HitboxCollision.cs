using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxCollision : MonoBehaviour
{
    public Vector2 knockback;
    public Rigidbody2D rb { get { return GetComponent<Rigidbody2D>(); } }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if(rb != null)
        {
            rb.AddForce(knockback, ForceMode2D.Impulse);
        }
    }
}
