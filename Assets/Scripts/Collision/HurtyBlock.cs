using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtyBlock : CollisionBlock {

    public AudioSource audioSource;
    public GameObject bouncePrefab;

    public override void OnCollisionEnterPlayer(PlayerDriver player, ContactPoint cp)
    {
        player.Damage();

        audioSource.Play();

        var project = Vector3.ProjectOnPlane(-cp.normal, Vector3.forward);
        var copy = Instantiate(bouncePrefab, cp.point, Quaternion.LookRotation(project));
        Destroy(copy, 5f);
    }
}
