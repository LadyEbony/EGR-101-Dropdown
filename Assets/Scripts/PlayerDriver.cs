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

    // normal vertical speed
    public float normalVerticalSpeed = -10f;

    // slow fall method
    public float slowFallVerticalSpeed = -5f;
    public bool isSlowFalling = false;

    [Header("Pushback")]
    public float pushBackSpeed = 20f; //10f might be better
    public float pushBackDampSpeed = 40f;

    public Vector3 velocity;
    public Vector3 pushBackVelocity;

    // collision variables for enemies and projectiles
    public int health = 3;
    public float invincibilityTime = 1f;
    private float invincibleUntil = 0f;

    private void Awake() {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        var i = GetInput();

        velocity += new Vector3(i.horizontal * horizontalAccelerationSpeed, verticalAccelerationSpeed) * Time.deltaTime;
        velocity.y = Mathf.Clamp(velocity.y, veritcalSpeed, 0);
        //Debug.Log(velocity);
        // this is the offset that we will move the player every frame
        // we scale the offset by Time.deltaTime so the offset is consistent every frame
        var offset = velocity * Time.deltaTime;
        var offset = new Vector3(i.horizontal * horizontalSpeed, normalVerticalSpeed) * Time.deltaTime;

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

        // check method for slow fall
        if (Input.GetKeyCode.W)
        {
            verticalSpeed = slowFallVerticalSpeed;
            isSlowFalling = true;

            // someone can add falling animation and effects here
        }
        else
        {
            verticalSpeed = normalVerticalSpeed;
            isSlowFalling = false;
        }
    }

    public const int WALL_LAYER = 8;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.layer == WALL_LAYER)
        {
            pushBackVelocity = hit.normal * pushBackSpeed;
        }
        else if (hit.gameObject.CompareTag("Enemy") || hit.gameObject.CompareTag("EnemyProjectile"))
        {
            TakeDamage();
            if (hit.gameObject.CompareTag("EnemyProjectile"))
            {
                Destroy(hit.gameObject);
            }
        }
        else if (hit.gameObject.CompareTag("Vine"))
        {
            verticalSpeed = Mathf.Max(verticalSpeed, -3f);
        }

        void TakeDamage()
        {
        if (Time.time < invincibilityUntil) return;
            health --;
            invincibilityUntil = Time.time + invincibilityTime;

            // visual effects for taking damage can be added here

            if (health <= 0)
            {
                Die();
            }
        }

        void Die()
        {
            Debug.Log("Player died");
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
