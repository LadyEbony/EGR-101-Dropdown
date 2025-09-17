using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    private Transform cachedTransform;

    private void Awake()
    {
        cachedTransform = transform;
    }

    private void LateUpdate()
    {
        var player = PlayerDriver.Instance;
        if (player == null) return;

        var pos = player.transform.position;
        pos.z = cachedTransform.position.z;
        cachedTransform.position = pos;

    }
}
