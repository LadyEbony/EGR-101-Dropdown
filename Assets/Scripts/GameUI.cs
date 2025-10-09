using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;  

public class GameUI : MonoBehaviour {

  public static GameUI Instance { get; private set; }

    public Volume volume;
    private Vignette vigenette;

    [Header("Health")]
    public GameObject healthPrefab;

    [Header("Death")]
    public Graphic[] deathGraphics;
    public TextMeshProUGUI deathScoreText;
    private bool playedDeathAnimation;

    [Header("Score")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public float score = 0;
    public float prevScore = 0;

    public static int highestScore = 1000;

    private void Awake()
    {
      Instance = this;
        volume.profile.TryGet(out vigenette);

        foreach (var g in deathGraphics) SetAlpha(g, 0f);
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

        UpdateScoreUI();
    }

    void UpdateHealthIcons(int health)
    {
        health = Mathf.Max(health, 0);  // fail-safe

        var parentTransform = healthPrefab.transform.parent;
        var count = parentTransform.childCount - 1;

        if (health < count)
        {
            Destroy(parentTransform.GetChild(1).gameObject);
        }

        if (health > count)
        {
            var copy = Instantiate(healthPrefab, parentTransform);
            copy.SetActive(true);
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
    }

    public void ResetScore()
    {
        score = 0;
    }

    void UpdateScoreUI() {
      var updating = false;

      if (Mathf.RoundToInt(prevScore) != Mathf.RoundToInt(score)) {
        prevScore = Mathf.Lerp(prevScore, score, Time.deltaTime * 4f);
        updating = true;
      }

        UpdateTextMesh(scoreText, Mathf.RoundToInt(prevScore));
      scoreText.color = Color.Lerp(scoreText.color, updating ? Color.yellow : Color.white, Time.deltaTime * 4f);
    }
   
    void UpdateTextMesh(TextMeshProUGUI textMesh, int score)
    {
        textMesh.text = string.Format("{0:000000}", score);
    }

    void SetColor(Graphic g, Color color)
    {
        var c = g.color;
        c.r = color.r;
        c.g = color.g;
        c.b = color.b;
        g.color = c;
    }

    void SetAlpha(Graphic g, float alpha)
    {
        var c = g.color;
        c.a = alpha;
        g.color = c;
    }

    IEnumerator DoDeathAnimation()
    {
        var newScore = Mathf.RoundToInt(score);
        UpdateTextMesh(deathScoreText, newScore);

        if (newScore > highestScore)
        {
            highestScore = newScore;
            highScoreText.text = $"!! NEW HIGH SCORE !! {highestScore}";

            StartCoroutine(FlashGraphic(highScoreText));
        } else
        {
            highScoreText.text = $"HIGH SCORE: {highestScore}";
        }

        var t = 0f;
        var vb = vigenette.intensity.value;

        while (t < 2f)
        {
            t += Time.deltaTime;
            var lerp = t / 2f;

            foreach (var d in deathGraphics) SetAlpha(d, lerp);
            vigenette.intensity.value = Mathf.Lerp(vb, 1f, lerp);

            AudioListener.volume = 1f - lerp;

            yield return null;
        }

        yield return new WaitForSeconds(1f);
        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }

        AudioListener.volume = 1f;
        SceneManager.LoadScene(0);
    }

    IEnumerator FlashGraphic(Graphic g)
    {
        while (true)
        {
            SetColor(highScoreText, Color.white);
            yield return new WaitForSeconds(0.2f);
            SetColor(highScoreText, Color.yellow);
            yield return new WaitForSeconds(0.2f);
        }
    }
}
