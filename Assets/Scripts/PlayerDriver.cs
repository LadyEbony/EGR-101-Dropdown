using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDriver : MonoBehaviour {

    public static PlayerDriver Instance { get; private set; }
    public CharacterController characterController;

    public float horizontalSpeed = 10f;
    public float veritcalSpeed = -10f;
    public float horizontalAccelerationSpeed = 80f;
    public float verticalAccelerationSpeed = -9.8f;

    [Header("Pushback")]
    public float pushBackSpeed = 20f; //10f might be better
    public float pushBackDampSpeed = 40f;

    public Vector3 velocity;
    public Vector3 pushBackVelocity;

    private void Awake() {
        Instance = this;
    }

    // Update is called once per frame
    void Update() {
        var i = GetInput();

        velocity += new Vector3(i.horizontal * horizontalAccelerationSpeed, verticalAccelerationSpeed) * Time.deltaTime;
        velocity.y = Mathf.Clamp(velocity.y, veritcalSpeed, 0);
        //Debug.Log(velocity);
        // this is the offset that we will move the player every frame
        // we scale the offset by Time.deltaTime so the offset is consistent every frame
        var offset = velocity * Time.deltaTime;

        offset += pushBackVelocity * Time.deltaTime;
        pushBackVelocity = Vector3.MoveTowards(pushBackVelocity, Vector3.zero, Time.deltaTime * pushBackDampSpeed);
        //pushBackVelocity.x = Mathf.Clamp(pushBackVelocity.x, 7, 0); // Mot sure this does anything

        // we can call character controller move every update
        characterController.Move(offset);

        // prevent player from moving in the z axis
        // kinda technically
        var pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
    }

    public const int WALL_LAYER = 8;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.layer == WALL_LAYER)
        {
            pushBackVelocity = hit.normal * pushBackSpeed;
        }
    }

    // we can extend this struct to whatever input we need to register
    struct PlayerInput
    {
        public float horizontal;
    }

    PlayerInput GetInput()
    {
        var pi = new PlayerInput();

        // input get key down is bad but it's quick to implement and test
        if (Input.GetKey(KeyCode.A)) pi.horizontal -= 0.5f; // was originally 1f
        if (Input.GetKey(KeyCode.D)) pi.horizontal += 0.5f; // was originally 1f

        return pi;
    }

}
