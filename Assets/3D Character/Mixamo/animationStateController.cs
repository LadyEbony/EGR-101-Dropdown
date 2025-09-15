using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationStateController : MonoBehaviour
  
{
    Animator animator;
    int isRunningHash;
    int isFallingHash;
    public float groundCheckDistance = 7.2f;
    public LayerMask groundMask;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        isRunningHash = Animator.StringToHash("isRunning");
        isFallingHash = Animator.StringToHash("isFalling");
    }

    // Update is called once per frame
    void Update()
    {
        bool isRunning = animator.GetBool(isRunningHash);
        bool forwardPressed = Input.GetKey("d");
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask);

        // update animator
        animator.SetBool(isFallingHash, !isGrounded);

        // if player presses w key
        if (!isRunning && forwardPressed)
        {
            // then set the isRunning boolean to be true
            animator.SetBool(isRunningHash, true);
        }
        // if player is not pressing w key
        if (isRunning && !forwardPressed)
        {
            // then set the isRunning boolean to be false
            animator.SetBool(isRunningHash, false);
        }
    }
}
