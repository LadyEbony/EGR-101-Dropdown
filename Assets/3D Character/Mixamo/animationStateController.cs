using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationStateController : MonoBehaviour {

    public PlayerDriver playerDriver;

    public Animator animator;
    //int isRunningHash;
    int isFallingHash;
    //public float groundCheckDistance = 1f;
    //public LayerMask groundMask;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        //isRunningHash = Animator.StringToHash("isRunning");
        isFallingHash = Animator.StringToHash("isFalling");
    }

    // Update is called once per frame
    void Update()
    {
        //bool isRunning = animator.GetBool(isRunningHash);
        // bool forwardPressed = Input.GetKey("d");
        bool parachutePressed = playerDriver.isSlowFalling;

        // if player presses w key
        //if (!isRunning && forwardPressed)
        //{
        // then set the isRunning boolean to be true
        //    animator.SetBool(isRunningHash, true);
        //}
        // if player is not pressing w key
        //if (isRunning && !forwardPressed)
        //{
        // then set the isRunning boolean to be false
        //    animator.SetBool(isRunningHash, false);
        //}
        // add offset so ray starts at feet
        //Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        //bool isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance, groundMask);
        //animator.SetBool(isFallingHash,!isGrounded);

        //Parachuting logic
        if (parachutePressed)
        {
            animator.SetBool(isFallingHash, false);
            animator.SetBool("isParachuting", true);
        }
        else
        {
            animator.SetBool(isFallingHash, true);
            animator.SetBool("isParachuting", false);
        }

    }

    /*
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);
    }
    */
}
