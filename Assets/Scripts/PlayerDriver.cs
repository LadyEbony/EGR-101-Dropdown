using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class PlayerDriver : MonoBehaviour {

    public static PlayerDriver Instance { get; private set; }
    public new Rigidbody rigidbody;

    public float horizontalSpeed = 10f;
    public float verticalSpeed = -10f;
    public float horizontalAccelerationSpeed = 80f;
    public float verticalAccelerationSpeed = -9.8f;

    [Header("Parachuting")]
    public float parachuteSpeedscaler = 0.5f;
    public bool isSlowFalling = false;

    [Header("Pushback")]
    [Tooltip("The temporary speed forced on to the player for knockback")]
    public float pushBackSpeed = 20f;
    [Tooltip("The length that the knockback is applied to the player")]
    public float pushBackControlTime = 1f;
    [Tooltip("The player's controls are inactive during knockback. The controls are returned during these last seconds")]
    public float pushBackControlReturnTime = 0.25f;

    private Vector3 pushBackNormal; // temp variable
    private float pushBackTime;     // temp variable

    //public Vector3 velocity;

    // collision variables for enemies and projectiles
    public int health = 3;
    public float invincibilityTime = 1f;
    private float invincibleUntil = 0f;

    private void Awake() {
        Instance = this;
    }

    PlayerInput input;

    // Update is called once per frame
    void Update()
    {
        input = GetInput();
    }

    private void FixedUpdate()
    {
        isSlowFalling = input.parachute;

        var velocity = rigidbody.velocity;

        // is knockback active? if so, apply knockback
        if (pushBackTime > 0f)
        {
            //var s = Mathf.SmoothStep(0f, pushBackControlTime, pushBackTime);
            //velocity = pushBackNormal * pushBackSpeed * s * Time.fixedDeltaTime;

            //Debug.Log($"{pushBackTime} + {pushBackNormal} -> {s}");
        }
        // check if charaacter control is active
        if (input.active)
        {
            velocity += new Vector3(input.horizontal * horizontalAccelerationSpeed, 0f) * Time.fixedDeltaTime;
        }

        //pushBackTime = Mathf.MoveTowards(pushBackTime, 0f, Time.deltaTime);

        // gravity
        velocity += new Vector3(0f, verticalAccelerationSpeed) * Time.fixedDeltaTime;
        // terminal velocity
        var terminalVel = verticalSpeed;
        if (isSlowFalling) terminalVel *= parachuteSpeedscaler;
        velocity.y = Mathf.Clamp(velocity.y, terminalVel, 0);

        rigidbody.velocity = velocity;

        // this is the offset that we will move the player every frame
        // we scale the offset by Time.deltaTime so the offset is consistent every frame
        // X MOVE
        //rigidbody.velocity = velocity;
        //var offset = velocity * Time.fixedDeltaTime;
        //rigidbody.MovePosition(rigidbody.position + offset);
    }

    public const int WALL_LAYER = 8;

    private void OnCollisionEnter(Collision collision)
    {

    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.LogWarning(hit.moveDirection);
        Debug.LogWarning(hit.moveLength);
        if (hit.gameObject.CompareTag("Enemy") || hit.gameObject.CompareTag("EnemyProjectile"))
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
        if (Time.time < invincibleUntil) return;
            health --;
            invincibleUntil = Time.time + invincibilityTime;

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
        public bool active;
        public float horizontal;

        public bool parachute;
    }

    PlayerInput GetInput()
    {
        var pi = new PlayerInput();

        // input get key down is bad but it's quick to implement and test
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) pi.horizontal -= 1f; 
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) pi.horizontal += 1f;

        pi.active = pushBackTime < pushBackControlReturnTime;

        pi.parachute = Input.GetKey(KeyCode.Space);

        return pi;
    }

}
