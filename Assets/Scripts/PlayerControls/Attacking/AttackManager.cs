using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    public bool isAttacking;
    public GameObject sidePrimary;

    private void Update()
    {
        if (!isAttacking)
        {
            if (Input.GetButtonDown("Primary"))
            {
                if (Input.GetButton("Crouch"))
                {

                }
                else if (Input.GetButtonDown("Up"))
                {

                }
                else if (Input.GetButton("Side"))
                {
                    Debug.Log("attack");
                    StartCoroutine (Attack(sidePrimary));
                }
            }
        }
        
    }

    IEnumerator Attack(GameObject attack)
    {
        AttackData data = attack.GetComponent<AttackData>();
        if(data != null)
        {
            isAttacking = true;
            yield return new WaitForSeconds(data.startup);
            if (data.isMelee)
            {
                Instantiate(attack, transform.position, transform.rotation, transform);
            }
            if (data.isRanged)
            {
                Instantiate(data.projectile, transform.position, transform.rotation);
            }
            yield return new WaitForSeconds(data.endLag);
            isAttacking = false;
        }
        yield return null;
    }
}
