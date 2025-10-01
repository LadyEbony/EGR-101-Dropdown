using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtyTriggerDoesntDisappear : TriggerItem {

    public bool disabled;

    public override void OnTriggerEnterPlayer(PlayerDriver player)
    {
        if (disabled) return;

        disabled = true;
        player.Damage();
    }
}
