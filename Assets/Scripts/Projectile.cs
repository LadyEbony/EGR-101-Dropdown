using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    //Get RigidBody Component
    [SerializeField] private Rigidbody rb;

    [SerializeField] private float LaunchForce = 50f;

    [SerializeField] private float destroyAfterSeconds = 5f;
    // Start is called before the first frame update
    void Start()
    {
        // Give a force to the projectile
        rb.velocity = Vector3.down*LaunchForce;

    }

    // Update is called once per frame
    void Update()
    {
        Destroy(gameObject, destroyAfterSeconds); 
    }
}
