using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public CharacterController controller;
    [SerializeField] private Collider2D m_PlatformCheck;
    public bool collided = false;

    public float runSpeend = 40f;

    public float horizontalMove = 0f;
    bool jump = false;
    bool crouch = false;


    /*private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.IsTouchingLayers(7))
        {
            collided = true;
            Vector2 tpPos = collision.transform.position;
            float tpPosY = collision.transform.position.y;
            Debug.Log(tpPosY);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        collided = false;
    }*/

    // Update is called once per frame
    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeend;

        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
        }

        if (Input.GetButtonDown("Crouch"))
        {
            crouch = true;
        } else if (Input.GetButtonUp("Crouch"))
        {
            crouch = false;
        }
        if (Input.GetButtonDown("Dash"))
        {
            if(collided)
            {
                controller.Dash(horizontalMove);
            }
            else if (controller.dashTimer >= controller.dashCooldown)
            {
                controller.Dash(horizontalMove);
            }
        }
    }

    void FixedUpdate()
    {
        //Move our character
        controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump, controller.m_wallSliding);
        jump = false;
    }
}
