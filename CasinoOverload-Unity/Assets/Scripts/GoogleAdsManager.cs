// GoogleAdsManager.cs
// Requires: Google Mobile Ads Unity plugin
// Menu: Assets > Google Mobile Ads > Settings  (set your App IDs first)

using System;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds;
using GoogleMobileAds.Api;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-90)]
public class GoogleAdsManager : MonoBehaviour
{
    public static GoogleAdsManager Instance { get; private set; }

    [Header("Rewarded Ad Unit IDs")]
    [Tooltip("Android Rewarded Ad Unit ID from AdMob")]
    [SerializeField] private string androidRewardedId = "ca-app-pub-3940256099942544/5224354917"; // test ID
    [Tooltip("iOS Rewarded Ad Unit ID from AdMob")]
    [SerializeField] private string iosRewardedId = "ca-app-pub-3940256099942544/1712485313";     // test ID

    [Header("Behavior")]
    [SerializeField] private bool initializeOnAwake = true;
    [SerializeField] private bool preloadOnStart = true;

    [Header("Testing (optional)")]
    [Tooltip("Register your device IDs here during development.")]
    [SerializeField] private List<string> testDeviceIds = new List<string>();

    private RewardedAd rewardedAd;
    private bool sdkInitialized;

    public bool IsRewardedReady => rewardedAd != null && rewardedAd.CanShowAd();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (initializeOnAwake)
            Initialize();
    }

    private void OnDestroy()
    {
        rewardedAd?.Destroy();
        rewardedAd = null;
    }

    // -------- Public API --------

    /// <summary>Initialize Google Mobile Ads SDK once (safe to call multiple times).</summary>
    public void Initialize()
    {
        if (sdkInitialized) return;

        // Optional: mark test devices (remove before release).
        if (testDeviceIds != null && testDeviceIds.Count > 0)
        {
            var cfg = new RequestConfiguration { TestDeviceIds = testDeviceIds };
            MobileAds.SetRequestConfiguration(cfg);
        }

        MobileAds.Initialize(initStatus =>
        {
            sdkInitialized = true;
            Debug.Log("[Ads] MobileAds initialized.");
            if (preloadOnStart) LoadRewarded();
        });
    }

    /// <summary>Show a Rewarded Ad. If not ready, it attempts to load and calls onFail.</summary>
    public void ShowRewarded(Action<Reward> onReward, Action<string> onFail = null)
    {
        if (!sdkInitialized) Initialize();

        if (!IsRewardedReady)
        {
            Debug.Log("[Ads] Rewarded not ready. Loading now...");
            LoadRewarded(() => onFail?.Invoke("not_ready"));
            return;
        }

        try
        {
            rewardedAd.Show(reward =>
            {
                // Called when user earns reward
                onReward?.Invoke(reward);
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"[Ads] ShowRewarded exception: {e.Message}");
            onFail?.Invoke("show_exception");
            // Try to have a fresh ad next time
            LoadRewarded();
        }
    }

    // -------- Internal --------

    private string GetRewardedUnitId()
    {
#if UNITY_ANDROID
        return string.IsNullOrEmpty(androidRewardedId) ? "unused" : androidRewardedId;
#elif UNITY_IPHONE
        return string.IsNullOrEmpty(iosRewardedId) ? "unused" : iosRewardedId;
#else
        return "unused"; // Editor uses mock ads; ID is ignored
#endif
    }

    private AdRequest BuildRequest()
    {
        // Newer SDK supports parameterless AdRequest(). Keep it simple.
        return new AdRequest();
    }

    private void LoadRewarded(Action onFailedImmediate = null)
    {
        // Destroy old instance (RewardedAd is one-time use)
        rewardedAd?.Destroy();
        rewardedAd = null;

        var adUnitId = GetRewardedUnitId();
        var request = BuildRequest();

        RewardedAd.Load(adUnitId, request, (ad, loadError) =>
        {
            if (loadError != null)
            {
                Debug.LogWarning($"[Ads] Rewarded load failed: {loadError.GetMessage()}");
                onFailedImmediate?.Invoke();
                return;
            }

            rewardedAd = ad;
            Debug.Log("[Ads] Rewarded loaded.");

            // Hook lifecycle events & auto-preload next one
            rewardedAd.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("[Ads] Rewarded opened.");
            };
            rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("[Ads] Rewarded closed. Preloading next...");
                LoadRewarded();
            };
            rewardedAd.OnAdFullScreenContentFailed += (AdError err) =>
            {
                Debug.LogWarning($"[Ads] Rewarded failed to present: {err}");
                LoadRewarded();
            };
            rewardedAd.OnAdImpressionRecorded += () => { /* optional analytics */ };
            rewardedAd.OnAdPaid += (AdValue v) => { /* optional ILRD */ };
            rewardedAd.OnAdClicked += () => { /* optional */ };
        });
    }
}
