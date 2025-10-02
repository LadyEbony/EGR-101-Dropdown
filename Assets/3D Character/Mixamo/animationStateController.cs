using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationStateController : MonoBehaviour
{

    public PlayerDriver playerDriver;

    public Animator animator;
    public float groundCheckDistance = 1f;
    public LayerMask groundMask;

    public GameObject shieldMagicGameObject;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //add offset so ray starts at feet
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;

        animator.SetBool("isParachuting", playerDriver.isSlowFalling);
        shieldMagicGameObject.SetActive(playerDriver.isSlowFalling);
    }

    /*
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);
    }
    */
}

