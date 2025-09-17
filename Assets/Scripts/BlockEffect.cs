using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockEffect : MonoBehaviour
{
    [Header("Death Effect Settings")]
    public bool killPlayerOnContact = true;

    [Header("Visual Feedback")]
    public ParticleSystem impactParticles;
    public Material hitMaterial;
    private Material originalMaterial;
    private Renderer blockRenderer;

    void Start()
    {
        blockRenderer = GetComponent<Renderer>();
        if (blockRenderer != null && hitMaterial != null)
        {
            originalMaterial = blockRenderer.material;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // check if the colliding object is the player
        if (other.CompareTag("Player") && killPlayerOnContact)
        {
            ApplyDeathEffect(other.gameObject);
            PlayImpactEffects();
        }
    }

    void ApplyDeathEffect(GameObject player)
    {
        PlayerDeath playerDeath = player.GetComponent<PlayerDeath>();

        if (playerDeath != null)
        {
            playerDeath.Die(); // calls the Die() method 
        }
    }

    void PlayImpactEffects()
    {
        // play particles
        if (impactParticles != null)
        {
            impactParticles.Play();
        }

        // change material briefly
        if (blockRenderer != null && hitMaterial != null)
        {
            blockRenderer.material = hitMaterial;
            Invoke("ResetMaterial", 0.2f);
        }
    }

    void ResetMaterial()
    {
        if (blockRenderer != null && originalMaterial != null)
        {
            blockRenderer.material = originalMaterial;
        }
    }
}