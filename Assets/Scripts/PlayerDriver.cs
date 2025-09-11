using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDriver : MonoBehaviour {

    public static PlayerDriver Instance { get; private set; }
    public new Rigidbody rigidbody;

    public float horizontalSpeed = 10f;
    public float veritcalSpeed = -10f;

    private void Awake() {
        Instance = this;
    }

    // Update is called once per frame
    void Update() {
        
    }

    private void FixedUpdate()
    {
        var i = GetInput();
        var offset = new Vector3(i.horizontal * horizontalSpeed, veritcalSpeed) * Time.fixedDeltaTime;
        rigidbody.MovePosition(rigidbody.position + offset);
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
        if (Input.GetKey(KeyCode.A)) pi.horizontal -= 1f;
        if (Input.GetKey(KeyCode.D)) pi.horizontal += 1f;

        return pi;
    }

}
