using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    //Get RigidBody Component
    public Rigidbody rb;

    public float launchForce = 50f;
    public float destroyAfterSeconds = 0.5f;

    void Start()
    {
        Destroy(gameObject, destroyAfterSeconds);
    }

  private void FixedUpdate()
  {
    var v = Vector3.down * launchForce;
    rb.MovePosition(rb.position + v * Time.fixedDeltaTime);
  }

}
