using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public Transform player;
    public List<GameObject> platforms = new List<GameObject>();

    // Update is called once per frame
    void Update()
    {
        foreach (var platform in platforms)
        {
            if (Input.GetButton("Crouch"))
            {
                platform.GetComponent<BoxCollider2D>().isTrigger = true;
                //platform.layer = 7;
            }
            else
            {
                if (player.position.y > platform.GetComponentInChildren<Transform>().position.y)
                {
                    platform.GetComponent<BoxCollider2D>().isTrigger = false;
                    //platform.layer = 3;
                }
                else
                {
                    platform.GetComponent<BoxCollider2D>().isTrigger = true;
                    //platform.layer = 7;
                }
            }

        }
    }
}
