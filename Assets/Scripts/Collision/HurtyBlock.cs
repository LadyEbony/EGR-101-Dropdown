using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtyBlock : CollisionBlock {

    public AudioSource audioSource;
    public GameObject bouncePrefab;

    public float delay = 0.25f;
    private float tDelay;

    public override void OnCollisionEnterPlayer(PlayerDriver player, ContactPoint cp)
    {
        if (Time.time <= tDelay) return;

        player.Damage();

        audioSource.Play();

        var project = Vector3.ProjectOnPlane(-cp.normal, Vector3.forward);
        var copy = Instantiate(bouncePrefab, cp.point, Quaternion.LookRotation(project));
        Destroy(copy, 5f);

        tDelay = Time.time + delay;
    }
}
