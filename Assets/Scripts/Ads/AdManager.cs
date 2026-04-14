using System;
using System.Collections;
using UnityEngine;
using Unity.Services.LevelPlay;

/// <summary>
/// Quản lý rewarded ads dùng Unity LevelPlay SDK (Ads Mediation package).
/// Trong Unity Editor: tự động bypass và gọi callback ngay.
/// </summary>
public class AdManager : MonoBehaviour
{
    [Header("LevelPlay Credentials")]
    [SerializeField] string appKey          = "YOUR_APP_KEY";
    [SerializeField] string rewardedAdUnitId = "YOUR_REWARDED_AD_UNIT_ID";

    [Header("Debug")]
    [SerializeField] bool verboseLog = true;

    public static AdManager Instance { get; private set; }

    LevelPlayRewardedAd rewardedAd;
    bool sdkInitialized     = false;
    bool adLoaded           = false;
    bool rewardGranted      = false;
    Action pendingCallback;

    // ─── Singleton ────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ─── Init SDK ─────────────────────────────────────────────────────────────

    void Start()
    {
        LevelPlay.OnInitSuccess += OnSDKInitSuccess;
        LevelPlay.OnInitFailed  += OnSDKInitFailed;
        LevelPlay.Init(appKey);
        Log("SDK initializing...");
    }

    void OnSDKInitSuccess(LevelPlayConfiguration cfg)
    {
        sdkInitialized = true;
        Log("SDK initialized. Creating rewarded ad...");

        rewardedAd = new LevelPlayRewardedAd(rewardedAdUnitId);
        rewardedAd.OnAdLoaded        += OnAdLoaded;
        rewardedAd.OnAdLoadFailed    += OnAdLoadFailed;
        rewardedAd.OnAdDisplayed     += OnAdDisplayed;
        rewardedAd.OnAdDisplayFailed += OnAdDisplayFailed;
        rewardedAd.OnAdRewarded      += OnAdRewarded;
        rewardedAd.OnAdClosed        += OnAdClosed;
        rewardedAd.LoadAd();
    }

    void OnSDKInitFailed(LevelPlayInitError err) =>
        Debug.LogWarning($"[AdManager] Init failed: {err.ErrorCode} – {err.ErrorMessage}");

    // ─── Ad callbacks ─────────────────────────────────────────────────────────

    void OnAdLoaded(LevelPlayAdInfo info)      { adLoaded = true; Log($"Ad loaded | {info.AdNetwork}"); }
    void OnAdDisplayed(LevelPlayAdInfo info)   => Log("Ad displayed.");
    void OnAdLoadFailed(LevelPlayAdError err)  { adLoaded = false; StartCoroutine(RetryLoad(10f)); }

    void OnAdDisplayFailed(LevelPlayAdInfo info, LevelPlayAdError err)
    {
        Debug.LogWarning($"[AdManager] Display failed: {err.ErrorCode}");
        pendingCallback = null;
        // GameController uim = FindFirstObjectByType<GameController>();
        // uim?.ShowToast("Lỗi hiển thị quảng cáo. Thử lại!");
        Reload();
    }

    void OnAdRewarded(LevelPlayAdInfo info, LevelPlayReward reward)
    {
        Log($"Rewarded! {reward.Name} x{reward.Amount}");
        if (!rewardGranted)
        {
            rewardGranted = true;
            pendingCallback?.Invoke();
            pendingCallback = null;
        }
    }

    void OnAdClosed(LevelPlayAdInfo info)
    {
        Log("Ad closed.");
        if (!rewardGranted)
        {
            // FindFirstObjectByType<GameController>()?.ShowToast("Xem hết quảng cáo để nhận gợi ý!");
            pendingCallback = null;
        }
        rewardGranted = false;
        Reload();
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    public void ShowRewardedAd(Action onReward)
    {
        pendingCallback = onReward;
        rewardGranted   = false;

#if UNITY_EDITOR
        Log("[Editor] Simulating rewarded ad.");
        onReward?.Invoke();
        pendingCallback = null;
        return;
#endif

        if (!sdkInitialized)
        {
            FindFirstObjectByType<UIGameplay>()?.ShowToast("SDK chưa sẵn sàng. Thử lại!");
            pendingCallback = null;
            return;
        }

        if (adLoaded && rewardedAd.IsAdReady())
            rewardedAd.ShowAd();
        else
        {
            FindFirstObjectByType<UIGameplay>()?.ShowToast("Đang tải quảng cáo... Thử lại sau!");
            pendingCallback = null;
            Reload();
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    void Reload() { adLoaded = false; rewardedAd?.LoadAd(); }

    IEnumerator RetryLoad(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!adLoaded) { Log("Retrying..."); rewardedAd?.LoadAd(); }
    }

    void Log(string msg) { if (verboseLog) Debug.Log($"[AdManager] {msg}"); }

    void OnDestroy()
    {
        LevelPlay.OnInitSuccess -= OnSDKInitSuccess;
        LevelPlay.OnInitFailed  -= OnSDKInitFailed;
        if (rewardedAd == null) return;
        rewardedAd.OnAdLoaded        -= OnAdLoaded;
        rewardedAd.OnAdLoadFailed    -= OnAdLoadFailed;
        rewardedAd.OnAdDisplayed     -= OnAdDisplayed;
        rewardedAd.OnAdDisplayFailed -= OnAdDisplayFailed;
        rewardedAd.OnAdRewarded      -= OnAdRewarded;
        rewardedAd.OnAdClosed        -= OnAdClosed;
    }
}
