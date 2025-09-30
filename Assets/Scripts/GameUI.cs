using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {

    public Volume volume;
    private Vignette vigenette;

    [Header("Health")]
    public GameObject healthPrefab;

    [Header("Death")]
    public Graphic[] deathGraphics;
    private bool playedDeathAnimation;

    private void Awake()
    {
        volume.profile.TryGet(out vigenette);
    }

    private void LateUpdate()
    {
        var player = PlayerDriver.Instance;
        if (player == null) return;

        UpdateHealthIcons(player.health);

        if (player.isDead && !playedDeathAnimation)
        {
            playedDeathAnimation = true;
            StartCoroutine(DoDeathAnimation());
        }
    }

    void UpdateHealthIcons(int health)
    {
        health = Mathf.Max(health, 0);  // fail-safe

        var parentTransform = healthPrefab.transform.parent;
        var count = parentTransform.childCount - 1;

        // health less than gameobject count
        // remove children
        if (health < count)
        {
            Destroy(parentTransform.GetChild(1).gameObject);
        }

        // gameobject count less than health
        // add children
        if (health > count)
        {
            var copy = Instantiate(healthPrefab, parentTransform);
            copy.SetActive(true);
        }        
    }

    void SetAlpha(Graphic g, float alpha)
    {
        var c = g.color;
        c.a = alpha;
        g.color = c;
    }

    IEnumerator DoDeathAnimation()
    {
        var t = 0f;
        var vb = vigenette.intensity.value;


        while(t < 2f)
        {
            t += Time.deltaTime;
            var lerp = t / 2f;

            foreach (var d in deathGraphics) SetAlpha(d, lerp);
            vigenette.intensity.value = Mathf.Lerp(vb, 1f, lerp);

            yield return null;
        }

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(0);

    }
}
