using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Monobehaviours.AttackFunctions
{
    public class AttackFunctions : MonoBehaviour
    {
        #region Field
        [SerializeField] private Animator animator;
        [SerializeField] private CharacterController player;
        public bool isAttacking;
        private event Action<Abilities> AttackEvent;
        [SerializeField] private LayerMask enemyLayer;

        //Attacks
        [SerializeField] private Abilities[] abilities;
        private Abilities neutralPrimaryG;
        private Abilities sidePrimaryG;
        private Abilities downPrimaryG;
        private Abilities upPrimaryG;
        #endregion


        private void Awake()
        {
            AttackEvent += Attack;
            neutralPrimaryG = abilities[0];
            sidePrimaryG = abilities[1];
            downPrimaryG = abilities[2];
            upPrimaryG = abilities[3];
        }
        // Start is called before the first frame update
        

        // Update is called once per frame
        void Update()
        {

            if (!isAttacking)
            {
                if (Input.GetButtonDown("Primary"))
                {
                    if (Input.GetButton("Crouch"))
                    {
                        AttackEvent?.Invoke(downPrimaryG);
                    }
                    else if (Input.GetButton("Up"))
                    {
                        AttackEvent?.Invoke(upPrimaryG);
                    }
                    else if (Input.GetButton("Side"))
                    {
                        Debug.Log("attack");
                        AttackEvent?.Invoke(sidePrimaryG);
                    }
                    else
                    {
                        AttackEvent?.Invoke(neutralPrimaryG);
                    }
                }
            }
        }

        void Attack(Abilities attack)
        {
            foreach (var point in attack.HitboxOffsets)
            {
                Debug.DrawLine(transform.position, point * new Vector2(player.direction, 1) + (Vector2)transform.position, Color.green, 1f);
                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(point * new Vector2(player.direction, 1) + (Vector2)transform.position, attack.HitboxScale, enemyLayer);
                foreach (var enemy in hitEnemies)
                {
                    Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
                    rb.AddForce(attack.KnockbackValue * attack.KnockbackDirection * new Vector2(player.direction, 1), ForceMode2D.Impulse);
                }
            }

            
            
            Debug.Log($"used {attack.name}");
            animator.Play(attack.Animation);
        }

        /*void Attack(GameObject attack)
        {
            Instantiate(attack, transform);
            animator.Play("Attack1");
        }*/
    }
}

