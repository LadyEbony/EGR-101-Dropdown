using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricHurtyTrigger : TriggerItem {

  public ParticleSystem[] particleSystems;
  public AudioSource loopAudioSource;
  public AudioSource hitAudioSource;
  public bool disabled;
  public float disableTimer = 3f;

  public GameObject hurtPrefab;

  public override void OnTriggerEnterPlayer(PlayerDriver player) {
    if (disabled) return;

    disabled = true;
    player.Damage();

    foreach(var p in particleSystems) p.Stop();
    loopAudioSource.Stop();
    hitAudioSource.Play();

    var copy = Instantiate(hurtPrefab, player.transform.position, Quaternion.identity);
    Destroy(copy, 5f);

    StartCoroutine(Retrigger());
  }

  IEnumerator Retrigger(){
    yield return new WaitForSeconds(disableTimer);
    foreach(var p in particleSystems) p.Play();
    loopAudioSource.Play();

    disabled = false;
  }
}