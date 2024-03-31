using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlatformDash : MonoBehaviour
{
    public CharacterController controller;
    public float y;
    public bool colliding;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 3)
        {
            colliding = true;
            y = collision.transform.position.y;
            Debug.Log(y); Debug.Log(colliding);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            colliding = false;
            Debug.Log(colliding);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.layer == 3)
        {
            colliding = false;
        }
    }

    private void Update()
    {
        if (colliding)
        {
            if (Input.GetButtonDown("Dash"))
            {
                controller.Dash(controller.horizontalMove);
            }
        }
    }
}
