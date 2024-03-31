using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class Grappling : MonoBehaviour
{
    public LayerMask WhatisGrappleable;
    public Camera mainCamera;
    public LineRenderer lineRenderer;
    private CharacterController player;
    private SliderJoint2D rope;
    private Vector2 grapplePos;

    // Start is called before the first frame update
    void Start()
    {
        rope = GetComponent<SliderJoint2D>();
        rope.enabled = false;
        rope.autoConfigureConnectedAnchor = false;
        player = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (player.canGrapple && Input.GetButtonDown("Grapple"))
        {
            Vector2 mousePos = (Vector2)mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 startPos = (Vector2)transform.position;
            Vector2 grappleDir = mousePos - (Vector2)player.transform.position;
            RaycastHit2D hit = Physics2D.Raycast(origin: player.transform.position, grappleDir, 100f, WhatisGrappleable);
            if (hit)
            {
                Debug.Log(hit.point);
                grapplePos = hit.point;
                lineRenderer.SetPosition(0, hit.point);
                rope.connectedAnchor = hit.point;
                rope.angle = Vector2.SignedAngle(Vector2.right, startPos - rope.connectedAnchor);
                Debug.DrawLine(startPos, rope.connectedAnchor, Color.green, 3f);
                lineRenderer.enabled = true;
                player.grappling = true;
                rope.enabled = true;
                player.canGrapple = false;
            }
        }

        else if (Input.GetButtonUp("Grapple"))
        {
            rope.enabled = false;
            lineRenderer.enabled = false;
            player.grappling = false;
        }

        if (rope.enabled)
        {
            lineRenderer.SetPosition(1, transform.position);
        }
    }
}
