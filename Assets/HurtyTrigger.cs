using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtyTrigger : TriggerItem {

    public bool disabled;

    public override void OnTriggerEnterPlayer(PlayerDriver player)
    {
        if (disabled) return;

        disabled = true;
        player.Damage();
        gameObject.SetActive(false);
    }
}
