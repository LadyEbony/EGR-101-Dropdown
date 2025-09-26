using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Coin Settings")]
    public int coinValue = 1;
    public float rotationSpeed = 100f;
    public float floatHeight = 0.5f;
    public float floatSpeed = 2f;

    [Header("Effects")]
    public ParticleSystem collectParticles;
    public AudioClip collectSound;

    private Vector3 startPosition;
    private bool isCollected = false;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (isCollected) return;

        // rotate the coin
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

        // floating animation
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            CollectCoin();
        }
    }

    void CollectCoin()
    {
        isCollected = true;

        // add to score
        if (Score.Instance != null)
        {
            Score.Instance.AddCoin(coinValue);
        }

        // play effects
        if (collectParticles != null)
        {
            ParticleSystem particles = Instantiate(collectParticles, transform.position, Quaternion.identity);
            Destroy(particles.gameObject, 2f);
        }

        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        // disable visual components
        GetComponentInChildren<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        // after effects finish
        Destroy(gameObject, 2f);
    }
}
