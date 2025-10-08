using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Burst.CompilerServices;
using UnityEditor;
using UnityEngine;

public class PlayerDriver : MonoBehaviour
{

    public static PlayerDriver Instance { get; private set; }
    public new Rigidbody rigidbody;
    public animationStateController animatorState;
    public GameObject projectile;
    public GameObject projectileParticles;

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
    public float parachuteStartTime;
    public float parachuteCooldownEndTime;
    public float timeLimitForParachuteUse = 4.5f; // seconds
    public float timeBeforeParachuteNextUse = 8f; // seconds

    public UnityEngine.UI.Image parachuteCooldownImage;
    public UnityEngine.UI.Image shootCooldownImage;

    [Header("Pushback")]
    public GameObject pushbackPrefab;
    public float pushbackInputDisableTime = 0.5f;
    public float pushbackForceScaler = 1f;
    private float pushbackInputDisableStartTime;

    [Header("ShootTimeDelay")]
    public float shootTimeDelay = 10f;
    private float shootStartTime;
    public AnimationCurve shootSizeAnimationCurve;

    //public Vector3 velocity;

    // collision variables for enemies and projectiles
    [Header("Health")]
    public int health = 3;
    public float invincibilityTime = 1f;
    private float invincibleUntil = 0f;

    [Header("Audio Source")]
    public AudioSource hurtAudioSource;

    private void Awake()
    {
        Instance = this;

        shootStartTime = Time.time - shootTimeDelay * 0.5f;
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
            v.y = 5.5f;
            rigidbody.velocity = v;

            shootStartTime = Time.time;

            limiterBypassStartTime = Time.time;
            limiterBypassDuration = 1.5f;
            Instantiate(projectile, transform.position, Quaternion.identity);

            var copy = Instantiate(projectileParticles, transform.position, Quaternion.identity);
            Destroy(copy, 5f);
        }

        HandleParachuteCooldown();
        HandleShootCooldown();
    }

    void HandleParachuteCooldown()
    {
        /* // This way might work
parachuteCooldownImage.fillAmount = Mathf.Clamp01((parachuteCooldownEndTime - Time.time) / timeBeforeParachuteNextUse);
parachuteCooldownImage.color = parachuteCooldownEndTime <= Time.time ? Color.white : Color.gray; */

        // how much cooldown time is left (0 if ready)
        float remainingCooldown = Mathf.Max(0f, parachuteCooldownEndTime - Time.time);

        // fill amount (1 = fully cooling down, 0 = ready)
        parachuteCooldownImage.fillAmount = remainingCooldown / timeBeforeParachuteNextUse;

        if (remainingCooldown > timeBeforeParachuteNextUse / 2f)
        {
            parachuteCooldownImage.color = Color.red;
        }
        else if (remainingCooldown > 0f)
        {
            parachuteCooldownImage.color = Color.yellow;
        }
        else
        { // TODO: this never happens
            parachuteCooldownImage.color = Color.blue;
        }
    }

    void HandleShootCooldown()
    {

        var cd = InverseLerpUnclamped(0f, shootTimeDelay, Time.time - shootStartTime);

        shootCooldownImage.fillAmount = cd;

        var ulerp = Mathf.InverseLerp(1f, 1.1f, cd);
        var c = Color.Lerp(Color.white, Color.cyan, ulerp);
        c.a = Mathf.InverseLerp(2f, 1.5f, cd);
        shootCooldownImage.color = c;

        shootCooldownImage.transform.localScale = shootSizeAnimationCurve.Evaluate(ulerp) * Vector3.one;
    }

    float InverseLerpUnclamped(float a, float b, float value)
    {
        return Mathf.Max(0f, (value - a) / (b - a));
    }

    private void FixedUpdate()
    {
        if (input.parachute && Time.time >= parachuteCooldownEndTime) {
            if (!isSlowFalling) {// just started parachuting
                parachuteStartTime = Time.time;
                isSlowFalling = true;
            }

            // check if we've exceeded 3 seconds
            if (Time.time - parachuteStartTime > timeLimitForParachuteUse){
                isSlowFalling = false;
                parachuteCooldownEndTime = Time.time + timeBeforeParachuteNextUse; // start cooldown
            }
        } else {
            if (isSlowFalling) { // parachute stopped early
                isSlowFalling = false;
                parachuteCooldownEndTime = Time.time + 6f; 
            }
        }

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
        pi.parachute = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);

        var shootDelay = Mathf.InverseLerp(0f, shootTimeDelay, Time.time - shootStartTime);
        pi.shoot = Input.GetKeyDown(KeyCode.S) && shootDelay >= 1f || Input.GetKey(KeyCode.DownArrow) && shootDelay >= 1f;

        return pi;
    }

    public void Damage()
    {
        health -= 1;
        if (health <= 0)
        {
            Kill();
        }

        hurtAudioSource.PlayOneShot(hurtAudioSource.clip);
    }

    public void Kill()
    {
        isDead = true;
    }

}