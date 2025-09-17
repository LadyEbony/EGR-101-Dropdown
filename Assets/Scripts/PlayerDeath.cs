using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeath : MonoBehaviour {

    [Header("Visual Effects")]
    public ParticleSystem deathParticles;
    public ParticleSystem trailParticles;
    public Light playerLight;
    public GameObject playerModel;
    public float lightFadeSpeed = 2f;

    [Header("Animation")]
    public Animator playerAnimator;
    public string deathAnimationTrigger = "Die";
    public float animationSlowdownFactor = 0.3f;

    // i will leave this for ellie to implement
    /*
    [Header("Audio")]
    public AudioSource deathSound;
    public AudioSource backgroundMusic;
    public float musicFadeSpeed = 1f;
    */

    /*
    [Header("Camera Effects")]
    public CameraShake cameraShake;
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.4f;
    */

    [Header("Game Over")]
    public GameObject gameOverUI;
    public float gameOverDelay = 1.5f;

    private bool isDead = false;
    private float originalTimeScale;
    private Color originalLightColor;
    private float originalLightIntensity;

    private PlayerDriver playerDriver;
    void Start()
    {
        playerDriver = GetComponent<PlayerDriver>();

        if (playerLight != null)

        {
            originalLightColor = playerLight.color;
            originalLightIntensity = playerLight.intensity;
        }

        originalTimeScale = Time.timeScale;
    }

    // Update is called once per frame
    void Update()

    {
        if (isDead)
        {
            HandleDeathEffects();
        }
    }

    // method when the player dies
    public void Die()

    {
        if (isDead) return;

        isDead = true;

        // disable player control
        if (playerDriver != null)
        {
            playerDriver.enabled = false;
        }

        // play death animation
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger(deathAnimationTrigger);
        }

        // leaving this for ellie to implement
        /*
        if (deathSound != null)
        {
            deathSound.Play();
        }
        */

        // death particles
        if (deathParticles != null)
        {
            deathParticles.Play();
        }

        // stop trail particles
        if (trailParticles != null)
        {
            trailParticles.Stop();
        }

        // will work on this later
        /*
        if (cameraShake != null)
        {
            cameraShake.Shake(shakeDuration, shakeMagnitude);
        }
        */

        // Slow down time for dramatic effect
        Time.timeScale = animationSlowdownFactor;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // leaving this for ellie to implement
        /*
        if (backgroundMusic != null)
        {
            StartCoroutine(FadeOutMusic());
        }
        */

        StartCoroutine(ShowGameOverAfterDelay());
    }

    private void HandleDeathEffects()
    {
        // fade out player light
        if (playerLight != null)
        {
            playerLight.intensity = Mathf.Lerp(playerLight.intensity, 0f, lightFadeSpeed * Time.unscaledDeltaTime);
            playerLight.color = Color.Lerp(playerLight.color, Color.red, lightFadeSpeed * Time.unscaledDeltaTime);
        }

        // make player model slowly rotate as it falls
        if (playerModel != null)
        {
            playerModel.transform.Rotate(Vector3.forward, 45f * Time.unscaledDeltaTime);
        }
    }

    // leaving this for ellie to implement
    /*
    private IEnumerator FadeOutMusic()
    {
        float startVolume = backgroundMusic.volume;

        while (backgroundMusic.volume > 0)
        {
            backgroundMusic.volume -= startVolume * Time.unscaledDeltaTime * musicFadeSpeed;
            yield return null;
        }

        backgroundMusic.Stop();
        backgroundMusic.volume = startVolume;
    }
    */

    private IEnumerator ShowGameOverAfterDelay()
    {
        yield return new WaitForSecondsRealtime(gameOverDelay);

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        // reset time scale
        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = 0.02f;
    }

    // reset player for new game
    public void ResetPlayer()
    {
        isDead = false;

        // reset light
        if (playerLight != null)
        {
            playerLight.color = originalLightColor;
            playerLight.intensity = originalLightIntensity;
        }

        // reset player model rotation
        if (playerModel != null)
        {
            playerModel.transform.rotation = Quaternion.identity;
        }

        // enable player control
        if (playerDriver != null)
        {
            playerDriver.enabled = true;
        }

        // reset animation
        if (playerAnimator != null)
        {
            playerAnimator.Rebind();
            playerAnimator.Update(0f);
        }

        // restart trail particles
        if (trailParticles != null)
        {
            trailParticles.Play();
        }
    }

    // maybe implement this later
    /*
    public void OnHealthDepleted()
    {
        Die();
    }
    */

}

// camera shake still testing be implemented later
/*
[System.Serializable]
public class CameraShake : MonoBehaviour
{
    private Transform cameraTransform;
    private Vector3 originalPosition;
    private float shakeTimeRemaining = 0f;
    private float shakePower = 0f;

    void Start()
    {
        cameraTransform = GetComponent<Transform>();
        originalPosition = cameraTransform.localPosition;
    }

    void Update()
    {
        if (shakeTimeRemaining > 0)
        {
            shakeTimeRemaining -= Time.deltaTime;

            float xShake = Random.Range(-1f, 1f) * shakePower;
            float yShake = Random.Range(-1f, 1f) * shakePower;

            cameraTransform.localPosition = originalPosition + new Vector3(xShake, yShake, 0f);

            shakePower = Mathf.MoveTowards(shakePower, 0f, Time.deltaTime * 2f);

            if (shakeTimeRemaining <= 0)
            {
                cameraTransform.localPosition = originalPosition;
            }
        }
    }

    public void Shake(float duration, float power)
    {
        shakeTimeRemaining = duration;
        shakePower = power;
    }
}
*/