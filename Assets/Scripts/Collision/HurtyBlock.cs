using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtyBlock : CollisionBlock {
    public override void OnCollisionEnterPlayer(PlayerDriver player, ContactPoint cp)
    {
        player.Damage();
    }
}
