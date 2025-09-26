using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class PlayerDriver : MonoBehaviour {

    public static PlayerDriver Instance { get; private set; }
    public new Rigidbody rigidbody;
    public animationStateController animatorState;

    public float horizontalSpeed = 10f;
    public float verticalSpeed = -10f;
    public float horizontalAccelerationSpeed = 80f;
    public float verticalAccelerationSpeed = -9.8f;

    public bool isDead;

    [Header("Temporary Bypass Limiters")]
    public float limiterBypassDuration = 0.5f;
    private float limiterBypassStartTime;
    
    [Header("Parachuting")]
    public float parachuteSpeedscaler = 0.5f;
    public bool isSlowFalling = false;

    [Header("Pushback")]
    public GameObject pushbackPrefab;
    public float pushbackInputDisableTime = 0.5f;
    public float pushbackForceScaler = 1f;
    private float pushbackInputDisableStartTime;

    [Header("ShootTimeDelay")]
    public float shootTimeDelay = 1f;
    private float shootStartTime;

    //public Vector3 velocity;

    // collision variables for enemies and projectiles
    [Header("Health")]
    public int health = 3;
    public float invincibilityTime = 1f;
    private float invincibleUntil = 0f;

    private void Awake() {
        Instance = this;

        shootStartTime = Time.time;
        limiterBypassStartTime = Time.time;
        pushbackInputDisableStartTime = Time.time;
    }

    PlayerInput input;

    // Update is called once per frame
    void Update()
    {
        input = GetInput();
        if (input.shoot)
        {
            animatorState.animator.SetTrigger("IsShooting");
            Debug.Log("shoot");
            // shoot logic
            var v = rigidbody.velocity;
            v.y = 10f;
            rigidbody.velocity = v;

            shootStartTime = Time.time;

            limiterBypassStartTime = Time.time;
            limiterBypassDuration = 1.5f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            // coin will handle its own collection logic
            // we can add player specific effects here if needed
        }
    }

    private void FixedUpdate()
    {
        isSlowFalling = input.parachute;

        var velocity = rigidbody.velocity;
        var disableLerp = Mathf.InverseLerp(0f, pushbackInputDisableTime, Time.time - pushbackInputDisableStartTime);
        var maxVelocityScaler = 1f + (1f - disableLerp) * 4f;

        var bypassLerp = Mathf.InverseLerp(0f, limiterBypassDuration, Time.time - limiterBypassStartTime);
        var maxBypassScaler = 1f + (1f - bypassLerp) * 4f;

        maxVelocityScaler *= maxBypassScaler;

        // check if charaacter control is active
        if (input.active)
        {
            var inputScaler = Mathf.Clamp01(disableLerp);
            velocity += new Vector3(input.horizontal * horizontalAccelerationSpeed, 0f) * Time.fixedDeltaTime * inputScaler;
        }

        //pushBackTime = Mathf.MoveTowards(pushBackTime, 0f, Time.deltaTime);

        // gravity
        velocity += new Vector3(0f, verticalAccelerationSpeed) * Time.fixedDeltaTime;
        // terminal velocity
        var terminalVel = verticalSpeed;
        if (isSlowFalling) terminalVel *= parachuteSpeedscaler;
        velocity.x = Mathf.Clamp(velocity.x, -horizontalSpeed * maxVelocityScaler, horizontalSpeed * maxVelocityScaler);
        velocity.y = Mathf.Clamp(velocity.y, -terminalVel, verticalSpeed * maxVelocityScaler);

        rigidbody.velocity = velocity;

    }

    public const int WALL_LAYER = 8;

    private void OnCollisionEnter(Collision collision)
    {
        pushbackInputDisableStartTime = Time.time;

        if (collision.contactCount > 0)
        {
            var c = collision.contacts[0];
            var pos = c.point;
            var rotation = Quaternion.LookRotation(c.normal);

            // particles
            var copy = Instantiate(pushbackPrefab, pos, rotation);
            Destroy(copy, 5f);

            // additional pushback
            //Debug.Log(c.normal);
            rigidbody.AddForce(c.normal * pushbackForceScaler, ForceMode.Impulse);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // FUNCTION IS NOW INVALID
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
        public bool shoot;
        public bool parachute;
    }

    PlayerInput GetInput()
    {
        var pi = new PlayerInput();

        // input get key down is bad but it's quick to implement and test
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) pi.horizontal -= 1f; 
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) pi.horizontal += 1f;

        pi.active = !isDead;
        pi.parachute = Input.GetKey(KeyCode.W);

        var shootDelay = Mathf.InverseLerp(0f, shootTimeDelay, Time.time - shootStartTime);
        pi.shoot = Input.GetKeyDown(KeyCode.S) && shootDelay >= 1f;
        
        

        return pi;
    }

}


