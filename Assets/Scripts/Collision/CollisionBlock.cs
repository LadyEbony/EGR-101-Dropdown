using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CollisionBlock : MonoBehaviour {
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject && collision.contactCount > 0)
        {
            var script = collision.gameObject.GetComponent<PlayerDriver>();
            var cp = collision.contacts[0];
            if (script) OnCollisionEnterPlayer(script, cp);

            var script2 = collision.gameObject.GetComponent<Projectile>();
            if (script2){
              OnColliderEnterProjectile(cp);
              Destroy(script2.gameObject);
            }
        }
    }

    public abstract void OnCollisionEnterPlayer(PlayerDriver player, ContactPoint cp);

    public virtual void OnColliderEnterProjectile(ContactPoint cp) { }
}
