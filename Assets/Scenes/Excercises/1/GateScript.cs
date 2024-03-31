using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateScript : MonoBehaviour
{
    protected int destroyedObjects = 0;
    public List<GameObject> destroyableObjects = new List<GameObject>();

    private void Start()
    {
        foreach (var objects in destroyableObjects)
        {
            DestroyObject objectScript = objects.GetComponent<DestroyObject>();
            objectScript.destroyEvent += OpenEvent;
        }
    }

    public virtual void OpenEvent()
    {
        destroyedObjects++;
        if (destroyedObjects >= destroyableObjects.Count)
        {
            Destroy(gameObject);
        }
    }
}
