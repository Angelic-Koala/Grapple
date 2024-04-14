using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

namespace Monobehaviours.AttackFunctions
{
    public class AttackFunctions : MonoBehaviour
    {
        #region Field
        [SerializeField] private Animator animator;
        [SerializeField] private CharacterMovement player;
        public bool isAttacking;
        private event Action<BasicAttack> AttackEvent;
        //private event Action<Abilities> AttackEvent;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private GameObject hitboxCollider;

        //Attacks
        [SerializeField] private BasicAttack[] attacks;
        private BasicAttack neutralPrimaryG;
        private BasicAttack sidePrimaryG;
        private BasicAttack downPrimaryG;
        private BasicAttack upPrimaryG;

        /*[SerializeField] private Abilities[] abilities;
        private Abilities neutralPrimaryG;
        private Abilities sidePrimaryG;
        private Abilities downPrimaryG;
        private Abilities upPrimaryG;*/
        #endregion


        private void Awake()
        {
            AttackEvent += AttackFrame;
            neutralPrimaryG = attacks[0];
            sidePrimaryG = attacks[1];
            downPrimaryG = attacks[2];
            upPrimaryG = attacks[3];

            /*AttackEvent += Attack;
            neutralPrimaryG = abilities[0];
            sidePrimaryG = abilities[1];
            downPrimaryG = abilities[2];
            upPrimaryG = abilities[3];*/
        }
        // Start is called before the first frame update
        

        // Update is called once per frame
        void Update()
        {

            if (!isAttacking && player.m_canAttack)
            {
                if (Input.GetButtonDown("Primary"))
                {
                    player.ChangeState("attacking");
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

        void AttackFrame(BasicAttack attack)
        {
            foreach (var h in attack.hitboxes)
            {
                hitboxCollider.transform.localScale = new Vector2(h.sizeX, h.sizeY);
                StartCoroutine(SpawnAttack(h));
                
            }
        }

        IEnumerator SpawnAttack(Hitbox h)
        {
            player.m_canTurn = false;
            player.m_canAttack = false;
            GameObject hitbox = Instantiate(hitboxCollider, new Vector2(transform.position.x + (h.offset.x * player.direction), transform.position.y + h.offset.y), transform.rotation);
            if (h.parented)
                hitbox.transform.parent = transform;
            HitboxCollision HitboxScript = hitbox.GetComponent<HitboxCollision>();
            HitboxScript.knockback = h.knockback;
            if (!h.hasGravity)
                HitboxScript.rb.gravityScale = 0;
            HitboxScript.rb.AddForce(h.velocity, ForceMode2D.Impulse);
            yield return new WaitForSeconds(h.duration);
            Destroy(hitbox);
            player.m_canTurn = true;
            player.m_canAttack = true;
        }

        bool WaitForFrames(int duration)
        {
            for (int i = 0; i < duration; i++)
            {
                new WaitForEndOfFrame();
            }
            return true;
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

