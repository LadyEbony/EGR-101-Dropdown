using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncyBlock : CollisionBlock
{
    public GameObject bouncePrefab;
    public float extraBounceForce = 1f;
    public float bounceReactTime = 1f;
    public MeshRenderer meshRenderer;
    private Color meshBaseColor;
    private float lastBounceTime;

    private void Reset()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    private void Awake()
    {
        lastBounceTime = 0f;
        meshBaseColor = meshRenderer.sharedMaterial.color;
    }

    private void Update()
    {
        var lerp = Mathf.InverseLerp(lastBounceTime, lastBounceTime + bounceReactTime, Time.time);
        meshRenderer.material.color = Color.Lerp(Color.red, meshBaseColor, lerp);
    }

    public override void OnCollisionEnterPlayer(PlayerDriver player, ContactPoint cp)
    {
        var project = Vector3.ProjectOnPlane(-cp.normal, Vector3.forward);

        player.rigidbody.AddForce(project *  extraBounceForce, ForceMode.Impulse);
        lastBounceTime = Time.time;

        var copy = Instantiate(bouncePrefab, cp.point, Quaternion.LookRotation(project));
        Destroy(copy, 5f);
    }
}
