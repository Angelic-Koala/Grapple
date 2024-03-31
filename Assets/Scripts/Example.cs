using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class Example : MonoBehaviour
{
    private event Action AttackEvent;

    private event Action<int> ExampleEvent;

    private event Func<int, int, int> ExampleFunction;

    //Look up object pooling tutorial & try it

    private void Start()
    {
        AttackEvent += () =>
        {
            Debug.Log("Lambda Function");
        };

        ExampleEvent += (int a) =>
        {
            Debug.Log(a);
        };

        ExampleFunction += Function;

        Debug.Log(ExampleFunction.Invoke(7, 8));
        
    }

    int Function(int a, int b)
    {
        return a + b;
    }

    
    
}
