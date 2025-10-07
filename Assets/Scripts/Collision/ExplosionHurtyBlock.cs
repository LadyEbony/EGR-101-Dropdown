using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionHurtyBlock : CollisionBlock {

  private bool disabled;

  public GameObject explosionPrefab;
  public float extraBounceForce = 5f;

  public override void OnCollisionEnterPlayer(PlayerDriver player, ContactPoint cp) {
    if (disabled) return;

    player.Damage();

    var project = Vector3.ProjectOnPlane(-cp.normal, Vector3.forward);
    player.rigidbody.AddForce(project *  extraBounceForce, ForceMode.Impulse);

    var copy = Instantiate(explosionPrefab, cp.point, Quaternion.identity);
    Destroy(copy, 5f);

    disabled = true;
    Destroy(gameObject);
  }

  public override void OnColliderEnterProjectile(ContactPoint cp) {
    if (disabled) return;

    var copy = Instantiate(explosionPrefab, cp.point, Quaternion.identity);
    Destroy(copy, 5f);

    disabled = true;
    Destroy(gameObject);
  }
}
