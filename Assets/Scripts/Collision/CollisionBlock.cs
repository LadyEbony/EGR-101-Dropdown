using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CollisionBlock : MonoBehaviour {
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject && collision.contactCount > 0)
        {
            var script = collision.gameObject.GetComponent<PlayerDriver>();
            if (script) OnCollisionEnterPlayer(script, collision.contacts[0]);
        }
    }

    public abstract void OnCollisionEnterPlayer(PlayerDriver player, ContactPoint cp);
}
