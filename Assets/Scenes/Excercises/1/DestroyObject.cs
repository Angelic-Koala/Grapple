using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    [SerializeField] private float seconds;
    public event Action destroyEvent;

    IEnumerator Break()
    {
        yield return new WaitForSeconds(seconds);
        destroyEvent?.Invoke();
    }
    private void Start()
    {
        destroyEvent += DestructionEvent;
        StartCoroutine(Break());
    }

    void DestructionEvent()
    {
        Destroy(gameObject);
    }

}
