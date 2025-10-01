using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TriggerItem : MonoBehaviour
{
    public void OnTriggerEnter(Collider collider)
    {
        if (collider)
        {
            var script = collider.GetComponent<PlayerDriver>();
            if (script) OnTriggerEnterPlayer(script);
        }
    }

    public abstract void OnTriggerEnterPlayer(PlayerDriver player);
}
