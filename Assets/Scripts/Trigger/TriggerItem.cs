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

            var script2 = collider.GetComponent<Projectile>();
            if (script2) OnTriggerEnterProjectile(script2);
        }
    }

    public abstract void OnTriggerEnterPlayer(PlayerDriver player);

    public virtual void OnTriggerEnterProjectile(Projectile projectle) { }
}
