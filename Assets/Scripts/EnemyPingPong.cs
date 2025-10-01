using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPingPong : MonoBehaviour {

    public Vector3 offset;
    private Vector3 startPosition;
    private Vector3 destinationPosition;
    
    [Header("Animation")]
    public float length = 1f;
    public float speed = 1f;

    private void Start()
    {
        startPosition = transform.position;
        destinationPosition = startPosition + offset;
    }

    private void Update()
    {
        var pp = Mathf.PingPong(Time.time * speed, length);
        var lerp = Mathf.InverseLerp(0f, length, pp);
        transform.position = Vector3.Lerp(startPosition, destinationPosition, lerp);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        var soffset = Vector3.back;
        if (Application.isPlaying)
        {
            Gizmos.DrawLine(startPosition + soffset, destinationPosition + soffset);
        } else
        {
            Gizmos.DrawLine(transform.position + soffset, transform.position + offset + soffset);
        }
    }
}
