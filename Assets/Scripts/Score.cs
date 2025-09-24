using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score : MonoBehaviour
{
    public static Score Instance { get; private set; }

    [Header("Score Settings")]
    public int currentScore = 0;
    public int coinsCollected = 0;
    public int pointsPerCoin = 10;

    [Header("UI References")]
    public UnityEngine.UI.Text scoreText;
    public UnityEngine.UI.Text coinsText;

    // for UI updates
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnCoinsChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddCoin(int value = 1)
    {
        coinsCollected += value;
        currentScore += pointsPerCoin * value;

        OnCoinsChanged?.Invoke(coinsCollected);
        OnScoreChanged?.Invoke(currentScore);

        UpdateUI();
    }

    public void AddPoints(int points)
    {
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);
        UpdateUI();
    }

    public void ResetScore()
    {
        currentScore = 0;
        coinsCollected = 0;
        OnScoreChanged?.Invoke(currentScore);
        OnCoinsChanged?.Invoke(coinsCollected);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {currentScore}";

        if (coinsText != null)
            coinsText.text = $"Coins: {coinsCollected}";
    }

    // save/load functionality
    public void SaveScore()
    {
        PlayerPrefs.SetInt("HighScore", Mathf.Max(GetHighScore(), currentScore));
        PlayerPrefs.SetInt("TotalCoins", GetTotalCoins() + coinsCollected);
        PlayerPrefs.Save();
    }

    public int GetHighScore()
    {
        return PlayerPrefs.GetInt("HighScore", 0);
    }

    public int GetTotalCoins()
    {
        return PlayerPrefs.GetInt("TotalCoins", 0);
    }
}
