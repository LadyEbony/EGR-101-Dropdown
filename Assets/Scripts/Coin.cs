using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : TriggerItem {
    [Header("Coin Settings")]
    public int coinValue = 1;
    public float rotationSpeed = 100f;
    public float floatHeight = 0.5f;
    public float floatSpeed = 2f;

    [Header("Effects")]
    public GameObject collectPrefab;

    private bool isCollected = false;

    void Update() {
      // rotate the coin
      transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

      // floating animation
      float newY = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
      transform.localPosition = new Vector3(0f, newY, 0f);
    }

  public override void OnTriggerEnterPlayer(PlayerDriver player) {
    if (isCollected) return;

    // add to score
    GameUI.Instance.AddScore(coinValue);

    // play effects
    var copy = Instantiate(collectPrefab, transform.position, Quaternion.identity);
    Destroy(copy, 2f);

    // disable visual components
    GetComponentInChildren<Renderer>().enabled = false;
    GetComponent<Collider>().enabled = false;

    // after effects finish
    Destroy(transform.parent.gameObject, 2f);

    isCollected = true;
  }
}
