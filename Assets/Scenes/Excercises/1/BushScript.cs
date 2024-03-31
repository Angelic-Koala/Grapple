using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BushScript : GateScript
{
    public override void OpenEvent()
    {
        destroyedObjects++;
        transform.localScale = new Vector3(1, 1, 1) * (destroyedObjects + 1);
    }
}