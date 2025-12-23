using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] public int PlayerScore { get; private set; }
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject gameOver;
    [SerializeField] private TextMeshProUGUI endScoreText;
    [SerializeField] private TextMeshProUGUI coinsCollected;

    [SerializeField] private PlayerController playerController;

    [Header("UI Panels")]
    [SerializeField] private GameObject gameOverPanel; // Your original "gameOver" panel
    [SerializeField] private GameObject revivePanel;   // Your new "RevivePanel"

    public bool IsGameOver { get; private set; } = false;

    // CHANGED: Score per second can be its own variable now, distinct from coin values
    [Header("Base Score Settings")]
    [SerializeField] private float scorePerSecond = 10f; // Score gained every second alive
    private float sessionScore = 0f;

    // Coin-related rewards
    [Header("Coin Rewards")]
    [SerializeField] private int baseCoinScoreValue = 50;
    [SerializeField] private float speedIncreasePerCoin = 0.1f;
    [SerializeField] private int coinStreakBonusThreshold = 3;
    [SerializeField] private int coinStreakBonusScore = 200;
    [SerializeField] private float coinStreakSpeedBonus = 0.5f;

    // Dynamic Coin Streak Settings
    [Header("Dynamic Coin Streak")]
    [SerializeField] private float baseCoinStreakWindow = 0.8f;
    [SerializeField] private float minCoinStreakWindow = 0.2f;
    [SerializeField] private float speedToWindowReductionFactor = 0.02f;

    // For tracking coin streak - now ONLY in GameManager
    private int currentCoinStreak = 0;
    private float lastCoinCollectTime;


    void Start()
    {
        IsGameOver = false;

        if (playerController == null && player != null)
        {
            playerController = player.GetComponent<PlayerController>();
        }

        if (playerController != null)
        {
            playerController.OnCoinCollected += HandleCoinCollected;
        }

        if (SoundManager.Instance == null)
        {
            Debug.LogError("SoundManager not found in scene! Please add a SoundManager GameObject.");
        }

        if (ParticleManager.Instance == null)
        {
            Debug.LogError("ParticleManager not found in scene!");
        }

        gameOverPanel.SetActive(false);
        revivePanel.SetActive(false);
        Time.timeScale = 1; // Ensure game is running

        UpdateCoinText(0);
        PlayerScore = 0; // Ensure initial score is 0
        scoreText.text = "00 |";
        lastCoinCollectTime = -baseCoinStreakWindow; // Initialize to ensure first coin starts a streak
    }

    private float GetCurrentEffectiveCoinStreakWindow()
    {
        float currentSpeed = playerController.CurrentForwardSpeed;
        float dynamicWindow = baseCoinStreakWindow - (currentSpeed * speedToWindowReductionFactor);
        return Mathf.Max(dynamicWindow, minCoinStreakWindow);
    }

    private void HandleCoinCollected(int newCoinTotal, Vector3 collectionPosition)
    {
        UpdateCoinText(newCoinTotal);

        AddScore(baseCoinScoreValue);
        playerController.IncreaseForwardSpeed(speedIncreasePerCoin);

        // --- Centralized Streak Logic ---
        // ... (your existing streak logic) ...

        // --- ADDED: Play Particle Effect ---
        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayCoinEffect(collectionPosition);
        }
        // --- End Added Section ---

        PlayCoinCollectFeedback(); // This is your sound method
    }

    private void AddScore(int amount)
    {
        sessionScore += amount;
        PlayerScore = (int)sessionScore;
        scoreText.text = PlayerScore.ToString() + " |";
    }

    private void PlayCoinCollectFeedback()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCoinCollectSFX();
        }

        // TODO: Implement particle effects, sound effects, UI animations here
    }

    private void UpdateCoinText(int newCoinTotal)
    {
        coinsCollected.text = "| " + newCoinTotal.ToString();
    }

    void Update()
    {
        if (IsGameOver || player == null || !player.activeSelf)
        {
            return;
        }

        // ADDED: Continuous score gain while player is alive and game is not over
        sessionScore += Time.deltaTime * scorePerSecond;
        PlayerScore = (int)sessionScore; // Update display score
        scoreText.text = PlayerScore.ToString() + " |";
    }

    public void PlayerDied()
    {
        if (IsGameOver) return; // Already dead

        IsGameOver = true; // This will stop Update loops
        Time.timeScale = 0f; // Pause the entire game
        SoundManager.Instance.PauseMusic();

        // Show the revive screen
        revivePanel.SetActive(true);
    }

    public void OnWatchAdClicked()
    {
        Debug.Log("Watch Ad Clicked");

        // 1. Hide the Revive Panel immediately so it doesn't block the screen
        revivePanel.SetActive(false);

        // 2. Call ShowRewardedAd with TWO callbacks: Success and Failure
        bool adStarted = AdManager.Instance.ShowRewardedAd(HandleReviveSuccess, HandleAdFailed);

        if (!adStarted)
        {
            // If the ad didn't even start (not loaded), go straight to game over
            HandleAdFailed();
        }
    }

    /// <summary>
    /// This is called by the "NoThanksButton" in your UI.
    /// </summary>
    public void OnNoThanksClicked()
    {
        revivePanel.SetActive(false);
        // Player gave up, show the final game over screen
        ShowFinalGameOver();
    }

    // Callback 1: Ad Watched Successfully
    private void HandleReviveSuccess()
    {
        Debug.Log("Revive Success!");
        revivePanel.SetActive(false); // Ensure it's off

        Time.timeScale = 1f;
        IsGameOver = false;
        SoundManager.Instance.ResumeMusic();

        if (playerController != null)
        {
            playerController.RevivePlayer();
        }
    }

    // Callback 2: Ad Skipped or Failed
    private void HandleAdFailed()
    {
        Debug.Log("Ad Failed or Skipped. Showing Game Over.");
        revivePanel.SetActive(false); // Hide revive panel
        ShowFinalGameOver(); // Show the Score/Game Over panel
    }

    /// <summary>
    /// This is the final end-of-game screen.
    /// </summary>
    private void ShowFinalGameOver()
    {
        // This is your original 'SetGameOverActive' logic
        endScoreText.text = scoreText.text;
        gameOverPanel.SetActive(true);
        // Time.timeScale is already 0, so the game is paused
    }

    private void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnCoinCollected -= HandleCoinCollected;
        }
    }
}