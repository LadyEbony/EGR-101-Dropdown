using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour {

    public Vector3 velocity;

    private void Update()
    {
        transform.position += velocity * Time.deltaTime;
    }
}
