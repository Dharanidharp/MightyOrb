using System;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static AdManager Instance { get; private set; }

    private string gameId = "5983886";
    private string rewardedVideoPlacementId = "Rewarded_iOS";
    private bool testMode = true;

    // Actions for Success (Revive) and Failure (Game Over)
    private Action onAdSuccess;
    private Action onAdFailedOrSkipped;

    private bool isAdLoaded = false;

    // --- FLAGS FOR MAIN THREAD ---
    private bool triggerReward = false;
    private bool triggerFailure = false;
    // -----------------------------

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAds();
        }
    }

    void Update()
    {
        // 1. Handle SUCCESS on Main Thread
        if (triggerReward)
        {
            triggerReward = false;
            Debug.Log("Triggering Reward on Main Thread");
            if (onAdSuccess != null) onAdSuccess.Invoke();
            ResetCallbacks();
            LoadRewardedAd();
        }

        // 2. Handle FAILURE/SKIP on Main Thread
        if (triggerFailure)
        {
            triggerFailure = false;
            Debug.Log("Triggering Failure/Skip on Main Thread");
            if (onAdFailedOrSkipped != null) onAdFailedOrSkipped.Invoke();
            ResetCallbacks();
            LoadRewardedAd();
        }
    }

    private void ResetCallbacks()
    {
        onAdSuccess = null;
        onAdFailedOrSkipped = null;
    }

    public void InitializeAds()
    {
        Advertisement.Initialize(gameId, testMode, this);
    }

    // UPDATED: Now accepts two actions (Success and Failure)
    public bool ShowRewardedAd(Action onSuccess, Action onFailure)
    {
        if (isAdLoaded)
        {
            this.onAdSuccess = onSuccess;
            this.onAdFailedOrSkipped = onFailure;

            Debug.Log("Showing Ad...");
            Advertisement.Show(rewardedVideoPlacementId, this);
            return true;
        }
        else
        {
            Debug.LogWarning("Ad not ready.");
            // If ad isn't ready, trigger failure immediately so the game doesn't get stuck
            onFailure?.Invoke();
            return false;
        }
    }

    private void LoadRewardedAd()
    {
        Debug.Log("Loading next ad...");
        Advertisement.Load(rewardedVideoPlacementId, this);
    }

    // --- Listeners ---

    public void OnInitializationComplete() { LoadRewardedAd(); }
    public void OnInitializationFailed(UnityAdsInitializationError error, string message) { Debug.Log($"Init Failed: {message}"); }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (placementId == rewardedVideoPlacementId) isAdLoaded = true;
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        isAdLoaded = false;
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Ad Show Failed: {message}");
        // Trigger failure logic on Main Thread
        triggerFailure = true;
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PauseMusic();
        isAdLoaded = false;
    }

    public void OnUnityAdsShowClick(string placementId) { }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (placementId == rewardedVideoPlacementId)
        {
            if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
            {
                Debug.Log("Ad Finished. Reward player.");
                triggerReward = true; // Success!
            }
            else
            {
                Debug.Log("Ad Skipped or Unknown.");
                triggerFailure = true; // Failed/Skipped!
            }
        }
    }
}