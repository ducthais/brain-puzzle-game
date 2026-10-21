using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    [Header("=== Cấu hình Interstitial ===")]
    public int levelsPerAd = 2; // Hiện quảng cáo sau 1-2 level
    public int deathsPerAd = 9; // Mặc định = GameConfig.DefaultDeathsPerAd (prefab AdManager đang đặt 9)

    [Header("=== AdMob Unit IDs (Giữ nguyên cấu hình đồ án) ===")]
    public string interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
    public string rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";

    [Header("=== UI QUẢNG CÁO ẢO ===")]
    [SerializeField] private GameObject panelFakeAd;
    [SerializeField] private TextMeshProUGUI txtCountdown;
    [SerializeField] private Button btnSkipAd;

    private int _levelsCompletedSinceLastAd = 0;
    private int _deathCountCurrentLevel = 0;
    private Action _onAdCompletedCallback;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ApplyConfig(GameConfigService.Current);
        GameConfigService.Changed += ApplyConfig;

        // Ẩn bảng quảng cáo ảo lúc mới khởi động game
        if (panelFakeAd != null) panelFakeAd.SetActive(false);
    }

    public void OnLevelCompleted()
    {
        if (PremiumManager.Instance != null && PremiumManager.Instance.IsPremium) return;

        _deathCountCurrentLevel = 0;
        _levelsCompletedSinceLastAd++;
        if (_levelsCompletedSinceLastAd >= levelsPerAd)
        {
            _levelsCompletedSinceLastAd = 0;
            TryShowInterstitial("LevelComplete", null);
        }
    }

    private void ApplyConfig(GameConfig config)
    {
        levelsPerAd = config.LevelsPerAd;
        deathsPerAd = config.DeathsPerAd;
    }

    private void OnDestroy()
    {
        GameConfigService.Changed -= ApplyConfig;
        if (Instance == this) Instance = null;
    }

    // ĐÃ NÂNG CẤP: Nhận Callback hành động từ hệ thống PlayerDeath gửi sang
    public void OnPlayerDeath(Action onAdClosedBeforeRespawn)
    {
        Debug.Log($"<color=cyan>[AdManager - KIỂM TRA CHẾT]</color> Hàm được gọi! " +
              $"Số lần chết TRƯỚC KHI TĂNG: {_deathCountCurrentLevel}. Mục tiêu cần đạt: {deathsPerAd}");
        // 1. Kiểm tra đặc quyền Premium, nếu có thì cho hồi sinh ngay lập tức, bỏ qua Ads
        if (PremiumManager.Instance != null && PremiumManager.Instance.IsPremium)
        {
            onAdClosedBeforeRespawn?.Invoke();
            return;
        }

        _deathCountCurrentLevel++;

        // 2. Kiểm tra xem số lần chết đã chạm mốc quy định hay chưa
        if (_deathCountCurrentLevel >= deathsPerAd)
        {
            _deathCountCurrentLevel = 0;
            // Đủ số lần chết -> Kích hoạt quảng cáo ảo và truyền lệnh chờ hồi sinh vào
            TryShowInterstitial("DeathCounter", onAdClosedBeforeRespawn);
        }
        else
        {
            // Chưa đủ số lần chết -> Cho phép nhân vật tải lại màn chơi luôn
            onAdClosedBeforeRespawn?.Invoke();
        }
    }

    public void ShowRewardedAdForHint(Action onGranted, Action onFailed = null)
    {
        if (PremiumManager.Instance != null && PremiumManager.Instance.IsPremium)
        {
            onGranted?.Invoke();
            return;
        }

        _onAdCompletedCallback = onGranted;
        StartCoroutine(PlayFakeAdRoutine(isRewarded: true));
    }

    private void TryShowInterstitial(string trigger, Action onAdClosed)
    {
        Debug.Log($"[AdManager] Kích hoạt quảng cáo Interstitial do: {trigger}");
        _onAdCompletedCallback = onAdClosed; // Gán lệnh chờ thực hiện sau khi bấm Skip
        StartCoroutine(PlayFakeAdRoutine(isRewarded: false));
    }

    private IEnumerator PlayFakeAdRoutine(bool isRewarded)
    {
        if (panelFakeAd == null)
        {
            // Phòng hờ nếu quên chưa kéo UI thì tự kích hoạt luồng game chạy tiếp, không bị kẹt màn hình
            _onAdCompletedCallback?.Invoke();
            yield break;
        }

        // ĐÓNG BĂNG GAME: Tạm dừng thế giới vật lý của game trong lúc xem Ads
        Time.timeScale = 0f;

        panelFakeAd.SetActive(true);
        if (btnSkipAd != null) btnSkipAd.gameObject.SetActive(false);

        string title = isRewarded ? "[Quảng Cáo Nhận Gợi Ý]" : "[Quảng Cáo Hệ Thống]";
        int countdown = 5;

        // Vòng lặp đếm ngược 5 giây thời gian thực (Bất chấp Time.timeScale đang bằng 0)
        while (countdown > 0)
        {
            if (txtCountdown != null) txtCountdown.text = $"{title}\nBỏ qua sau: {countdown}s";
            yield return new WaitForSecondsRealtime(1f);
            countdown--;
        }

        if (txtCountdown != null) txtCountdown.text = title;

        // Hiển thị nút bấm Skip quảng cáo sau khi hết thời gian chờ bắt buộc
        if (btnSkipAd != null)
        {
            btnSkipAd.gameObject.SetActive(true);
            btnSkipAd.onClick.RemoveAllListeners();
            btnSkipAd.onClick.AddListener(() =>
            {
                panelFakeAd.SetActive(false); // Đóng bảng quảng cáo ảo
                Time.timeScale = 1f;          // Trả lại thời gian thực cho game

                // Kích hoạt hành động chờ (Ví dụ: Cấp Hint hoặc Tải lại màn chơi hồi sinh)
                if (_onAdCompletedCallback != null)
                {
                    _onAdCompletedCallback.Invoke();
                    _onAdCompletedCallback = null;
                }
            });
        }
    }
}
