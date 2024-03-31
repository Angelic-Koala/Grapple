using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Monobehaviours.NewEvent
{
    public class NewEvent : MonoBehaviour
    {

        #region Field
        private event Action<int, int, int> AttackEvent;
        #endregion
        // Start is called before the first frame update
        private void Awake()
        {
            AttackEvent += Speak;
            AttackEvent += Attack;
        }
        void Start()
        {
            AttackEvent.Invoke(3, 17, 12);
        }

        // Update is called once per frame
        void Update()
        {

        }

        void Attack(int a, int b, int c)
        {
            Debug.Log($" {a} {b} {c}");
        }
        void Speak(int a, int b, int c)
        {
            Debug.Log(a + b + c);
        }
    }
}

