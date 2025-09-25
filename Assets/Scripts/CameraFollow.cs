using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    private Transform cachedTransform;
    public float yOffset = 10;

    private void Awake()
    {
        cachedTransform = transform;
    }

    private void LateUpdate()
    {
        var player = PlayerDriver.Instance;
        if (player == null) return;

        var pos = cachedTransform.position;
        pos.y = player.transform.position.y + yOffset;
        cachedTransform.position = pos;

    }
}
