using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinTrigger : TriggerItem {

    public bool disabled;

    public override void OnTriggerEnterPlayer(PlayerDriver player)
    {
        if (disabled) return;

        disabled = true;
        
        // increase score/
        // play particles
        // play sound

        gameObject.SetActive(false);
    }
}
