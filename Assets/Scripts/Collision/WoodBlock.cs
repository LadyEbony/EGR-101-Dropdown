using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodBlock : CollisionBlock {

  private bool disabled;

  public GameObject explosionPrefab;

  public override void OnCollisionEnterPlayer(PlayerDriver player, ContactPoint cp) {

  }

  public override void OnColliderEnterProjectile(ContactPoint cp) {
    if (disabled) return;

    var copy = Instantiate(explosionPrefab, cp.point, Quaternion.identity);
    Destroy(copy, 5f);

    disabled = true;
    Destroy(gameObject);
  }
}
